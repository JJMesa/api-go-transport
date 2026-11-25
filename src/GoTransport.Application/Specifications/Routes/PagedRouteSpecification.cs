using Ardalis.Specification;
using GoTransport.Application.Dtos.City;
using GoTransport.Application.Dtos.Point;
using GoTransport.Application.Dtos.Route;
using GoTransport.Application.Enums;
using GoTransport.Application.Parameters;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Specifications.Routes;

public class PagedRouteSpecification : Specification<Route, RouteDto>
{
    public PagedRouteSpecification(RouteParameters parameters)
    {
        RouteFilter.Apply(Query, parameters);

        switch (parameters.OrderBy)
        {
            case SortDirection.Ascending:
                Query.OrderBy(route => route.Description);
                break;

            case SortDirection.Descending:
                Query.OrderByDescending(route => route.Description);
                break;

            default:
                Query.OrderBy(route => route.Description);
                break;
        }

        Query.Skip((parameters.PageNumber - 1) * parameters.PageSize).Take(parameters.PageSize);

        Query.Select(route => new RouteDto
        {
            RouteId = route.RouteId,
            OriginPoint = new PointDto
            {
                PointId = route.OriginPoint!.PointId,
                City = new CityDto
                {
                    CityId = route.OriginPoint!.City!.CityId,
                    Description = route.OriginPoint!.City!.Description,
                    IsActive = route.OriginPoint!.City!.IsActive
                },
                Detail = route.OriginPoint!.Detail
            },
            DestinationPoint = new PointDto
            {
                PointId = route.DestinationPoint!.PointId,
                City = new CityDto
                {
                    CityId = route.DestinationPoint!.City!.CityId,
                    Description = route.DestinationPoint!.City!.Description,
                    IsActive = route.DestinationPoint!.City!.IsActive
                },
                Detail = route.DestinationPoint!.Detail
            },
            Description = route.Description,
            IsActive = route.IsActive
        });
    }
}