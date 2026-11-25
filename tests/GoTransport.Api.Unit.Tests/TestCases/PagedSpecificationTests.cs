using GoTransport.Application.Enums;
using GoTransport.Application.Parameters;
using GoTransport.Application.Specifications.Cities;
using GoTransport.Application.Specifications.Departments;
using GoTransport.Application.Specifications.Points;
using GoTransport.Application.Specifications.Reservations;
using GoTransport.Application.Specifications.Routes;
using GoTransport.Application.Specifications.Users;
using GoTransport.Application.Specifications.Vehicles;
using GoTransport.Domain.Entities.App;
using GoTransport.Domain.Entities.Bas;

namespace GoTransport.Api.Unit.Tests.TestCases;

/// <summary>
/// The sort direction of every paged list and the IsActive filter of every list that has one, evaluated
/// in memory (RelationalSpecificationTests in the component tests checks that the queries translate to SQL).
/// No direction sorts ascending, like Ascending.
/// </summary>
public class PagedSpecificationTests
{
    private static readonly Department Antioquia = new() { DepartmentId = 1, Description = "Antioquia", IsActive = true };
    private static readonly City Medellin = new() { CityId = 1, Description = "Medellín", DepartmentId = 1, Department = Antioquia, IsActive = true };

    public static TheoryData<SortDirection?, string[]> Directions => new()
    {
        { SortDirection.Ascending, ["Abejorral", "Bello", "Caldas"] },
        { SortDirection.Descending, ["Caldas", "Bello", "Abejorral"] },
        { null, ["Abejorral", "Bello", "Caldas"] }
    };

    // Ties on the first name are broken by the last name, in the same direction.
    public static TheoryData<SortDirection?, string[]> NameDirections => new()
    {
        { SortDirection.Ascending, ["Ana Mora", "Ana Ruiz", "Luis Gómez"] },
        { SortDirection.Descending, ["Luis Gómez", "Ana Ruiz", "Ana Mora"] },
        { null, ["Ana Mora", "Ana Ruiz", "Luis Gómez"] }
    };

    #region Ordering

    [Theory]
    [MemberData(nameof(Directions))]
    public void PagedDepartmentSpecification_ShouldSortByDescription(SortDirection? direction, string[] expected)
    {
        var departments = Shuffled().Select((name, i) => new Department { DepartmentId = i + 1, Description = name, IsActive = true });

        var actual = new PagedDepartmentSpecification(new DepartmentParameters { OrderBy = direction }).Evaluate(departments);

        Assert.Equal(expected, actual.Select(d => d.Description));
    }

    [Theory]
    [MemberData(nameof(Directions))]
    public void PagedVehicleSpecification_ShouldSortByLicensePlate(SortDirection? direction, string[] expected)
    {
        var scania = new Manufacturer { ManufacturerId = 1, Description = "SCANIA", IsActive = true };
        var vehicles = Shuffled().Select((plate, i) => new Vehicle { VehicleId = i + 1, LicensePlate = plate, Model = 2024, Capacity = 40, ManufacturerId = 1, Manufacturer = scania, IsActive = true });

        var actual = new PagedVehicleSpecification(new VehicleParameters { OrderBy = direction }).Evaluate(vehicles);

        Assert.Equal(expected, actual.Select(v => v.LicensePlate));
    }

    [Theory]
    [MemberData(nameof(Directions))]
    public void PagedPointSpecification_ShouldSortByDetail(SortDirection? direction, string[] expected)
    {
        var points = Shuffled().Select((detail, i) => PointAt(i + 1, detail));

        var actual = new PagedPointSpecification(new PointParameters { OrderBy = direction }).Evaluate(points);

        Assert.Equal(expected, actual.Select(p => p.Detail));
    }

    [Theory]
    [MemberData(nameof(Directions))]
    public void PagedRouteSpecification_ShouldSortByDescription(SortDirection? direction, string[] expected)
    {
        var routes = Shuffled().Select((description, i) => RouteNamed(i + 1, description));

        var actual = new PagedRouteSpecification(new RouteParameters { OrderBy = direction }).Evaluate(routes);

        Assert.Equal(expected, actual.Select(r => r.Description));
    }

    [Theory]
    [MemberData(nameof(NameDirections))]
    public void PagedUserSpecification_ShouldSortByFirstThenLastName(SortDirection? direction, string[] expected)
    {
        var users = ShuffledNames().Select((name, i) => new User { Id = i + 1, FirstName = name.First, LastName = name.Last, Email = $"user{i}@gotransport.test", IsActive = true });

        var actual = new PagedUserSpecification(new UserParameters { OrderBy = direction }).Evaluate(users);

        Assert.Equal(expected, actual.Select(u => $"{u.FirstName} {u.LastName}"));
    }

    [Theory]
    [MemberData(nameof(NameDirections))]
    public void PagedReservationSpecification_ShouldSortByPassengerFirstThenLastName(SortDirection? direction, string[] expected)
    {
        var schedule = new Schedule { ScheduleId = Guid.NewGuid(), DepartureTime = new TimeSpan(8, 0, 0), ArrivalTime = new TimeSpan(17, 0, 0), Duration = TimeSpan.FromHours(9), Route = RouteNamed(1, "Medellín - Bogotá"), IsActive = true };
        var cedula = new IdentificationType { IdentificationTypeId = 1, Description = "Cédula de ciudadanía", IsActive = true };
        var reservations = ShuffledNames().Select(name => new Reservation
        {
            ReservationId = Guid.NewGuid(),
            ScheduleId = schedule.ScheduleId,
            Schedule = schedule,
            ReservationDate = new DateTime(2026, 12, 24),
            PassengerFirstName = name.First,
            PassengerLastName = name.Last,
            PassengerIdentification = "1000123456",
            IdentificationTypeId = 1,
            IdentificationType = cedula,
            IsActive = true
        });

        var actual = new PagedReservationSpecification(schedule.ScheduleId, new ReservationParameters { OrderBy = direction }).Evaluate(reservations);

        Assert.Equal(expected, actual.Select(r => $"{r.PassengerFirstName} {r.PassengerLastName}"));
    }

    #endregion Ordering

    #region IsActive filters

    [Theory]
    [InlineData(true, 2)]
    [InlineData(false, 1)]
    [InlineData(null, 3)]
    public void DepartmentSpecification_ShouldFilterByIsActive(bool? isActive, int expected)
    {
        var departments = ActiveFlags().Select((active, i) => new Department { DepartmentId = i + 1, Description = $"Department {i}", IsActive = active });

        var actual = new DepartmentSpecification(new DepartmentParameters { IsActive = isActive }).Evaluate(departments);

        Assert.Equal(expected, actual.Count());
    }

    [Theory]
    [InlineData(true, 2)]
    [InlineData(false, 1)]
    [InlineData(null, 3)]
    public void CitySpecification_ShouldFilterByIsActive(bool? isActive, int expected)
    {
        var cities = ActiveFlags().Select((active, i) => new City { CityId = i + 1, Description = $"City {i}", DepartmentId = 1, IsActive = active });

        var actual = new CitySpecification(new CityParameters { IsActive = isActive }).Evaluate(cities);

        Assert.Equal(expected, actual.Count());
    }

    [Theory]
    [InlineData(true, 2)]
    [InlineData(false, 1)]
    [InlineData(null, 3)]
    public void PointSpecification_ShouldFilterByIsActive(bool? isActive, int expected)
    {
        var points = ActiveFlags().Select((active, i) => { var point = PointAt(i + 1, $"Point {i}"); point.IsActive = active; return point; });

        var actual = new PointSpecification(new PointParameters { IsActive = isActive }).Evaluate(points);

        Assert.Equal(expected, actual.Count());
    }

    [Theory]
    [InlineData(true, 2)]
    [InlineData(false, 1)]
    [InlineData(null, 3)]
    public void RouteSpecification_ShouldFilterByIsActive(bool? isActive, int expected)
    {
        var routes = ActiveFlags().Select((active, i) => { var route = RouteNamed(i + 1, $"Route {i}"); route.IsActive = active; return route; });

        var actual = new RouteSpecification(new RouteParameters { IsActive = isActive }).Evaluate(routes);

        Assert.Equal(expected, actual.Count());
    }

    [Theory]
    [InlineData(true, 2)]
    [InlineData(false, 1)]
    [InlineData(null, 3)]
    public void VehicleSpecification_ShouldFilterByIsActive(bool? isActive, int expected)
    {
        var vehicles = ActiveFlags().Select((active, i) => new Vehicle { VehicleId = i + 1, LicensePlate = $"ABC{i:000}", Model = 2024, Capacity = 40, ManufacturerId = 1, IsActive = active });

        var actual = new VehicleSpecification(new VehicleParameters { IsActive = isActive }).Evaluate(vehicles);

        Assert.Equal(expected, actual.Count());
    }

    #endregion IsActive filters

    #region Helpers

    private static string[] Shuffled() => ["Bello", "Caldas", "Abejorral"];

    private static (string First, string Last)[] ShuffledNames() => [("Ana", "Ruiz"), ("Luis", "Gómez"), ("Ana", "Mora")];

    private static bool[] ActiveFlags() => [true, false, true];

    private static Point PointAt(int pointId, string detail) =>
        new() { PointId = pointId, Detail = detail, CityId = Medellin.CityId, City = Medellin, IsActive = true };

    private static Route RouteNamed(int routeId, string description) => new()
    {
        RouteId = routeId,
        Description = description,
        OriginPointId = 1,
        OriginPoint = PointAt(1, "Terminal del Norte"),
        DestinationPointId = 2,
        DestinationPoint = PointAt(2, "Terminal del Sur"),
        IsActive = true
    };

    #endregion Helpers
}
