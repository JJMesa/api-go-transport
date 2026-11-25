using Ardalis.Specification;
using GoTransport.Application.Parameters;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Specifications.Vehicles;

/// <summary>
/// The filters a page of Vehicle rows and its total count share, so the page and the count always agree.
/// </summary>
internal static class VehicleFilter
{
    public static void Apply(ISpecificationBuilder<Vehicle> query, VehicleParameters parameters)
    {
        if (!string.IsNullOrWhiteSpace(parameters.SearchCriteria))
            query.Search(vehicle => $"{vehicle.LicensePlate}", $"%{parameters.SearchCriteria}%");

        if (parameters.IsActive.HasValue)
            query.Where(vehicle => vehicle.IsActive == parameters.IsActive);
    }
}
