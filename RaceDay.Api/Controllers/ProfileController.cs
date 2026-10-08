using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Security;

namespace RaceDay.Api.Controllers;

[Route("api/profile")]
[SessionAuthorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class ProfileController : ApiControllerBase
{
    private readonly AppDbContext _db;
    public ProfileController(AppDbContext db) => _db = db;

    /// <summary>View your own profile (Organiser or Participant).</summary>
    /// <response code="200">Your profile.</response>
    [HttpGet]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        var user = await _db.Users.FindAsync(CurrentUserId);
        return user is null ? NotFound() : Ok(AuthController.ToResponse(user));
    }

    /// <summary>Update your own name, phone number and date of birth.</summary>
    /// <response code="200">Profile updated.</response>
    /// <response code="400">Validation failed.</response>
    [HttpPut]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(UpdateProfileRequest request)
    {
        var user = await _db.Users.FindAsync(CurrentUserId);
        if (user is null) return NotFound();

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;
        user.DateOfBirth = request.DateOfBirth;
        await _db.SaveChangesAsync();
        return Ok(AuthController.ToResponse(user));
    }
}
