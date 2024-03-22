using Asp.Versioning;
using GoTransport.Application.Dtos.IdentificationType;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoTransport.Api.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/identification-types")]
public class IdentificationTypesController : ControllerBase
{
    private readonly IIdentificationTypeService _identificationTypeService;

    public IdentificationTypesController(IIdentificationTypeService identificationTypeService)
    {
        _identificationTypeService = identificationTypeService;
    }

    //GET: api/v1/identification-types/all
    [AllowAnonymous]
    [HttpGet("all")]
    public async Task<ActionResult<JsonResponse<IEnumerable<IdentificationTypeDto>>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var response = await _identificationTypeService.GetAllAsync(cancellationToken);
        return StatusCode((int)response.HttpCode, response);
    }
}