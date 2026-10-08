using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Controllers;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.Api.Security;

namespace RaceDay.Tests;

public class AuthControllerTests
{
    private static RegisterRequest NewRequest(UserRole role, string? code = null) => new()
    {
        FirstName = "Sam", LastName = "Runner", Email = "Sam@Example.com", Password = "Password123",
        DateOfBirth = new DateTime(1995, 5, 5), Role = role, OrganiserInviteCode = code
    };

    [Fact]
    public async Task Register_Participant_StoresHashedPassword()
    {
        using var db = TestHelpers.NewDb();
        var controller = new AuthController(db, TestHelpers.NewConfig()).WithSession();

        var result = await controller.Register(NewRequest(UserRole.Participant));

        Assert.Equal(201, ((ObjectResult)result).StatusCode);
        var saved = db.Users.Single();
        Assert.NotEqual("Password123", saved.PasswordHash);          // never stored in original form
        Assert.True(BCrypt.Net.BCrypt.Verify("Password123", saved.PasswordHash));
        Assert.Equal("sam@example.com", saved.Email);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        using var db = TestHelpers.NewDb();
        var controller = new AuthController(db, TestHelpers.NewConfig()).WithSession();
        await controller.Register(NewRequest(UserRole.Participant));

        var result = await controller.Register(NewRequest(UserRole.Participant));

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Register_Organiser_WithoutInviteCode_IsForbidden()
    {
        using var db = TestHelpers.NewDb();
        var controller = new AuthController(db, TestHelpers.NewConfig()).WithSession();

        var result = await controller.Register(NewRequest(UserRole.Organiser, "WRONG"));

        Assert.Equal(403, ((ObjectResult)result).StatusCode);
        Assert.Empty(db.Users);
    }

    [Fact]
    public async Task Register_Organiser_WithCorrectInviteCode_Succeeds()
    {
        using var db = TestHelpers.NewDb();
        var controller = new AuthController(db, TestHelpers.NewConfig()).WithSession();

        var result = await controller.Register(NewRequest(UserRole.Organiser, "TEST-CODE"));

        Assert.Equal(201, ((ObjectResult)result).StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_SetsSessionIdAndRole()
    {
        using var db = TestHelpers.NewDb();
        var user = db.AddUser("a@b.com", UserRole.Participant);
        var controller = new AuthController(db, TestHelpers.NewConfig()).WithSession();

        var result = await controller.Login(new LoginRequest { Email = "a@b.com", Password = "Password123" });

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(user.Id, controller.HttpContext.Session.GetInt32(SessionKeys.UserId));
        Assert.Equal("Participant", controller.HttpContext.Session.GetString(SessionKeys.Role));
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        using var db = TestHelpers.NewDb();
        db.AddUser("a@b.com", UserRole.Participant);
        var controller = new AuthController(db, TestHelpers.NewConfig()).WithSession();

        var result = await controller.Login(new LoginRequest { Email = "a@b.com", Password = "nope-nope" });

        Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Null(controller.HttpContext.Session.GetInt32(SessionKeys.UserId));
    }

    [Fact]
    public async Task Logout_ClearsSession()
    {
        using var db = TestHelpers.NewDb();
        var controller = new AuthController(db, TestHelpers.NewConfig()).WithSession(5, "Participant");

        controller.Logout();

        Assert.Null(controller.HttpContext.Session.GetInt32(SessionKeys.UserId));
        await Task.CompletedTask;
    }
}
