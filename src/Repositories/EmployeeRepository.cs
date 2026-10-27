using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Data;
using EmployeeManagement.DTO;
using EmployeeManagement.Models;

namespace EmployeeManagement.Repositories;

public sealed class EmployeeRepository(EmployeeDbContext dbContext) : IEmployeeRepository
{
    public async Task<IReadOnlyList<EmployeeResponse>> GetAllAsync(
        int? departmentId,
        CancellationToken cancellationToken)
    {
        var employees = dbContext.Employees.AsNoTracking().AsQueryable();
        if (departmentId.HasValue)
        {
            employees = employees.Where(employee => employee.DepartmentId == departmentId.Value);
        }

        return await employees
            .OrderBy(employee => employee.Id)
            .Select(employee => new EmployeeResponse(
                employee.Id,
                employee.Name,
                employee.Phone,
                employee.Position,
                employee.DepartmentId,
                employee.Department!.Name))
            .ToListAsync(cancellationToken);
    }

    public Task<EmployeeResponse?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        dbContext.Employees
            .AsNoTracking()
            .Where(employee => employee.Id == id)
            .Select(employee => new EmployeeResponse(
                employee.Id,
                employee.Name,
                employee.Phone,
                employee.Position,
                employee.DepartmentId,
                employee.Department!.Name))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<EmployeeResponse?> CreateAsync(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var (name, phone, position) = NormalizeFields(
            request.Name,
            request.Phone,
            request.Position);

        var department = await dbContext.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(
                department => department.Id == request.DepartmentId,
                cancellationToken);
        if (department is null)
        {
            return null;
        }

        var employee = new Employee
        {
            Name = name,
            Phone = phone,
            Position = position,
            DepartmentId = department.Id
        };

        dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new EmployeeResponse(
            employee.Id,
            employee.Name,
            employee.Phone,
            employee.Position,
            employee.DepartmentId,
            department.Name);
    }

    public async Task<EmployeeResponse?> UpdateAsync(
        int id,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await (
            from candidate in dbContext.Employees
            join department in dbContext.Departments
                on request.DepartmentId equals department.Id
            where candidate.Id == id
            select new { Employee = candidate, DepartmentName = department.Name })
            .FirstOrDefaultAsync(cancellationToken);
        if (result is null)
        {
            return null;
        }

        var (name, phone, position) = NormalizeFields(
            request.Name,
            request.Phone,
            request.Position);
        var employee = result.Employee;

        employee.Name = name;
        employee.Phone = phone;
        employee.Position = position;
        employee.DepartmentId = request.DepartmentId;

        await dbContext.SaveChangesAsync(cancellationToken);

        return new EmployeeResponse(
            employee.Id,
            employee.Name,
            employee.Phone,
            employee.Position,
            employee.DepartmentId,
            result.DepartmentName);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var employee = await dbContext.Employees
            .FirstOrDefaultAsync(employee => employee.Id == id, cancellationToken);
        if (employee is null)
        {
            return false;
        }

        dbContext.Employees.Remove(employee);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static (string Name, string Phone, string Position) NormalizeFields(
        string name,
        string phone,
        string position)
    {
        name = name.Trim();
        phone = phone.Trim();
        position = position.Trim();
        if (string.IsNullOrWhiteSpace(name)
            || string.IsNullOrWhiteSpace(phone)
            || string.IsNullOrWhiteSpace(position))
        {
            throw new ArgumentException("Employee fields cannot be empty or whitespace.");
        }

        return (name, phone, position);
    }
}
