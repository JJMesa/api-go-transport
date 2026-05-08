using GoTransport.Application.Dtos.Vehicle;
using GoTransport.Application.Interfaces.Base;
using GoTransport.Application.Parameters;
using GoTransport.Application.Wrappers;

namespace GoTransport.Application.Interfaces;

public interface IVehicleService : IServiceBase<int, VehicleCreationDto, VehicleUpdateDto, VehicleDto>
{
    Task<JsonPagedResponse<IEnumerable<VehicleDto>>> GetAsync(VehicleParameters parameters, CancellationToken cancellationToken);
}