using ClosedXML.Excel;
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

    [HttpGet("export")]
    public async Task<IActionResult> ExportAsync(CancellationToken cancellationToken)
    {
        var employees = await _repository.GetEmployeesAsync(cancellationToken);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Employees");

        var headers = new[]
        {
            "EmployeeId",
            "FirstName",
            "LastName",
            "Email",
            "HireDate",
            "IsActive",
            "CreatedAt"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
        }

        var row = 2;
        foreach (var employee in employees)
        {
            worksheet.Cell(row, 1).Value = employee.EmployeeId;
            worksheet.Cell(row, 2).Value = employee.FirstName;
            worksheet.Cell(row, 3).Value = employee.LastName;
            worksheet.Cell(row, 4).Value = employee.Email;
            worksheet.Cell(row, 5).Value = employee.HireDate;
            worksheet.Cell(row, 6).Value = employee.IsActive.HasValue ? employee.IsActive.Value : string.Empty;
            worksheet.Cell(row, 7).Value = employee.CreatedAt?.ToString("u") ?? string.Empty;
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var fileName = $"employees_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";

        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
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
