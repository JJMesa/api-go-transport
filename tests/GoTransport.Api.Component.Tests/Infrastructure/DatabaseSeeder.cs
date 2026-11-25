using GoTransport.Domain.Entities.App;
using GoTransport.Domain.Entities.Bas;
using GoTransport.Persistence.Contexts;

namespace GoTransport.Api.Component.Tests.Infrastructure;

/// <summary>
/// Populates the in-memory database with a small, deterministic dataset that the component tests
/// assert against. Ids match the values exposed through <c>Utils.DefaultId</c> so the existing
/// expectations remain valid.
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>The seeded Schedule: a daily Medellín → Bogotá departure.</summary>
    public static readonly Guid ScheduleId = new("5f1c9a3e-0000-4000-8000-000000000001");

    /// <summary>A Reservation whose Trip is a week ahead, so it can still be updated or cancelled.</summary>
    public static readonly Guid UpcomingReservationId = new("5f1c9a3e-0000-4000-8000-000000000002");

    /// <summary>A Reservation whose Trip took place a week ago, so it can no longer be changed.</summary>
    public static readonly Guid PastReservationId = new("5f1c9a3e-0000-4000-8000-000000000003");

    /// <summary>Capacity of the seeded Vehicle. Kept small so a test can fill a Trip with a few bookings.</summary>
    public const int VehicleCapacity = 2;

    /// <summary>Travel date of the upcoming seeded Reservation, so its Trip already has one place taken.</summary>
    public static DateTime UpcomingTravelDate => DateTime.Today.AddDays(7);

    public static void Seed(ApplicationDbContext context)
    {
        context.Set<Department>().AddRange(
            new Department { DepartmentId = 1, Description = "Antioquia", IsActive = true },
            new Department { DepartmentId = 2, Description = "Cundinamarca", IsActive = true });

        context.Set<City>().AddRange(
            new City { CityId = 1, Description = "Medellín", DepartmentId = 1, IsActive = true },
            new City { CityId = 2, Description = "Abejorral", DepartmentId = 1, IsActive = true },
            new City { CityId = 3, Description = "Bello", DepartmentId = 1, IsActive = true },
            new City { CityId = 4, Description = "Envigado", DepartmentId = 1, IsActive = true },
            new City { CityId = 5, Description = "Itagüí", DepartmentId = 1, IsActive = true },
            new City { CityId = 6, Description = "Bogotá", DepartmentId = 2, IsActive = true },
            new City { CityId = 7, Description = "Soacha", DepartmentId = 2, IsActive = true });

        SeedReservationNetwork(context);

        // Persist through SaveChangesAsync so the auditing override stamps the required audit columns.
        context.SaveChangesAsync().GetAwaiter().GetResult();
    }

    private static void SeedReservationNetwork(ApplicationDbContext context)
    {
        context.Set<Manufacturer>().Add(new Manufacturer { ManufacturerId = 1, Description = "SCANIA", IsActive = true });
        context.Set<IdentificationType>().Add(new IdentificationType { IdentificationTypeId = 1, Description = "Cédula de ciudadanía", IsActive = true });

        context.Set<Point>().AddRange(
            new Point { PointId = 1, CityId = 1, Detail = "Terminal de Transporte Norte", IsActive = true },
            new Point { PointId = 2, CityId = 6, Detail = "Terminal Salitre", IsActive = true });

        context.Set<Route>().Add(new Route { RouteId = 1, Description = "Medellín - Bogotá", OriginPointId = 1, DestinationPointId = 2, IsActive = true });
        context.Set<Vehicle>().Add(new Vehicle { VehicleId = 1, ManufacturerId = 1, LicensePlate = "ABC123", Model = 2022, Capacity = VehicleCapacity, IsActive = true });

        context.Set<Schedule>().Add(new Schedule
        {
            ScheduleId = ScheduleId,
            DepartureTime = new TimeSpan(6, 0, 0),
            ArrivalTime = new TimeSpan(15, 0, 0),
            Duration = new TimeSpan(9, 0, 0),
            VehicleId = 1,
            RouteId = 1,
            IsActive = true
        });

        context.Set<Reservation>().AddRange(
            NewReservation(UpcomingReservationId, UpcomingTravelDate),
            NewReservation(PastReservationId, DateTime.Today.AddDays(-7)));
    }

    private static Reservation NewReservation(Guid reservationId, DateTime travelDate) => new()
    {
        ReservationId = reservationId,
        ScheduleId = ScheduleId,
        ReservationDate = travelDate,
        PassengerFirstName = "John",
        PassengerLastName = "Doe",
        IdentificationTypeId = 1,
        PassengerIdentification = "1000123456",
        Detail = "Seeded reservation",
        IsActive = true
    };
}
