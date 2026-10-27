using Microsoft.AspNetCore.Mvc;
using EmployeeManagement.DTO;
using EmployeeManagement.Repositories;

namespace EmployeeManagement.Controllers;

[ApiController]
[Route("api/departments")]
[Produces("application/json")]
public sealed class DepartmentsController(IDepartmentRepository departmentRepository) : ControllerBase
{
    /// <summary>Returns all departments with employee counts.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DepartmentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DepartmentResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        return Ok(await departmentRepository.GetAllAsync(cancellationToken));
    }

    /// <summary>Returns a department by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DepartmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DepartmentResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var department = await departmentRepository.GetByIdAsync(id, cancellationToken);

        return department is null ? NotFound() : Ok(department);
    }

    /// <summary>Creates a department.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(DepartmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DepartmentResponse>> Create(
        CreateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        var response = await departmentRepository.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>Updates a department.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(DepartmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DepartmentResponse>> Update(
        int id,
        UpdateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        var department = await departmentRepository.UpdateAsync(id, request, cancellationToken);
        if (department is null)
        {
            return NotFound();
        }

        return Ok(department);
    }

    /// <summary>Deletes a department that has no employees.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await departmentRepository.DeleteAsync(id, cancellationToken);
        if (result == DepartmentDeleteResult.NotFound)
        {
            return NotFound();
        }

        if (result == DepartmentDeleteResult.HasEmployees)
        {
            return Conflict(new ProblemDetails { Title = "Department still has employees." });
        }

        return NoContent();
    }
}
