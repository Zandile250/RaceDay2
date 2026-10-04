using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class User
{
    public int Id { get; set; }
    [Required, MaxLength(50)] public string FirstName { get; set; } = "";
    [Required, MaxLength(50)] public string LastName { get; set; } = "";
    [Required, MaxLength(150)] public string Email { get; set; } = "";
    [Required] public string PasswordHash { get; set; } = "";
    [MaxLength(20)] public string? PhoneNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public UserRole Role { get; set; }
}

public class Event
{
    public int Id { get; set; }
    [Required, MaxLength(100)] public string Name { get; set; } = "";
    [Required, MaxLength(1000)] public string Description { get; set; } = "";
    public DateTime Date { get; set; }
    [Required, MaxLength(150)] public string Location { get; set; } = "";
    public decimal DistanceKm { get; set; }
    public EventType EventType { get; set; }
    public int OrganiserId { get; set; }
    public User? Organiser { get; set; }
    public List<Category> Categories { get; set; } = new();
}

public class Category
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event? Event { get; set; }
    [Required, MaxLength(60)] public string Name { get; set; } = "";   // e.g. Under 20, Senior, 10km
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
}

public class Enrolment
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event? Event { get; set; }
    public int ParticipantId { get; set; }
    public User? Participant { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public Result? Result { get; set; }
}

public class Result
{
    public int Id { get; set; }
    public int EnrolmentId { get; set; }
    public Enrolment? Enrolment { get; set; }
    public TimeSpan FinishTime { get; set; }
    public int Position { get; set; }
}
