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
        if (!string.IsNullOrWhiteSpace(parameters.OriginPointSearchCriteria))
        {
            Query.Search(ruote => $"{ruote.OriginPoint.City.Description}", $"%{parameters.OriginPointSearchCriteria}%");
            Query.Search(ruote => $"{ruote.OriginPoint.Detail}", $"%{parameters.OriginPointSearchCriteria}%");
        }

        if (!string.IsNullOrWhiteSpace(parameters.DestinationPointSearchCriteria))
        {
            Query.Search(ruote => $"{ruote.DestinationPoint.City.Description}", $"%{parameters.OriginPointSearchCriteria}%");
            Query.Search(ruote => $"{ruote.DestinationPoint.Detail}", $"%{parameters.OriginPointSearchCriteria}%");
        }

        switch (parameters.OrderBy)
        {
            case SortDirection.Ascending:
                Query.OrderBy(ruote => ruote.Description);
                break;

            case SortDirection.Descending:
                Query.OrderByDescending(ruote => ruote.Description);
                break;

            default:
                Query.OrderBy(ruote => ruote.Description);
                break;
        }

        if (parameters.IsActive.HasValue)
            Query.Where(ruote => ruote.IsActive == parameters.IsActive);

        Query.Skip((parameters.PageNumber - 1) * parameters.PageSize).Take(parameters.PageSize);

        Query.Select(ruote => new RouteDto
        {
            RouteId = ruote.RouteId,
            OriginPoint = new PointDto
            {
                PointId = ruote.OriginPoint!.PointId,
                City = new CityDto
                {
                    CityId = ruote.OriginPoint!.City!.CityId,
                    Description = ruote.OriginPoint!.City!.Description,
                    IsActive = ruote.OriginPoint!.City!.IsActive!.Value
                },
                Detail = ruote.OriginPoint!.Detail
            },
            DestinationPoint = new PointDto
            {
                PointId = ruote.DestinationPoint!.PointId,
                City = new CityDto
                {
                    CityId = ruote.DestinationPoint!.City!.CityId,
                    Description = ruote.DestinationPoint!.City!.Description,
                    IsActive = ruote.DestinationPoint!.City!.IsActive!.Value
                },
                Detail = ruote.DestinationPoint!.Detail
            },
            Description = ruote.Description,
            IsActive = ruote.IsActive!.Value
        });
    }
}