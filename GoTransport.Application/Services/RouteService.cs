using AutoMapper;
using GoTransport.Application.Attributes;
using GoTransport.Application.Builders;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.Route;
using GoTransport.Application.Extensions;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Interfaces.Base;
using GoTransport.Application.Parameters;
using GoTransport.Application.Specifications.Routes;
using GoTransport.Application.Wrappers;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Services;

[Transient]
internal class RouteService : IRouteService
{
    private readonly IRepository<Route> _routeRepository;
    private readonly IMapper _mapper;
    private readonly ICacheService<Route> _cacheService;

    public RouteService(IRepository<Route> routeRepository
        , IMapper mapper
        , ICacheService<Route> cacheService)
    {
        _routeRepository = routeRepository;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<JsonResponse<IEnumerable<RouteDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var routes = _cacheService.Get(CacheKey.Routes);
        if (routes is null)
        {
            routes = await _routeRepository.ListAsync(new RouteSpecification(), cancellationToken);
            _cacheService.Set(CacheKey.Routes, routes);
        }

        return ResponseBuilder<IEnumerable<RouteDto>>.Ok(_mapper.Map<IEnumerable<RouteDto>>(routes));
    }

    public async Task<JsonPagedResponse<IEnumerable<RouteDto>>> GetAsync(RouteParameters parameters, CancellationToken cancellationToken)
    {
        var routes = await _routeRepository.ListAsync(new PagedRouteSpecification(parameters), cancellationToken);
        var totalRecords = await _routeRepository.CountAsync(new RouteSpecification(parameters), cancellationToken);
        var metadata = Metadata.Create(parameters.PageNumber, parameters.PageSize, totalRecords);
        return ResponseBuilder<IEnumerable<RouteDto>>.OkPaged(routes, metadata);
    }

    public async Task<JsonResponse<RouteDto>> GetByIdAsync(dynamic id, CancellationToken cancellationToken)
    {
        var route = await _routeRepository.GetByIdAsync(id, cancellationToken);
        if (route is null) return ResponseBuilder<RouteDto>.NotFound();
        return ResponseBuilder<RouteDto>.Ok(_mapper.Map<RouteDto>(route));
    }

    public async Task<JsonResponse<RouteDto>> CreateAsync(RouteCreationDto routeCreation)
    {
        if (await IsDuplicarePoints(routeCreation.OriginPointId, routeCreation.DestinationPointId))
            return ResponseBuilder<RouteDto>.BadRequest(ErrorMessages.DuplicateRoute);

        var route = _mapper.Map<Route>(routeCreation);
        await _routeRepository.AddAsync(route);

        return ResponseBuilder<RouteDto>.Created(_mapper.Map<RouteDto>(route));
    }

    public async Task<JsonResponse<RouteDto>> UpdateAsync(dynamic id, RouteUpdateDto routeUpdate)
    {
        if (id != routeUpdate.RouteId)
            return ResponseBuilder<RouteDto>.BadRequest(ErrorMessages.UrlAndBodyIdNotEqual);

        var route = await _routeRepository.GetByIdAsync(id);
        if (route is null) return ResponseBuilder<RouteDto>.NotFound();

        if (await IsDuplicarePoints(routeUpdate.OriginPointId, routeUpdate.DestinationPointId, id))
            return ResponseBuilder<RouteDto>.BadRequest(ErrorMessages.DuplicateRoute);

        routeUpdate.Description = routeUpdate.Description.RemoveExtraBlank();

        route = _mapper.Map(routeUpdate, route);
        await _routeRepository.UpdateAsync(route);

        return ResponseBuilder<RouteDto>.Ok(_mapper.Map<RouteDto>(route));
    }

    public async Task<JsonResponse<bool?>> DeleteAsync(dynamic id)
    {
        var route = await _routeRepository.GetByIdAsync(id);
        if (route is null) return ResponseBuilder<bool?>.NotFound();

        await _routeRepository.DeleteAsync(route);
        return ResponseBuilder<bool?>.NoContent();
    }

    private async Task<bool> IsDuplicarePoints(int originPointId, int destinationPointId, int? id = null) =>
        await _routeRepository.AnyAsync(new DuplicationRouteSpecification(originPointId, destinationPointId, id));
}