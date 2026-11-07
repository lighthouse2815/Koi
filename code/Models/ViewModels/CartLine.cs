namespace Koi.Web.Models;

public record CartLine(Guid LineId, string Name, string Customization, int Quantity, decimal Price);
