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
