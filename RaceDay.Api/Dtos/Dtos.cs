using System.ComponentModel.DataAnnotations;
using RaceDay.Api.Models;

namespace RaceDay.Api.Dtos;

public class RegisterRequest
{
    [Required, MaxLength(50)] public string FirstName { get; set; } = "";
    [Required, MaxLength(50)] public string LastName { get; set; } = "";
    [Required, EmailAddress, MaxLength(150)] public string Email { get; set; } = "";
    [Required, MinLength(8)] public string Password { get; set; } = "";
    [Phone] public string? PhoneNumber { get; set; }
    [Required] public DateTime DateOfBirth { get; set; }
    [Required] public UserRole Role { get; set; }
    /// <summary>Required only when registering as an Organiser.</summary>
    public string? OrganiserInviteCode { get; set; }
}

public class LoginRequest
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required] public string Password { get; set; } = "";
}

public record UserResponse(int Id, string FirstName, string LastName, string Email,
    string? PhoneNumber, DateTime DateOfBirth, UserRole Role);

public class UpdateProfileRequest
{
    [Required, MaxLength(50)] public string FirstName { get; set; } = "";
    [Required, MaxLength(50)] public string LastName { get; set; } = "";
    [Phone] public string? PhoneNumber { get; set; }
    [Required] public DateTime DateOfBirth { get; set; }
}

public class EventRequest
{
    [Required, MaxLength(100)] public string Name { get; set; } = "";
    [Required, MaxLength(1000)] public string Description { get; set; } = "";
    [Required] public DateTime Date { get; set; }
    [Required, MaxLength(150)] public string Location { get; set; } = "";
    [Range(0.1, 1000)] public decimal DistanceKm { get; set; }
    [Required] public EventType EventType { get; set; }
}

public record EventResponse(int Id, string Name, string Description, DateTime Date,
    string Location, decimal DistanceKm, EventType EventType, int OrganiserId);

public class CategoryRequest
{
    [Required, MaxLength(60)] public string Name { get; set; } = "";
    [Range(0, 120)] public int? MinAge { get; set; }
    [Range(0, 120)] public int? MaxAge { get; set; }
}

public record CategoryResponse(int Id, int EventId, string Name, int? MinAge, int? MaxAge);

public class EnrolRequest
{
    [Required] public int EventId { get; set; }
    [Required] public int CategoryId { get; set; }
}

public record EnrolmentResponse(int Id, int EventId, string EventName, int ParticipantId,
    string ParticipantName, int CategoryId, string CategoryName, DateTime EnrolledAt);

public class CaptureResultRequest
{
    [Required] public int EnrolmentId { get; set; }
    [Required] public TimeSpan FinishTime { get; set; }
    [Range(1, 100000)] public int Position { get; set; }
}

public record ResultResponse(int Id, int EnrolmentId, int EventId, string EventName,
    string ParticipantName, string CategoryName, TimeSpan FinishTime, int Position);
