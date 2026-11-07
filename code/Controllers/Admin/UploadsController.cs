using Koi.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koi.Web.Controllers.Admin;

[Area("Admin"), Authorize(Roles = "Admin")]
public class UploadsController(ImageUploadService uploads) : Controller
{
    [HttpPost]
    public IActionResult Sign(string folder)
    {
        try
        {
            return Json(uploads.CreateUploadSignature(folder));
        }
        catch (ArgumentException)
        {
            return BadRequest();
        }
    }
}
