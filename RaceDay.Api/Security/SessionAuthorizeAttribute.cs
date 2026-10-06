using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Api.Security;

public static class SessionKeys
{
    public const string UserId = "UserId";
    public const string Role = "Role";
}

/// <summary>
/// Blocks requests that have no logged-in session (401) or the wrong role (403).
/// Usage: [SessionAuthorize] for any logged-in user, [SessionAuthorize(Roles = "Organiser")] for one role.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SessionAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public string? Roles { get; set; }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var session = context.HttpContext.Session;

        if (session.GetInt32(SessionKeys.UserId) is null)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Please log in first." });
            return;
        }

        if (Roles is not null)
        {
            var allowed = Roles.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (!allowed.Contains(session.GetString(SessionKeys.Role)))
            {
                context.Result = new ObjectResult(new { message = "Your role cannot access this resource." })
                { StatusCode = StatusCodes.Status403Forbidden };
            }
        }
    }
}
