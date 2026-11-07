using System.ComponentModel.DataAnnotations;

namespace Koi.Web.Models;

public class GeographyEditViewModel
{
    public Guid Id { get; set; }
    public int Revision { get; set; }

    [Required, RegularExpression("cities")]
    public string Kind { get; set; } = "cities";

    [Required, StringLength(150)]
    public string Name { get; set; } = "";

    [Range(0, 100000)]
    public int SortOrder { get; set; }

}
