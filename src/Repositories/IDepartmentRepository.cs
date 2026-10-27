using EmployeeManagement.DTO;

namespace EmployeeManagement.Repositories;

public interface IDepartmentRepository
{
    Task<IReadOnlyList<DepartmentResponse>> GetAllAsync(CancellationToken cancellationToken);

    Task<DepartmentResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);

    Task<DepartmentResponse> CreateAsync(
        CreateDepartmentRequest request,
        CancellationToken cancellationToken);

    Task<DepartmentResponse?> UpdateAsync(
        int id,
        UpdateDepartmentRequest request,
        CancellationToken cancellationToken);

    Task<DepartmentDeleteResult> DeleteAsync(int id, CancellationToken cancellationToken);
}

public enum DepartmentDeleteResult
{
    Deleted,
    NotFound,
    HasEmployees
}
