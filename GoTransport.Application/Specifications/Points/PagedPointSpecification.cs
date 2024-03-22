using Ardalis.Specification;
using GoTransport.Application.Dtos.City;
using GoTransport.Application.Dtos.Point;
using GoTransport.Application.Enums;
using GoTransport.Application.Parameters;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Specifications.Points;

public class PagedPointSpecification : Specification<Point, PointDto>
{
    public PagedPointSpecification(PointParameters parameters)
    {
        switch (parameters.OrderBy)
        {
            case SortDirection.Ascending:
                Query.OrderBy(point => point.Detail);
                break;

            case SortDirection.Descending:
                Query.OrderByDescending(point => point.Detail);
                break;

            default:
                Query.OrderBy(point => point.Detail);
                break;
        }

        if (parameters.IsActive.HasValue)
            Query.Where(point => point.IsActive == parameters.IsActive);

        Query.Skip((parameters.PageNumber - 1) * parameters.PageSize).Take(parameters.PageSize);

        Query.Select(point => new PointDto
        {
            PointId = point.PointId,
            City = new CityDto
            {
                CityId = point.City!.CityId,
                Description = point.City!.Description,
                IsActive = point.City!.IsActive!.Value
            },
            Detail = point.Detail
        });
    }
}