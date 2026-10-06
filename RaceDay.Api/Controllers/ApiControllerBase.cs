using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Security;

namespace RaceDay.Api.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Id of the user stored in the session at login.</summary>
    protected int CurrentUserId => HttpContext.Session.GetInt32(SessionKeys.UserId) ?? 0;

    protected IActionResult Forbidden(string message) =>
        StatusCode(StatusCodes.Status403Forbidden, new { message });
}
