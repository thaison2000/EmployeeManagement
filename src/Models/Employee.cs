namespace EmployeeManagement.Models;

public sealed class Employee
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required string Phone { get; set; }

    public required string Position { get; set; }

    public int DepartmentId { get; set; }

    //public string? Note { get; set; }

    public Department? Department { get; set; }
}
