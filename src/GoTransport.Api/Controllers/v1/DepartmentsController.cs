using System.Net;
using System.Text.Json;
using Asp.Versioning;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.Department;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Parameters;
using GoTransport.Application.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoTransport.Api.Controllers.v1;

[Authorize(Roles = Roles.Administrator)]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentsController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    // GET: api/v1/departments/all
    [HttpGet("all")]
    public async Task<ActionResult<JsonResponse<IEnumerable<DepartmentDto>>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var response = await _departmentService.GetAllAsync(cancellationToken);
        return StatusCode((int)response.HttpCode, response);
    }

    // GET: api/v1/departments
    [HttpGet]
    public async Task<ActionResult<JsonPagedResponse<IEnumerable<DepartmentDto>>>> GetAsync([FromQuery] DepartmentParameters parameters, CancellationToken cancellationToken)
    {
        var response = await _departmentService.GetAsync(parameters, cancellationToken);
        Response.Headers.Add(Constants.PaginationHeader, JsonSerializer.Serialize(response.Metadata));
        return StatusCode((int)response.HttpCode, response);
    }

    // GET: api/v1/departments/4
    [HttpGet("{id:int}")]
    public async Task<ActionResult<JsonResponse<DepartmentDto>>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var response = await _departmentService.GetByIdAsync(id, cancellationToken);
        return StatusCode((int)response.HttpCode, response);
    }

    // POST: api/v1/departments
    [HttpPost]
    public async Task<ActionResult<JsonResponse<DepartmentDto>>> CreateAsync([FromBody] DepartmentCreationDto departmentCreation)
    {
        var response = await _departmentService.CreateAsync(departmentCreation);
        return StatusCode((int)response.HttpCode, response);
    }

    //PUT: api/v1/departments/4
    [HttpPut("{id:int}")]
    public async Task<ActionResult<JsonResponse<DepartmentDto>>> UpdateAsync(int id, [FromBody] DepartmentUpdateDto departmentUpdate)
    {
        var response = await _departmentService.UpdateAsync(id, departmentUpdate);
        return StatusCode((int)response.HttpCode, response);
    }

    // DELETE: api/v1/departments/4
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<JsonResponse<bool?>>> DeleteAsync(int id)
    {
        var response = await _departmentService.DeleteAsync(id);
        return StatusCode((int)response.HttpCode, response.HttpCode != HttpStatusCode.NoContent ? response : null);
    }
}