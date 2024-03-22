using Ardalis.Specification;
using GoTransport.Application.Dtos.Manufacturer;
using GoTransport.Application.Dtos.Vehicle;
using GoTransport.Application.Enums;
using GoTransport.Application.Parameters;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Specifications.Vehicles;

public class PagedVehicleSpecification : Specification<Vehicle, VehicleDto>
{
    public PagedVehicleSpecification(VehicleParameters parameters)
    {
        if (!string.IsNullOrWhiteSpace(parameters.SearchCriteria))
            Query.Search(vehicle => $"{vehicle.LicensePlate}", $"%{parameters.SearchCriteria}%");

        switch (parameters.OrderBy)
        {
            case SortDirection.Ascending:
                Query.OrderBy(vehicle => vehicle.LicensePlate);
                break;

            case SortDirection.Descending:
                Query.OrderByDescending(vehicle => vehicle.LicensePlate);
                break;

            default:
                Query.OrderBy(vehicle => vehicle.LicensePlate);
                break;
        }

        if (parameters.IsActive.HasValue)
            Query.Where(vehicle => vehicle.IsActive == parameters.IsActive);

        Query.Skip((parameters.PageNumber - 1) * parameters.PageSize).Take(parameters.PageSize);

        Query.Select(vehicle => new VehicleDto
        {
            VehicleId = vehicle.VehicleId,
            LicensePlate = vehicle.LicensePlate,
            Manufacturer = new ManufacturerDto
            {
                ManufacturerId = vehicle.Manufacturer.ManufacturerId,
                Description = vehicle.Manufacturer.Description
            },
            Model = vehicle.Model,
            Capacity = vehicle.Capacity,
            IsActive = vehicle.IsActive!.Value
        });
    }
}