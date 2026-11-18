using System.Net;
using System.Text.Json;
using Asp.Versioning;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.Point;
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
public class PointsController : ControllerBase
{
    private readonly IPointService _pointService;

    public PointsController(IPointService pointService)
    {
        _pointService = pointService;
    }

    //GET: api/points/all
    [HttpGet("all")]
    public async Task<ActionResult<JsonResponse<IEnumerable<PointDto>>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var response = await _pointService.GetAllAsync(cancellationToken);
        return StatusCode((int)response.HttpCode, response);
    }

    //GET: api/points
    [HttpGet]
    public async Task<ActionResult<JsonPagedResponse<IEnumerable<PointDto>>>> GetAsync([FromQuery] PointParameters parameters, CancellationToken cancellationToken)
    {
        var response = await _pointService.GetAsync(parameters, cancellationToken);
        Response.Headers.Add(Constants.PaginationHeader, JsonSerializer.Serialize(response.Metadata));
        return StatusCode((int)response.HttpCode, response);
    }

    //GET: api/points/3
    [HttpGet("{id:int}")]
    public async Task<ActionResult<JsonResponse<PointDto>>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var response = await _pointService.GetByIdAsync(id, cancellationToken);
        return StatusCode((int)response.HttpCode, response);
    }

    //POST: api/points
    [HttpPost]
    public async Task<ActionResult<JsonResponse<PointDto>>> CreateAsync(PointCreationDto pointCreationDto)
    {
        var response = await _pointService.CreateAsync(pointCreationDto);
        return StatusCode((int)response.HttpCode, response);
    }

    //PUT: api/points/3
    [HttpPut("{id:int}")]
    public async Task<ActionResult<JsonResponse<PointDto>>> UpdateAsync(int id, PointUpdateDto pointUpdateDto)
    {
        var response = await _pointService.UpdateAsync(id, pointUpdateDto);
        return StatusCode((int)response.HttpCode, response);
    }

    //DELETE: api/points/3
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<JsonResponse<bool?>>> DeleteAsync(int id)
    {
        var response = await _pointService.DeleteAsync(id);
        return StatusCode((int)response.HttpCode, response.HttpCode != HttpStatusCode.NoContent ? response : null);
    }
}