using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.DTO;

/// <summary>Payload for creating an employee.</summary>
public sealed record CreateEmployeeRequest : IValidatableObject
{
    [Required, StringLength(150, MinimumLength = 1)]
    public required string Name { get; init; }

    [Required, StringLength(30, MinimumLength = 1)]
    public required string Phone { get; init; }

    [Required, StringLength(100, MinimumLength = 1)]
    public required string Position { get; init; }

    [Range(1, int.MaxValue)]
    public int DepartmentId { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield return new ValidationResult("Name is required.", [nameof(Name)]);
        }

        if (string.IsNullOrWhiteSpace(Phone))
        {
            yield return new ValidationResult("Phone is required.", [nameof(Phone)]);
        }

        if (string.IsNullOrWhiteSpace(Position))
        {
            yield return new ValidationResult("Position is required.", [nameof(Position)]);
        }
    }
}

/// <summary>Payload for updating an employee.</summary>
public sealed record UpdateEmployeeRequest : IValidatableObject
{
    [Required, StringLength(150, MinimumLength = 1)]
    public required string Name { get; init; }

    [Required, StringLength(30, MinimumLength = 1)]
    public required string Phone { get; init; }

    [Required, StringLength(100, MinimumLength = 1)]
    public required string Position { get; init; }

    [Range(1, int.MaxValue)]
    public int DepartmentId { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield return new ValidationResult("Name is required.", [nameof(Name)]);
        }

        if (string.IsNullOrWhiteSpace(Phone))
        {
            yield return new ValidationResult("Phone is required.", [nameof(Phone)]);
        }

        if (string.IsNullOrWhiteSpace(Position))
        {
            yield return new ValidationResult("Position is required.", [nameof(Position)]);
        }
    }
}

/// <summary>An employee returned by the API.</summary>
public sealed record EmployeeResponse(
    int Id,
    string Name,
    string Phone,
    string Position,
    int DepartmentId,
    string DepartmentName);
