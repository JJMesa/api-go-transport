using System.Net;
using System.Text.Json;
using Asp.Versioning;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.Vehicle;
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
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;

    public VehiclesController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    //GET: api/v1/vehicle/all
    [HttpGet("all")]
    public async Task<ActionResult<JsonResponse<IEnumerable<VehicleDto>>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var response = await _vehicleService.GetAllAsync(cancellationToken);
        return StatusCode((int)response.HttpCode, response);
    }

    //GET: api/v1/vehicle
    [HttpGet]
    public async Task<ActionResult<JsonResponse<IEnumerable<VehicleDto>>>> GetAsync([FromQuery] VehicleParameters parameters, CancellationToken cancellationToken)
    {
        var response = await _vehicleService.GetAsync(parameters, cancellationToken);
        Response.Headers.Add(Constants.PaginationHeader, JsonSerializer.Serialize(response.Metadata));
        return StatusCode((int)response.HttpCode, response);
    }

    //GET: api/v1/vehicle/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<JsonResponse<VehicleDto>>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var response = await _vehicleService.GetByIdAsync(id, cancellationToken);
        return StatusCode((int)response.HttpCode, response);
    }

    //POST: api/v1/vehicle
    [HttpPost]
    public async Task<ActionResult<JsonResponse<VehicleDto>>> CreateAsync([FromBody] VehicleCreationDto vehicleCreationDto)
    {
        var response = await _vehicleService.CreateAsync(vehicleCreationDto);
        return StatusCode((int)response.HttpCode, response);
    }

    //PUT: api/v1/vehicle/5
    [HttpPut("{id:int}")]
    public async Task<ActionResult<JsonResponse<VehicleDto>>> UpdateAsync(int id, [FromBody] VehicleUpdateDto vehicleUpdateDto)
    {
        var response = await _vehicleService.UpdateAsync(id, vehicleUpdateDto);
        return StatusCode((int)response.HttpCode, response);
    }

    //DELETE: api/v1/vehicle/5
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<JsonResponse<bool?>>> DeleteAsync(int id)
    {
        var response = await _vehicleService.DeleteAsync(id);
        return StatusCode((int)response.HttpCode, response.HttpCode != HttpStatusCode.NoContent ? response : null);
    }
}