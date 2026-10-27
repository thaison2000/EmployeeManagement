using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Data;
using EmployeeManagement.DTO;
using EmployeeManagement.Models;
namespace EmployeeManagement.Repositories;

public sealed class DepartmentRepository(EmployeeDbContext dbContext) : IDepartmentRepository
{
    public async Task<IReadOnlyList<DepartmentResponse>> GetAllAsync(
        CancellationToken cancellationToken) =>
        await dbContext.Departments
            .AsNoTracking()
            .OrderBy(department => department.Id)
            .Select(department => new DepartmentResponse(
                department.Id,
                department.Name,
                department.Employees.Count))
            .ToListAsync(cancellationToken);

    public Task<DepartmentResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken) =>
        dbContext.Departments
            .AsNoTracking()
            .Where(department => department.Id == id)
            .Select(department => new DepartmentResponse(
                department.Id,
                department.Name,
                department.Employees.Count))
            .FirstOrDefaultAsync(cancellationToken);

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken) =>
        dbContext.Departments.AnyAsync(department => department.Id == id, cancellationToken);

    public async Task<DepartmentResponse> CreateAsync(
        CreateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Department name cannot be empty or whitespace.", nameof(request));
        }

        var department = new Department { Name = name };
        dbContext.Departments.Add(department);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new DepartmentResponse(department.Id, department.Name, 0);
    }

    public async Task<DepartmentResponse?> UpdateAsync(
        int id,
        UpdateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        var department = await dbContext.Departments
            .FirstOrDefaultAsync(department => department.Id == id, cancellationToken);
        if (department is null)
        {
            return null;
        }

        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Department name cannot be empty or whitespace.", nameof(request));
        }

        department.Name = name;

        await dbContext.SaveChangesAsync(cancellationToken);

        var employeeCount = await dbContext.Employees
            .CountAsync(employee => employee.DepartmentId == department.Id, cancellationToken);
        return new DepartmentResponse(department.Id, department.Name, employeeCount);
    }

    public async Task<DepartmentDeleteResult> DeleteAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var department = await dbContext.Departments
            .FirstOrDefaultAsync(department => department.Id == id, cancellationToken);
        if (department is null)
        {
            return DepartmentDeleteResult.NotFound;
        }

        if (await dbContext.Employees.AnyAsync(
            employee => employee.DepartmentId == id,
            cancellationToken))
        {
            return DepartmentDeleteResult.HasEmployees;
        }

        dbContext.Departments.Remove(department);
        await dbContext.SaveChangesAsync(cancellationToken);
        return DepartmentDeleteResult.Deleted;
    }
}
