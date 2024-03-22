using System.Net;
using Asp.Versioning;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.Schedule;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoTransport.Api.Controllers.v1;

[Authorize(Roles = Roles.Administrator)]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class SchedulesController : ControllerBase
{
    private readonly IScheduleService _scheduleService;

    public SchedulesController(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    //GET: api/v1/ruotes/2/schedules-all
    [HttpGet("/api/v{version:apiVersion}/routes/{routeId:int}/[controller]")]
    public async Task<ActionResult<JsonResponse<IEnumerable<ScheduleDto>>>> GetAllByRouteAsync(int routeId, CancellationToken cancellationToken)
    {
        var response = await _scheduleService.GetAllByRouteAsync(routeId, cancellationToken);
        return StatusCode((int)response.HttpCode, response);
    }

    //GET: api/v1/schedules/ABCD-1234-A1B2-C3D4
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<JsonResponse<ScheduleDto>>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var response = await _scheduleService.GetByIdAsync(id, cancellationToken);
        return StatusCode((int)response.HttpCode, response);
    }

    //POST: api/v1/schedules
    [HttpPost]
    public async Task<ActionResult<JsonResponse<ScheduleDto>>> CreateAsync([FromBody] ScheduleCreationDto scheduleCreationDto)
    {
        var response = await _scheduleService.CreateAsync(scheduleCreationDto);
        return StatusCode((int)response.HttpCode, response);
    }

    //PUT: api/v1/schedules/ABCD-1234-A1B2-C3D4
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<JsonResponse<ScheduleDto>>> UpdateAsync(Guid id, [FromBody] ScheduleUpdateDto scheduleUpdateDto)
    {
        var response = await _scheduleService.UpdateAsync(id, scheduleUpdateDto);
        return StatusCode((int)response.HttpCode, response);
    }

    //DELETE: api/v1/schedules/ABCD-1234-A1B2-C3D4
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<JsonResponse<bool?>>> DeleteAsync(Guid id)
    {
        var response = await _scheduleService.DeleteAsync(id);
        return StatusCode((int)response.HttpCode, response.HttpCode != HttpStatusCode.NoContent ? response : null);
    }
}