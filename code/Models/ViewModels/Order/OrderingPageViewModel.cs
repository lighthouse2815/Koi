namespace Koi.Web.Models;

public class OrderingPageViewModel
{
    public List<Store> Branches { get; set; } = [];
    public Guid? CategoryId { get; set; }
    public Guid? DrinkId { get; set; }
    public string? Search { get; set; }
    public string? FilterName { get; set; }
    public List<Category> Categories { get; set; } = [];
    public List<Drink> Drinks { get; set; } = [];
    public int Page { get; set; } = 1;
    public int DrinkCount { get; set; }
    public bool HasMoreDrinks { get; set; }
    public CheckoutViewModel Checkout { get; set; } = new();
}
