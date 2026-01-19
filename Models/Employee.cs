using System.ComponentModel.DataAnnotations;

namespace employeesAPI.Models;

public sealed class Employee
{
    public int EmployeeId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public DateTime HireDate { get; init; }
    public bool? IsActive { get; init; }
    public DateTime? CreatedAt { get; init; }
}

public sealed class CreateEmployeeRequest
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; init; } = string.Empty;

    [EmailAddress]
    [StringLength(150)]
    public string? Email { get; init; }

    [Required]
    public DateTime HireDate { get; init; }

    public bool? IsActive { get; init; }

    public DateTime? CreatedAt { get; init; }

    [Required]
    [StringLength(200, MinimumLength = 8)]
    public string Password { get; init; } = string.Empty;
}

public sealed class UpdateEmployeeRequest
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; init; } = string.Empty;

    [EmailAddress]
    [StringLength(150)]
    public string? Email { get; init; }

    [Required]
    public DateTime HireDate { get; init; }

    public bool? IsActive { get; init; }

    public DateTime? CreatedAt { get; init; }

    [StringLength(200, MinimumLength = 8)]
    public string? Password { get; init; }
}
