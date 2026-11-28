using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ComponentRouteData = Microsoft.AspNetCore.Components.RouteData;

namespace Koi.Web.Components.Shared;

internal sealed class BlazorViewResult<TComponent>(object? parameters = null)
    : RazorComponentResult<RouteView>(new
    {
        RouteData = new ComponentRouteData(typeof(TComponent), new RouteValueDictionary(parameters))
    }), IActionResult
    where TComponent : IComponent
{
    public Task ExecuteResultAsync(ActionContext context) => ExecuteAsync(context.HttpContext);
}
