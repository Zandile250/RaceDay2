using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using RaceDay.Api.Security;

namespace RaceDay.Tests;

public class SessionAuthorizeTests
{
    private static AuthorizationFilterContext Context(int? userId, string? role) =>
        new(new ActionContext(TestHelpers.NewHttpContext(userId, role), new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>());

    [Fact]
    public void NoSession_Returns401()
    {
        var ctx = Context(null, null);
        new SessionAuthorizeAttribute { Roles = "Organiser" }.OnAuthorization(ctx);
        Assert.IsType<UnauthorizedObjectResult>(ctx.Result);
    }

    [Fact]
    public void ParticipantOnOrganiserRoute_Returns403()
    {
        var ctx = Context(1, "Participant");
        new SessionAuthorizeAttribute { Roles = "Organiser" }.OnAuthorization(ctx);
        Assert.Equal(403, ((ObjectResult)ctx.Result!).StatusCode);
    }

    [Fact]
    public void OrganiserOnParticipantRoute_Returns403()
    {
        var ctx = Context(1, "Organiser");
        new SessionAuthorizeAttribute { Roles = "Participant" }.OnAuthorization(ctx);
        Assert.Equal(403, ((ObjectResult)ctx.Result!).StatusCode);
    }

    [Fact]
    public void CorrectRole_IsAllowed()
    {
        var ctx = Context(1, "Organiser");
        new SessionAuthorizeAttribute { Roles = "Organiser" }.OnAuthorization(ctx);
        Assert.Null(ctx.Result);
    }

    [Fact]
    public void AnyLoggedInUser_AllowedWhenNoRoleRequired()
    {
        var ctx = Context(1, "Participant");
        new SessionAuthorizeAttribute().OnAuthorization(ctx);
        Assert.Null(ctx.Result);
    }
}
