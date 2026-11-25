using Ardalis.Specification;
using GoTransport.Application.Specifications.Cities;
using GoTransport.Application.Specifications.Departments;
using GoTransport.Application.Specifications.Points;
using GoTransport.Application.Specifications.Reservations;
using GoTransport.Application.Specifications.Routes;
using GoTransport.Application.Specifications.Schedules;
using GoTransport.Application.Specifications.Vehicles;

namespace GoTransport.Api.Unit.Tests.TestCases;

/// <summary>
/// Specifications behind GET endpoints and cached catalogs skip change tracking; the ones that load an
/// entity to change it must keep tracking, or the change would never be saved.
/// </summary>
public class SpecificationTrackingTests
{
    public static TheoryData<string, bool> Specifications => new()
    {
        { nameof(CitySpecification), new CitySpecification(1).AsNoTracking },
        { nameof(DepartmentSpecification), new DepartmentSpecification().AsNoTracking },
        { nameof(PointSpecification), new PointSpecification().AsNoTracking },
        { nameof(RouteSpecification), new RouteSpecification().AsNoTracking },
        { nameof(RouteDetailSpecification), new RouteDetailSpecification(1).AsNoTracking },
        { nameof(VehicleSpecification), new VehicleSpecification().AsNoTracking },
        { nameof(VehicleDetailSpecification), new VehicleDetailSpecification(1).AsNoTracking },
        { "ScheduleSpecification(routeId)", new ScheduleSpecification(1).AsNoTracking },
        { nameof(ReservationDetailSpecification), new ReservationDetailSpecification(Guid.NewGuid()).AsNoTracking },
        { "ReservationSpecification(passenger lookup)", new ReservationSpecification(Guid.NewGuid(), "1000123456").AsNoTracking },
    };

    [Theory]
    [MemberData(nameof(Specifications))]
    public void ReadOnlySpecifications_ShouldNotTrack(string specification, bool asNoTracking)
    {
        Assert.True(asNoTracking, $"{specification} should use AsNoTracking");
    }

    [Fact]
    public void SpecificationsThatLoadForAnUpdate_ShouldTrack()
    {
        Assert.False(new ReservationSpecification(reservationId: Guid.NewGuid()).AsNoTracking);
        Assert.False(new ScheduleSpecification(Guid.NewGuid()).AsNoTracking);
        Assert.False(new PointSpecification(1).AsNoTracking);
    }
}
