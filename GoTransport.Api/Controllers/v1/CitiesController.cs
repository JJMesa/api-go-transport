using System.Net;
using System.Text.Json;
using Asp.Versioning;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.City;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Parameters;
using GoTransport.Application.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoTransport.Api.Controllers.v1;

[Authorize(Roles = Roles.Administrator)]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class CitiesController : ControllerBase
{
    private readonly ICityService _cityService;

    public CitiesController(ICityService cityService)
    {
        _cityService = cityService;
    }

    //GET: api/departments/2/cities-all
    [HttpGet("/api/v{version:apiVersion}/departments/{departmentId:int}/[controller]")]
    public async Task<ActionResult<JsonResponse<IEnumerable<CityDto>>>> GetAllByDepartmentAsync(int departmentId, CancellationToken cancellationToken)
    {
        var response = await _cityService.GetAllByDepartmentAsync(departmentId, cancellationToken);
        return StatusCode((int)response.HttpCode, response);
    }

    //GET: api/cities
    [HttpGet]
    public async Task<ActionResult<JsonPagedResponse<IEnumerable<CityDto>>>> GetAsync([FromQuery] CityParameters parameters, CancellationToken cancellationToken)
    {
        var response = await _cityService.GetAsync(parameters, cancellationToken);
        Response.Headers.Add(Constants.PaginationHeader, JsonSerializer.Serialize(response.Metadata));
        return StatusCode((int)response.HttpCode, response);
    }

    //GET: api/cities/4
    [HttpGet("{id:int}")]
    public async Task<ActionResult<JsonResponse<CityDto>>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var response = await _cityService.GetByIdAsync(id, cancellationToken);
        return StatusCode((int)response.HttpCode, response);
    }

    //POST: api/cities
    [HttpPost]
    public async Task<ActionResult<JsonResponse<CityDto>>> CreateAsync([FromBody] CityCreationDto cityCreationDto)
    {
        var response = await _cityService.CreateAsync(cityCreationDto);
        return StatusCode((int)response.HttpCode, response);
    }

    //PUT: api/cities/4
    [HttpPut("{id:int}")]
    public async Task<ActionResult<JsonResponse<CityDto>>> UpdateAsync(int id, [FromBody] CityUpdateDto cityUpdateDto)
    {
        var response = await _cityService.UpdateAsync(id, cityUpdateDto);
        return StatusCode((int)response.HttpCode, response);
    }

    //DELETE: api/cities/4
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<JsonResponse<bool?>>> DeleteAsync(int id)
    {
        var response = await _cityService.DeleteAsync(id);
        return StatusCode((int)response.HttpCode, response.HttpCode != HttpStatusCode.NoContent ? response : null);
    }
}