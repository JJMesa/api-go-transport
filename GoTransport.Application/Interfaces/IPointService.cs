using GoTransport.Application.Dtos.Point;
using GoTransport.Application.Interfaces.Base;
using GoTransport.Application.Parameters;
using GoTransport.Application.Wrappers;

namespace GoTransport.Application.Interfaces;

public interface IPointService : IServiceBase<PointCreationDto, PointUpdateDto, PointDto>
{
    Task<JsonPagedResponse<IEnumerable<PointDto>>> GetAsync(PointParameters parameters, CancellationToken cancellationToken);
}