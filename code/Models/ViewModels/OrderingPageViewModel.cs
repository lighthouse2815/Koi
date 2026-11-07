namespace Koi.Web.Models;

public class OrderingPageViewModel
{
    public Store[] Branches { get; set; } = [];
    public Guid? CategoryId { get; set; }
    public Guid? DrinkId { get; set; }
    public string? FilterName { get; set; }
    public Category[] Categories { get; set; } = [];
    public Drink[] Drinks { get; set; } = [];
    public int Page { get; set; } = 1;
    public int DrinkCount { get; set; }
    public bool HasMoreDrinks { get; set; }
    public ShoppingCart Cart { get; set; } = new();
    public Store? Branch { get; set; }
    public List<CartLine> Lines { get; set; } = [];
    public CheckoutViewModel Checkout { get; set; } = new();
    public decimal Total => Lines.Sum(value => value.Price * value.Quantity);
}
