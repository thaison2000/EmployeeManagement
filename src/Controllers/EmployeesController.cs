using Microsoft.AspNetCore.Mvc;
using EmployeeManagement.DTO;
using EmployeeManagement.Repositories;

namespace EmployeeManagement.Controllers;

[ApiController]
[Route("api/employees")]
[Produces("application/json")]
public sealed class EmployeesController(IEmployeeRepository employeeRepository) : ControllerBase
{
    /// <summary>Returns all employees, optionally filtered by department.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EmployeeResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EmployeeResponse>>> GetAll(
        [FromQuery] int? departmentId,
        CancellationToken cancellationToken)
    {
        return Ok(await employeeRepository.GetAllAsync(departmentId, cancellationToken));
    }

    /// <summary>Returns an employee by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetByIdAsync(id, cancellationToken);

        return employee is null ? NotFound() : Ok(employee);
    }

    /// <summary>Creates an employee assigned to an existing department.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeResponse>> Create(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.CreateAsync(request, cancellationToken);
        if (employee is null)
        {
            return NotFound(new ProblemDetails { Title = "Department not found." });
        }

        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
    }

    /// <summary>Updates an employee and its department assignment.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeResponse>> Update(
        int id,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.UpdateAsync(id, request, cancellationToken);
        if (employee is not null)
        {
            return Ok(employee);
        }

        var title = await employeeRepository.GetByIdAsync(id, cancellationToken) is null
            ? "Employee not found."
            : "Department not found.";
        return NotFound(new ProblemDetails { Title = title });
    }

    /// <summary>Deletes an employee.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (!await employeeRepository.DeleteAsync(id, cancellationToken))
        {
            return NotFound();
        }

        return NoContent();
    }
}
