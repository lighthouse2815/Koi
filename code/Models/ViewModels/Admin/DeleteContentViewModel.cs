namespace Koi.Web.Models;

public record DeleteContentViewModel(Guid Id, string Name, string? Kind = null);
