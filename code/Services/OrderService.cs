using System.Text.Json;
using Koi.Web.Data;
using Koi.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Koi.Web.Services;

public class OrderService(ApplicationDbContext database)
{
    private const string CartKey = "shopping-cart";
    public ShoppingCart GetCart(ISession session)
    {
        var value = session.GetString(CartKey);
        return value == null ? new ShoppingCart() : JsonSerializer.Deserialize<ShoppingCart>(value) ?? new ShoppingCart();
    }
    public void SaveCart(ISession session, ShoppingCart cart) => session.SetString(CartKey, JsonSerializer.Serialize(cart));

    public async Task<bool> SelectBranchAsync(ISession session, Guid storeId)
    {
        if (!await BranchAcceptsOrdersAsync(storeId)) return false;
        var cart = GetCart(session);
        if (cart.StoreId != storeId) { cart.StoreId = storeId; cart.OrderId = Guid.NewGuid(); }
        SaveCart(session, cart);
        return true;
    }

    public async Task<(string Status, string Message)> AddAsync(ISession session, Guid id, int quantity, Guid? sizeId, Guid[]? toppingIds, int? sugar, int? ice)
    {
        if (sizeId.HasValue || toppingIds is { Length: > 0 } || sugar.HasValue || ice.HasValue)
            return ("bad", "Các lựa chọn cũ không còn được hỗ trợ. Vui lòng tải lại trang.");
        var cart = GetCart(session);
        if (!await BranchAcceptsOrdersAsync(cart.StoreId)) return ("branch", "Hãy chọn chi nhánh đang nhận đơn.");
        var drink = await GetAvailableDrinkAsync(id);
        if (drink == null || quantity < 1 || quantity > 20) return ("bad", "Đồ uống hoặc lựa chọn không hợp lệ.");
        var existing = cart.Items.Find(line => line.DrinkId == id && line.SizeId == null && line.ToppingIds.Count == 0 && string.IsNullOrEmpty(line.Customization));
        if (existing != null)
        {
            if (existing.Quantity + quantity > 20) return ("quantity", "Mỗi lựa chọn chỉ được đặt tối đa 20 ly.");
            existing.Quantity += quantity;
        }
        else
        {
            if (cart.Items.Count >= 20) return ("full", "Giỏ hàng chỉ hỗ trợ tối đa 20 lựa chọn.");
            cart.Items.Add(new CartItem { DrinkId = drink.Id, Quantity = quantity, Price = drink.Price!.Value });
        }
        SaveCart(session, cart);
        return ("success", "Đã thêm đồ uống vào giỏ hàng.");
    }

    public string Update(ISession session, Guid id, int quantity)
    {
        var cart = GetCart(session);
        var line = cart.Items.Find(value => value.LineId == id);
        if (line == null) return "missing";
        if (quantity < 0 || quantity > 20) return "invalid";
        if (quantity == 0) cart.Items.Remove(line); else line.Quantity = quantity;
        SaveCart(session, cart);
        return "success";
    }

    public void CompleteCheckout(ISession session, Guid orderId)
    {
        session.Remove(CartKey);
        session.SetString("order-reference", orderId.ToString());
    }

    public string? TakeOrderReference(ISession session)
    {
        var reference = session.GetString("order-reference");
        session.Remove("order-reference");
        return reference;
    }

    public Task<bool> BranchAcceptsOrdersAsync(Guid id) =>
        database.Stores.AnyAsync(value => value.Id == id && value.IsPublished && value.AcceptsOrders);

    public Task<Drink?> GetAvailableDrinkAsync(Guid id) =>
        database.Drinks.AsNoTracking().SingleOrDefaultAsync(value => value.Id == id && value.IsPublished && value.Category.IsPublished && value.Price > 0);

    public async Task<bool> CheckoutAsync(ShoppingCart cart, CheckoutViewModel checkout, ModelStateDictionary modelState)
    {
        if (!Enum.IsDefined(checkout.Fulfillment)) modelState.AddModelError("Checkout.Fulfillment", "Chọn cách nhận hàng hợp lệ.");
        if (checkout.Fulfillment == FulfillmentMethod.Delivery && string.IsNullOrWhiteSpace(checkout.DeliveryAddress)) modelState.AddModelError("Checkout.DeliveryAddress", "Nhập địa chỉ giao hàng.");
        var branch = await database.Stores.AsNoTracking().SingleOrDefaultAsync(value => value.Id == cart.StoreId && value.IsPublished && value.AcceptsOrders);
        if (branch == null) modelState.AddModelError("", "Chi nhánh không còn nhận đơn. Vui lòng chọn chi nhánh khác.");
        if (cart.Items.Count == 0) modelState.AddModelError("", "Giỏ hàng đang trống.");
        var ids = cart.Items.Select(value => value.DrinkId).ToArray();
        var drinks = await database.Drinks.AsNoTracking().Where(value => ids.Contains(value.Id) && value.IsPublished && value.Category.IsPublished && value.Price > 0).ToDictionaryAsync(value => value.Id);
        if (drinks.Count != ids.Distinct().Count()) modelState.AddModelError("", "Có món không còn được bán. Vui lòng xóa món đó khỏi giỏ hàng.");
        var changed = false;
        foreach (var line in cart.Items)
        {
            if (line.SizeId.HasValue || line.ToppingIds.Count > 0 || !string.IsNullOrEmpty(line.Customization))
            {
                modelState.AddModelError("", "Các lựa chọn cũ không còn được hỗ trợ. Vui lòng xóa món cũ và chọn lại.");
                continue;
            }
            if (drinks.TryGetValue(line.DrinkId, out var drink))
            {
                var price = drink.Price!.Value;
                if (price != line.Price) { line.Price = price; changed = true; }
            }
        }
        if (changed) { modelState.AddModelError("", "Giá đã thay đổi. Vui lòng kiểm tra lại giỏ hàng và xác nhận đặt hàng lần nữa."); }
        if (!modelState.IsValid) return false;
        var order = new Order
        {
            Id = cart.OrderId, StoreId = branch!.Id, StoreName = branch.Name, StoreAddress = branch.Address,
            CustomerName = checkout.CustomerName.Trim(), CustomerPhone = checkout.CustomerPhone.Trim(),
            Fulfillment = checkout.Fulfillment, DeliveryAddress = checkout.Fulfillment == FulfillmentMethod.Delivery ? checkout.DeliveryAddress!.Trim() : "",
            Note = checkout.Note?.Trim() ?? "",
            Items = cart.Items.Select(line => new OrderItem { DrinkId = line.DrinkId, DrinkName = drinks[line.DrinkId].Name, Customization = line.Customization, UnitPrice = line.Price, Quantity = line.Quantity }).ToList()
        };
        if (!await database.Orders.AnyAsync(value => value.Id == order.Id))
        {
            database.Orders.Add(order);
            try { await database.SaveChangesAsync(); }
            catch (DbUpdateException)
            {
                database.Entry(order).State = EntityState.Detached;
                if (!await database.Orders.AsNoTracking().AnyAsync(value => value.Id == order.Id)) throw;
            }
        }
        return true;
    }

    public async Task<OrderingPageViewModel> GetPageAsync(ShoppingCart cart, Guid? categoryId = null, Guid? drinkId = null, int page = 1)
    {
        var branches = await database.Stores.AsNoTracking().Where(value => value.IsPublished && value.AcceptsOrders).OrderBy(value => value.Name).ToArrayAsync();
        var query = database.Drinks.AsNoTracking().Include(value => value.Category).Where(value => value.IsPublished && value.Category.IsPublished);
        if (drinkId.HasValue) query = query.Where(value => value.Id == drinkId);
        else if (categoryId.HasValue) query = query.Where(value => value.CategoryId == categoryId);
        const int pageSize = 12;
        var drinkCount = await query.CountAsync();
        var pageCount = Math.Max(1, (int)Math.Ceiling(drinkCount / (double)pageSize));
        page = Math.Clamp(page, 1, pageCount);
        var drinks = await query.OrderBy(value => value.Category.SortOrder).ThenBy(value => value.CategoryId)
            .ThenBy(value => value.SortOrder).ThenBy(value => value.Id)
            .Skip((page - 1) * pageSize).Take(pageSize).ToArrayAsync();
        var categories = await database.Categories.AsNoTracking().Where(value => value.IsPublished && value.Drinks.Any(drink => drink.IsPublished))
            .OrderBy(value => value.SortOrder).ThenBy(value => value.Id).ToArrayAsync();
        var filterName = drinkId.HasValue ? null : categoryId.HasValue
            ? await database.Categories.AsNoTracking().Where(value => value.Id == categoryId && value.IsPublished).Select(value => value.Name).SingleOrDefaultAsync() : null;
        var ids = cart.Items.Select(line => line.DrinkId).ToArray();
        var allCartDrinks = await database.Drinks.AsNoTracking().Where(value => ids.Contains(value.Id)).ToDictionaryAsync(value => value.Id);
        return new OrderingPageViewModel
        {
            Branches = branches, Branch = branches.FirstOrDefault(value => value.Id == cart.StoreId), Drinks = drinks, Cart = cart,
            CategoryId = categoryId, DrinkId = drinkId, FilterName = filterName, Categories = categories,
            Page = page, DrinkCount = drinkCount, HasMoreDrinks = page < pageCount,
            Lines = cart.Items.Select(line => new CartLine(line.LineId, allCartDrinks.TryGetValue(line.DrinkId, out var d) ? (d.Name) : "Món không còn tồn tại", line.Customization, line.Quantity, line.Price)).ToList()
        };
    }
}
