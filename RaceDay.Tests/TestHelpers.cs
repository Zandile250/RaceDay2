using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Api.Security;

namespace RaceDay.Tests;

/// <summary>Minimal in-memory session so controllers can be tested without a web server.</summary>
public class TestSession : ISession
{
    private readonly Dictionary<string, byte[]> _store = new();
    public bool IsAvailable => true;
    public string Id { get; } = Guid.NewGuid().ToString();
    public IEnumerable<string> Keys => _store.Keys;
    public void Clear() => _store.Clear();
    public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task LoadAsync(CancellationToken ct = default) => Task.CompletedTask;
    public void Remove(string key) => _store.Remove(key);
    public void Set(string key, byte[] value) => _store[key] = value;
    public bool TryGetValue(string key, [NotNullWhen(true)] out byte[]? value) => _store.TryGetValue(key, out value);
}

public static class TestHelpers
{
    public static AppDbContext NewDb() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    public static IConfiguration NewConfig() =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Organiser:InviteCode"] = "TEST-CODE"
        }).Build();

    public static HttpContext NewHttpContext(int? userId = null, string? role = null)
    {
        var ctx = new DefaultHttpContext { Session = new TestSession() };
        if (userId is not null) ctx.Session.SetInt32(SessionKeys.UserId, userId.Value);
        if (role is not null) ctx.Session.SetString(SessionKeys.Role, role);
        return ctx;
    }

    public static T WithSession<T>(this T controller, int? userId = null, string? role = null) where T : ControllerBase
    {
        controller.ControllerContext = new ControllerContext { HttpContext = NewHttpContext(userId, role) };
        return controller;
    }

    public static User AddUser(this AppDbContext db, string email, UserRole role)
    {
        var u = new User
        {
            FirstName = "Test", LastName = role.ToString(), Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
            DateOfBirth = new DateTime(2000, 1, 1), Role = role
        };
        db.Users.Add(u);
        db.SaveChanges();
        return u;
    }
}
