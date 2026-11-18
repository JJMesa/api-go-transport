using System.Net;
using System.Text.Json;
using Asp.Versioning;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.Route;
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
public class RoutesController : ControllerBase
{
    private readonly IRouteService _routeService;

    public RoutesController(IRouteService routeService)
    {
        _routeService = routeService;
    }

    //GET: api/v1/route/all
    [HttpGet("all")]
    public async Task<ActionResult<JsonResponse<IEnumerable<RouteDto>>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var response = await _routeService.GetAllAsync(cancellationToken);
        return StatusCode((int)response.HttpCode, response);
    }

    //GET: api/v1/route
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<JsonResponse<IEnumerable<RouteDto>>>> GetAsync([FromQuery] RouteParameters parameters, CancellationToken cancellationToken)
    {
        var response = await _routeService.GetAsync(parameters, cancellationToken);
        Response.Headers.Add(Constants.PaginationHeader, JsonSerializer.Serialize(response.Metadata));
        return StatusCode((int)response.HttpCode, response);
    }

    //GET: api/v1/route/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<JsonResponse<RouteDto>>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var response = await _routeService.GetByIdAsync(id, cancellationToken);
        return StatusCode((int)response.HttpCode, response);
    }

    //POST: api/v1/route
    [HttpPost]
    public async Task<ActionResult<JsonResponse<RouteDto>>> CreateAsync([FromBody] RouteCreationDto routeCreationDto)
    {
        var response = await _routeService.CreateAsync(routeCreationDto);
        return StatusCode((int)response.HttpCode, response);
    }

    //PUT: api/v1/route/5
    [HttpPut("{id:int}")]
    public async Task<ActionResult<JsonResponse<RouteDto>>> UpdateAsync(int id, [FromBody] RouteUpdateDto routeUpdateDto)
    {
        var response = await _routeService.UpdateAsync(id, routeUpdateDto);
        return StatusCode((int)response.HttpCode, response);
    }

    //DELETE: api/v1/route/5
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<JsonResponse<bool?>>> DeleteAsync(int id)
    {
        var response = await _routeService.DeleteAsync(id);
        return StatusCode((int)response.HttpCode, response.HttpCode != HttpStatusCode.NoContent ? response : null);
    }
}