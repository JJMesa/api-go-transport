using Ardalis.Specification;
using GoTransport.Application.Enums;
using GoTransport.Application.Parameters;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Specifications.Vehicles;

public class VehicleSpecification : Specification<Vehicle>
{
    public VehicleSpecification()
    {
        Query.Where(vehicle => vehicle.IsActive!.Value);

        Query.OrderBy(vehicle => vehicle.LicensePlate);
    }

    public VehicleSpecification(Guid scheduleId)
    {
        Query.Where(vehicle => vehicle.Schedules.Any(schedule => schedule.ScheduleId == scheduleId));

        Query.Include(vehicle => vehicle.Manufacturer);
    }

    public VehicleSpecification(VehicleParameters parameters)
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

        Query.Include(vehicle => vehicle.Manufacturer);
    }
}