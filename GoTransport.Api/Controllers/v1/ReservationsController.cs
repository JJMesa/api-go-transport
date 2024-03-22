using System.Net;
using System.Text.Json;
using Asp.Versioning;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.Reservation;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Parameters;
using GoTransport.Application.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoTransport.Api.Controllers.v1;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    //GET: api/v1/reservations/all
    [HttpGet("all")]
    public async Task<ActionResult<JsonResponse<IEnumerable<ReservationDto>>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var response = await _reservationService.GetAllAsync(cancellationToken);
        return StatusCode((int)response.HttpCode, response);
    }

    //GET: api/v1/schedules/ABCD-1234-A1B2-C3D4/reservations
    [AllowAnonymous]
    [HttpGet("/api/v{version:apiVersion}/schedules/{scheduleId:guid}/[controller]")]
    public async Task<ActionResult<JsonResponse<IEnumerable<ReservationDto>>>> GetByScheduleAsync(Guid scheduleId, [FromQuery] ReservationParameters parameters, CancellationToken cancellationToken)
    {
        var response = await _reservationService.GetByScheduleAsync(scheduleId, parameters, cancellationToken);
        Response.Headers.Add(Constants.PaginationHeader, JsonSerializer.Serialize(response.Metadata));
        return StatusCode((int)response.HttpCode, response);
    }

    //GET: /api/v1/reservations/A5D7/person/1000123456
    [HttpGet("{id:guid}/person/{passengerIdentification}/details")]
    public async Task<ActionResult<JsonResponse<ReservationDto>>> GetDetailsByPerson(string id, string passengerIdentification, CancellationToken cancellationToken)
    {
        var response = await _reservationService.GetDetailsByPerson(id, passengerIdentification, cancellationToken);
        return StatusCode((int)response.HttpCode, response);
    }

    //GET: api/v1/reservations/ABCD-1234-A1B2-C3D4
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<JsonResponse<ReservationDto>>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var response = await _reservationService.GetByIdAsync(id, cancellationToken);
        return StatusCode((int)response.HttpCode, response);
    }

    //POST: api/v1/reservations
    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<JsonResponse<ReservationDto>>> CreateAsync([FromBody] ReservationCreationDto reservationCreationDto)
    {
        var response = await _reservationService.CreateAsync(reservationCreationDto);
        return StatusCode((int)response.HttpCode, response);
    }

    //PUT: api/v1/reservations/ABCD-1234-A1B2-C3D4
    [AllowAnonymous]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<JsonResponse<ReservationDto>>> UpdateAsync(Guid id, [FromBody] ReservationUpdateDto reservationUpdateDto)
    {
        var response = await _reservationService.UpdateAsync(id, reservationUpdateDto);
        return StatusCode((int)response.HttpCode, response);
    }

    //DELETE: api/v1/reservations/ABCD-1234-A1B2-C3D4
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<JsonResponse<bool?>>> DeleteAsync(Guid id)
    {
        var response = await _reservationService.DeleteAsync(id);
        return StatusCode((int)response.HttpCode, response.HttpCode != HttpStatusCode.NoContent ? response : null);
    }
}