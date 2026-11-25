using GoTransport.Application.Parameters;
using GoTransport.Application.Specifications.Reservations;
using GoTransport.Application.Specifications.Users;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Api.Unit.Tests.TestCases;

/// <summary>
/// Each branch of the shared User and Reservation filters, evaluated in memory (the relational
/// translation is covered by RelationalSpecificationTests in the component tests).
/// </summary>
public class FilterSpecificationTests
{
    private static readonly Guid ScheduleId = Guid.NewGuid();
    private static readonly DateTime TravelDate = new(2026, 12, 24);

    [Theory]
    [InlineData(null, null, null, 3)]
    [InlineData(2L, null, null, 1)]
    [InlineData(null, "Ru", null, 1)]
    [InlineData(null, "Gómez", null, 1)]
    [InlineData(null, null, false, 1)]
    [InlineData(1L, null, true, 1)]
    public void UserFilter_ShouldCombineRoleSearchAndActiveFilters(long? roleId, string? search, bool? isActive, int expected)
    {
        // Arrange
        var users = new List<User>
        {
            UserWithRole("Ana", "Ruiz", roleId: 1, isActive: true),
            UserWithRole("Luis", "Gómez", roleId: 2, isActive: true),
            UserWithRole("Eva", "Mora", roleId: 1, isActive: false)
        };
        var parameters = new UserParameters { RoleId = (int?)roleId, SearchCriteria = search, IsActive = isActive };

        // Act
        var actual = new UserSpecification(parameters).Evaluate(users).Count();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(null, null, false, 3)]
    [InlineData("Doe", null, false, 2)]
    [InlineData("100099", null, false, 1)]
    [InlineData(null, true, false, 2)]
    [InlineData(null, null, true, 2)]
    public void ReservationFilter_ShouldCombineSearchActiveAndTravelDateFilters(string? search, bool? isActive, bool onTravelDate, int expected)
    {
        // Arrange
        var reservations = new List<Reservation>
        {
            Booking("John", "Doe", "1000123456", TravelDate, isActive: true),
            Booking("Jane", "Doe", "1000654321", TravelDate.AddDays(1), isActive: true),
            Booking("Mario", "Pérez", "1000999999", TravelDate, isActive: false),
            Booking("Other", "Schedule", "1", TravelDate, isActive: true, scheduleId: Guid.NewGuid())
        };
        var parameters = new ReservationParameters { SearchCriteria = search, IsActive = isActive, TravelDate = onTravelDate ? TravelDate : null };

        // Act
        var actual = new ReservationSpecification(ScheduleId, parameters).Evaluate(reservations).Count();

        // Assert
        Assert.Equal(expected, actual);
    }

    private static User UserWithRole(string firstName, string lastName, long roleId, bool isActive)
    {
        var user = new User { FirstName = firstName, LastName = lastName, IsActive = isActive };
        user.UserRoles.Add(new UserRole { RoleId = roleId });
        return user;
    }

    private static Reservation Booking(string firstName, string lastName, string document, DateTime travelDate, bool isActive, Guid? scheduleId = null) => new()
    {
        ReservationId = Guid.NewGuid(),
        ScheduleId = scheduleId ?? ScheduleId,
        PassengerFirstName = firstName,
        PassengerLastName = lastName,
        PassengerIdentification = document,
        ReservationDate = travelDate,
        IsActive = isActive
    };
}
