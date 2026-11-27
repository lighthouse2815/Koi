using Koi.Web.Data;
using Koi.Web.Exceptions;
using Koi.Web.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Koi.Web.Services;

public class OrderService(
    ApplicationDbContext database,
    StoreService storeService
)
{
    public async Task<OrderListViewModel> GetAdminListAsync(Guid? storeId, OrderStatus? status, int page)
    {
        var query = database.Orders.AsNoTracking().Include(order => order.Items).AsQueryable();
        if (storeId.HasValue)
            query = query.Where(order => order.StoreId == storeId);
        if (status.HasValue)
            query = query.Where(order => order.Status == status);

        const int pageSize = 25;
        var pageCount = Math.Max(1, (int)Math.Ceiling(await query.CountAsync() / (double)pageSize));
        page = Math.Clamp(page, 1, pageCount);

        var branches = await database.Stores.AsNoTracking()
            .Where(store => store.AcceptsOrders || database.Orders.Any(order => order.StoreId == store.Id))
            .OrderBy(store => store.Name)
            .ToListAsync();

        return new OrderListViewModel
        {
            Orders = await query.OrderByDescending(order => order.CreatedAt)
                .ThenBy(order => order.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(),
            Branches = branches,
            StoreId = storeId,
            Status = status,
            Page = page,
            PageCount = pageCount
        };
    }

    public Task<Order?> GetDetailsAsync(Guid id) =>
        database.Orders.AsNoTracking()
            .Include(order => order.Items)
            .SingleOrDefaultAsync(order => order.Id == id);

    public async Task<bool> ChangeStatusAsync(Guid id, OrderStatus current, OrderStatus next)
    {
        var valid = current == OrderStatus.New && (next == OrderStatus.Preparing || next == OrderStatus.Cancelled)
            || current == OrderStatus.Preparing && (next == OrderStatus.Completed || next == OrderStatus.Cancelled);
        if (!valid)
            throw new AppException(ErrorCode.INVALID_INPUT with { Message = "Chuyển trạng thái không hợp lệ." });

        var updated = await database.Orders.Where(order => order.Id == id && order.Status == current)
            .ExecuteUpdateAsync(setters => setters.SetProperty(order => order.Status, next));

        return updated == 1;
    }

    public Task<List<Order>> GetHistoryAsync(Guid customerId) =>
        database.Orders
            .AsNoTracking()
            .Where(order => order.CustomerId == customerId)
            .Include(order => order.Items)
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync();

    public Task<List<CartLine>> GetCartLinesAsync(Guid[] drinkIds) =>
        database.Drinks.AsNoTracking()
            .Where(drink => drinkIds.Contains(drink.Id) && drink.Price > 0)
            .Select(drink => new CartLine(drink.Id, drink.Name, drink.Price!.Value))
            .ToListAsync();

    public async Task<Guid> CheckoutAsync(CheckoutViewModel checkout, Guid customerId)
    {
        if (checkout.Items.Count == 0)
            throw new AppException(ErrorCode.ORDER_CART_EMPTY);

        var drinkIds = checkout.Items.Select(item => item.DrinkId).Distinct().ToArray();
        if (checkout.OrderId == Guid.Empty || checkout.Items.Count > 20 || drinkIds.Length != checkout.Items.Count)
            throw new AppException(ErrorCode.ORDER_CART_INVALID);

        var previousOrder = await database.Orders
            .Where(order => order.Id == checkout.OrderId)
            .Select(order => new { order.CustomerId })
            .SingleOrDefaultAsync();
        if (previousOrder != null)
        {
            if (previousOrder.CustomerId != customerId)
                throw new AppException(ErrorCode.DATA_CONFLICT);
            return checkout.OrderId;
        }

        var errors = new List<(string Field, ErrorCode Error)>();
        if (!Enum.IsDefined(checkout.Fulfillment))
            errors.Add(("Checkout.Fulfillment", ErrorCode.ORDER_FULFILLMENT_INVALID));
        if (checkout.Fulfillment == FulfillmentMethod.Delivery && string.IsNullOrWhiteSpace(checkout.DeliveryAddress))
            errors.Add(("Checkout.DeliveryAddress", ErrorCode.ORDER_ADDRESS_REQUIRED));

        var store = await storeService.GetStoreAcceptingOrdersAsync(checkout.StoreId);
        if (store == null)
            errors.Add(("", ErrorCode.ORDER_BRANCH_UNAVAILABLE));

        var drinks = (await GetCartLinesAsync(drinkIds)).ToDictionary(drink => drink.DrinkId);
        if (drinks.Count != drinkIds.Length)
            errors.Add(("", ErrorCode.ORDER_DRINK_UNAVAILABLE));
        if (checkout.Items.Any(item => drinks.TryGetValue(item.DrinkId, out var drink) && item.Price != drink.Price))
            errors.Add(("", ErrorCode.ORDER_PRICE_CHANGED));
        if (errors.Count > 0)
            throw new CheckoutException(errors);

        var order = new Order
        {
            Id = checkout.OrderId,
            CustomerId = customerId,
            StoreId = store!.Id,
            StoreName = store.Name,
            StoreAddress = store.Address,
            CustomerName = checkout.CustomerName.Trim(),
            CustomerPhone = checkout.CustomerPhone.Trim(),
            Fulfillment = checkout.Fulfillment,
            DeliveryAddress = checkout.Fulfillment == FulfillmentMethod.Delivery ? checkout.DeliveryAddress!.Trim() : "",
            Note = checkout.Note?.Trim() ?? "",
            Items = checkout.Items.Select(item => new OrderItem
            {
                DrinkId = item.DrinkId,
                DrinkName = drinks[item.DrinkId].Name,
                UnitPrice = drinks[item.DrinkId].Price,
                Quantity = item.Quantity
            }).ToList()
        };

        database.Orders.Add(order);
        try
        {
            await database.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: "PK_Orders"
        })
        {
            if (!await database.Orders.AnyAsync(existing => existing.Id == order.Id && existing.CustomerId == customerId))
                throw new AppException(ErrorCode.DATA_CONFLICT);
        }

        return order.Id;
    }

    public async Task<OrderingPageViewModel> GetPageAsync(
        Guid? categoryId = null,
        Guid? drinkId = null,
        int page = 1,
        string? search = null
    ){
        search = search?.Trim();

        var stores = await storeService.GetStoresAcceptingOrdersAsync();

        var query = database.Drinks.AsNoTracking();
        if (drinkId.HasValue) query = query.Where(value => value.Id == drinkId);
        else if (categoryId.HasValue) query = query.Where(value => value.CategoryId == categoryId);
        if (!string.IsNullOrEmpty(search))
        {
            var keyword = search.ToLowerInvariant();
            query = query.Where(value => value.Name.ToLower().Contains(keyword));
        }
        const int pageSize = 12;
        var drinkCount = await query.CountAsync();
        var pageCount = Math.Max(1, (int)Math.Ceiling(drinkCount / (double)pageSize));
        page = Math.Clamp(page, 1, pageCount);
        var drinks = await query
            .Include(value => value.Category)
            .OrderBy(value => value.Category.SortOrder)
            .ThenBy(value => value.CategoryId)
            .ThenBy(value => value.SortOrder)
            .ThenBy(value => value.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        var categories = await database.Categories.AsNoTracking()
            .Where(value => value.Drinks.Any())
            .OrderBy(value => value.SortOrder)
            .ThenBy(value => value.Id)
            .ToListAsync();
        var filterName = drinkId.HasValue || !categoryId.HasValue
            ? null
            : categories.Find(category => category.Id == categoryId)?.Name
                ?? await database.Categories
                    .Where(category => category.Id == categoryId)
                    .Select(category => category.Name)
                    .SingleOrDefaultAsync();
        return new OrderingPageViewModel
        {
            Stores = stores,
            Drinks = drinks,
            CategoryId = categoryId,
            DrinkId = drinkId,
            Search = search,
            FilterName = filterName,
            Categories = categories,
            Page = page,
            DrinkCount = drinkCount,
            HasMoreDrinks = page < pageCount
        };
    }
}
