using employeesAPI.Data;
using employeesAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace employeesAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeesRepository _repository;

    public EmployeesController(IEmployeesRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Employee>>> GetAsync(CancellationToken cancellationToken)
    {
        var employees = await _repository.GetEmployeesAsync(cancellationToken);
        return Ok(employees);
    }

    [HttpPost]
    public async Task<ActionResult<Employee>> CreateAsync([FromBody] CreateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var employee = await _repository.CreateEmployeeAsync(request, cancellationToken);
        return Created($"/api/employees/{employee.EmployeeId}", employee);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Employee>> UpdateAsync(int id, [FromBody] UpdateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var employee = await _repository.UpdateEmployeeAsync(id, request, cancellationToken);
        if (employee is null)
        {
            return NotFound();
        }

        return Ok(employee);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var deleted = await _repository.DeleteEmployeeAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
