using Ardalis.Specification.EntityFrameworkCore;
using GoTransport.Api.Component.Tests.Infrastructure;
using GoTransport.Application.Parameters;
using GoTransport.Application.Specifications.Cities;
using GoTransport.Application.Specifications.Departments;
using GoTransport.Application.Specifications.Points;
using GoTransport.Application.Specifications.Reservations;
using GoTransport.Application.Specifications.Routes;
using GoTransport.Application.Specifications.Users;
using GoTransport.Application.Specifications.Vehicles;
using GoTransport.Domain.Entities.App;
using GoTransport.Domain.Interfaces;
using GoTransport.Persistence.Contexts;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GoTransport.Api.Component.Tests.TestCases;

/// <summary>
/// Runs the specifications against a relational provider (in-memory SQLite) over the component-test seed.
/// EF InMemory evaluates queries in memory, so it accepts expressions SQL cannot translate and ignores
/// unique indexes; here every specification must become real SQL and the constraints are enforced.
/// SQL Server specifics (collation, the LIKE escapes of ToContainsPattern, ordering by time) are not covered.
/// </summary>
public sealed class RelationalSpecificationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _context;

    public RelationalSpecificationTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).Options;
        _context = new SqliteApplicationDbContext(options);
        _context.Database.EnsureCreated();
        DatabaseSeeder.Seed(_context);
        _context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task CityPageAndCount_FilterByDepartment()
    {
        var parameters = new CityParameters { DepartmentId = 2 };

        var page = await SpecificationEvaluator.Default.GetQuery(_context.Set<GoTransport.Domain.Entities.Bas.City>(), new PagedCitySpecification(parameters)).ToListAsync();
        var count = await SpecificationEvaluator.Default.GetQuery(_context.Set<GoTransport.Domain.Entities.Bas.City>(), new CitySpecification(parameters)).CountAsync();

        Assert.Equal(2, page.Count);
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task DepartmentPage_SearchesTheDescription()
    {
        var page = await SpecificationEvaluator.Default.GetQuery(_context.Set<GoTransport.Domain.Entities.Bas.Department>(),
            new PagedDepartmentSpecification(new DepartmentParameters { SearchCriteria = "Antio" })).ToListAsync();

        Assert.Equal("Antioquia", Assert.Single(page).Description);
    }

    [Fact]
    public async Task PointPage_PagesAndCounts()
    {
        var parameters = new PointParameters { PageNumber = 2, PageSize = 1 };

        var page = await SpecificationEvaluator.Default.GetQuery(_context.Set<Point>(), new PagedPointSpecification(parameters)).ToListAsync();
        var count = await SpecificationEvaluator.Default.GetQuery(_context.Set<Point>(), new PointSpecification(parameters)).CountAsync();

        Assert.Single(page);
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task RoutePage_SearchesTheDestinationAndProjectsItsPoints()
    {
        var page = await SpecificationEvaluator.Default.GetQuery(_context.Set<Route>(),
            new PagedRouteSpecification(new RouteParameters { DestinationPointSearchCriteria = "Bogot" })).ToListAsync();

        var route = Assert.Single(page);
        Assert.Equal("Medellín", route.OriginPoint!.City!.Description);
    }

    [Fact]
    public async Task VehiclePage_SearchesThePlateAndProjectsTheManufacturer()
    {
        var page = await SpecificationEvaluator.Default.GetQuery(_context.Set<Vehicle>(),
            new PagedVehicleSpecification(new VehicleParameters { SearchCriteria = "ABC" })).ToListAsync();

        Assert.Equal("SCANIA", Assert.Single(page).Manufacturer!.Description);
    }

    [Fact]
    public async Task UserPage_FiltersByRole()
    {
        // No users are seeded; the point is that the role filter (UserRoles.Any) translates.
        var page = await SpecificationEvaluator.Default.GetQuery(_context.Set<User>(),
            new PagedUserSpecification(new UserParameters { RoleId = 1 })).ToListAsync();

        Assert.Empty(page);
    }

    [Fact]
    public async Task ReservationPageAndCount_NarrowToOneTrip()
    {
        var parameters = new ReservationParameters { TravelDate = DatabaseSeeder.UpcomingTravelDate };

        var page = await SpecificationEvaluator.Default.GetQuery(_context.Set<Reservation>(),
            new PagedReservationSpecification(DatabaseSeeder.ScheduleId, parameters)).ToListAsync();
        var count = await SpecificationEvaluator.Default.GetQuery(_context.Set<Reservation>(),
            new ReservationSpecification(DatabaseSeeder.ScheduleId, parameters)).CountAsync();

        var reservation = Assert.Single(page);
        Assert.Equal("Terminal Salitre", reservation.Schedule!.Route!.DestinationPoint!.Detail);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task TripCount_CountsTheActiveReservationsOfOneTrip()
    {
        var count = await SpecificationEvaluator.Default.GetQuery(_context.Set<Reservation>(),
            new TripReservationSpecification(DatabaseSeeder.ScheduleId, DatabaseSeeder.UpcomingTravelDate)).CountAsync();

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task ReservationDetail_LoadsTheTripAndIdentificationType()
    {
        var reservation = await SpecificationEvaluator.Default.GetQuery(_context.Set<Reservation>(),
            new ReservationDetailSpecification(DatabaseSeeder.UpcomingReservationId)).SingleAsync();

        Assert.Equal("Medellín", reservation.Schedule.Route.OriginPoint.City.Description);
        Assert.Equal("Cédula de ciudadanía", reservation.IdentificationType.Description);
    }

    [Fact]
    public async Task DuplicateChecks_FindTheSeededRows()
    {
        Assert.True(await SpecificationEvaluator.Default.GetQuery(_context.Set<GoTransport.Domain.Entities.Bas.Department>(), new DuplicationDepartmentSpecification("Antioquia", 0)).AnyAsync());
        Assert.True(await SpecificationEvaluator.Default.GetQuery(_context.Set<GoTransport.Domain.Entities.Bas.City>(), new DuplicationCitySpecification("Medellín", 1, 0)).AnyAsync());
        Assert.True(await SpecificationEvaluator.Default.GetQuery(_context.Set<Route>(), new DuplicationRouteSpecification(1, 2, null)).AnyAsync());
        Assert.True(await SpecificationEvaluator.Default.GetQuery(_context.Set<Vehicle>(), new DuplicationVehicleSpecification("ABC123", null)).AnyAsync());
        Assert.False(await SpecificationEvaluator.Default.GetQuery(_context.Set<Vehicle>(), new DuplicationVehicleSpecification("ABC123", 1)).AnyAsync());
    }

    [Fact]
    public async Task UniqueIndexes_RejectADuplicateLicensePlate()
    {
        _context.Add(new Vehicle { VehicleId = 2, ManufacturerId = 1, LicensePlate = "ABC123", Model = 2023, Capacity = 30 });

        await Assert.ThrowsAsync<DbUpdateException>(() => _context.SaveChangesAsync());
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private sealed class NoSignedInUser : ICurrentUser
    {
        public string? UserId => null;
    }

    /// <summary>
    /// The production model with the SQL Server details SQLite cannot create: the database collation,
    /// varchar(max) columns and identity keys typed int/bigint (SQLite auto-increments only INTEGER).
    /// Everything else (tables, keys, indexes, queries) is the real model.
    /// </summary>
    private sealed class SqliteApplicationDbContext : ApplicationDbContext
    {
        public SqliteApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options, new NoSignedInUser(), TimeProvider.System)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Model.RemoveAnnotation("Relational:Collation");
            foreach (var property in builder.Model.GetEntityTypes().SelectMany(entity => entity.GetProperties()))
            {
                if (property.GetColumnType()?.Contains("(max)") == true)
                    property.SetColumnType("TEXT");
            }

            foreach (var key in builder.Model.GetEntityTypes().Select(entity => entity.FindPrimaryKey()).OfType<IMutableKey>())
            {
                if (key.Properties is [{ ClrType: var type } single] && (type == typeof(int) || type == typeof(long)))
                    single.SetColumnType("INTEGER");
            }
        }
    }
}
