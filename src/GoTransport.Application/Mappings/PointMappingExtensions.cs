using GoTransport.Application.Dtos.Point;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Mappings;

internal static class PointMappingExtensions
{
    /// <summary>
    /// Maps a point to its DTO. A navigation property that was not loaded maps to null.
    /// </summary>
    public static PointDto ToDto(this Point point) => new()
    {
        PointId = point.PointId,
        City = point.City?.ToDto()!,
        Detail = point.Detail
    };

    public static Point ToEntity(this PointCreationDto pointCreation) => new()
    {
        CityId = pointCreation.CityId,
        Detail = pointCreation.Detail
    };

    public static void ApplyTo(this PointUpdateDto pointUpdate, Point point)
    {
        point.CityId = pointUpdate.CityId;
        point.Detail = pointUpdate.Detail;
        point.IsActive = pointUpdate.IsActive;
    }
}
