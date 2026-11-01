namespace Koi.Web.Models;

public record MenuDocument(string Label, string File);
public record MenuCountry(string Name, IReadOnlyList<MenuDocument> Menus);
public record MenuViewModel(IReadOnlyList<MenuDocument> Menus);
