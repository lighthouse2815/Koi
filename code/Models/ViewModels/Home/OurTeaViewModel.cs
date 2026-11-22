namespace Koi.Web.Models;

public record OurTeaViewModel(IReadOnlyList<Category> Categories, IReadOnlyList<FeaturedDrink> Featured);
