using EmployeeManagement.DTO;

namespace EmployeeManagement.Repositories;

public interface IEmployeeRepository
{
    Task<IReadOnlyList<EmployeeResponse>> GetAllAsync(
        int? departmentId,
        CancellationToken cancellationToken);

    Task<EmployeeResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<EmployeeResponse?> CreateAsync(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken);

    Task<EmployeeResponse?> UpdateAsync(
        int id,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
