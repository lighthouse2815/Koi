namespace Koi.Web.Models;

public record CartLine(Guid LineId, string Name, int Quantity, decimal Price);
