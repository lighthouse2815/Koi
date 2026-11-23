using Koi.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koi.Web.Controllers.Admin;

[Area("Admin"), Authorize(Roles = "Admin")]
public class UploadsController(ImageUploadService uploads) : Controller
{
    [HttpPost]
    public IActionResult Sign(string folder) => Json(uploads.CreateUploadSignature(folder));
}
