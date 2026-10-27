using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.DTO;

/// <summary>Payload for creating a department.</summary>
public sealed record CreateDepartmentRequest : IValidatableObject
{
    [Required, StringLength(100, MinimumLength = 1)]
    public required string Name { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield return new ValidationResult("Name is required.", [nameof(Name)]);
        }
    }
}

/// <summary>Payload for updating a department.</summary>
public sealed record UpdateDepartmentRequest : IValidatableObject
{
    [Required, StringLength(100, MinimumLength = 1)]
    public required string Name { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield return new ValidationResult("Name is required.", [nameof(Name)]);
        }
    }
}

/// <summary>A department returned by the API.</summary>
public sealed record DepartmentResponse(int Id, string Name, int EmployeeCount);
