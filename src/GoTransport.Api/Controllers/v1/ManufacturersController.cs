using Asp.Versioning;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.Manufacturer;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoTransport.Api.Controllers.v1;

[Authorize(Roles = Roles.Administrator)]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ManufacturersController : ControllerBase
{
    private readonly IManufacturerService _manufacturerService;

    public ManufacturersController(IManufacturerService manufacturerService)
    {
        _manufacturerService = manufacturerService;
    }

    //GET: api/v1/manufacturers/all
    [HttpGet("all")]
    public async Task<ActionResult<JsonResponse<IEnumerable<ManufacturerDto>>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var response = await _manufacturerService.GetAllAsync(cancellationToken);
        return StatusCode((int)response.HttpCode, response);
    }
}