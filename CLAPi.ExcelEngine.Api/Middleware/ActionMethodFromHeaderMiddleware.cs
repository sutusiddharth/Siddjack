using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace CLAPi.ExcelEngine.Middleware;
public class ActionMethodFromHeaderMiddleware(RequestDelegate next, string headerName = "X-Action-Method")
{
    private readonly RequestDelegate _next = next;
    private readonly string _headerName = headerName;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(_headerName, out var actionMethodName))
        {
            // Check if the action method name exists and set it in the RouteData
            var routeData = context.GetRouteData();
            if (routeData != null)
            {
                context.Items["ActionMethod"] = actionMethodName.ToString();
            }
        }

        await _next(context);
    }
}
public class ActionFromHeaderAttribute : ActionMethodSelectorAttribute
{
    public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action)
    {
        if (routeContext.HttpContext.Items.TryGetValue("ActionMethod", out var actionMethodName) && action is ControllerActionDescriptor controllerActionDescriptor)
        {
            return string.Equals(controllerActionDescriptor.ActionName, actionMethodName as string, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }
}
