namespace Koi.Web.Models;

public record MenuDocument(string Label, string File);
public record MenuViewModel(IReadOnlyList<MenuDocument> Menus);
