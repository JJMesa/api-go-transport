using GoTransport.Application.Dtos.Point;
using GoTransport.Application.Dtos.Reservation;
using GoTransport.Application.Dtos.Route;
using GoTransport.Application.Dtos.Schedule;
using GoTransport.Application.Dtos.User;
using GoTransport.Application.Validators.Point;
using GoTransport.Application.Validators.Reservation;
using GoTransport.Application.Validators.Route;
using GoTransport.Application.Validators.Schedule;
using GoTransport.Application.Validators.User;

namespace GoTransport.Api.Unit.Tests.TestCases;

/// <summary>
/// The rules of the validators that have no dedicated test class: each accepts a well-formed request and
/// rejects the input its rules exist for. Validators run before the services, so these 400s never reach
/// the business rules or the database.
/// </summary>
public class ValidatorRulesTests
{
    private static readonly Guid ScheduleId = Guid.NewGuid();

    [Fact]
    public void ReservationCreation_ShouldAcceptAWellFormedBooking()
    {
        Assert.True(new ReservationCreationValidator().Validate(Booking()).IsValid);
    }

    [Theory]
    [InlineData(nameof(ReservationCreationDto.ScheduleId))]
    [InlineData(nameof(ReservationCreationDto.PassengerFirstName))]
    [InlineData(nameof(ReservationCreationDto.PassengerIdentification))]
    [InlineData(nameof(ReservationCreationDto.Detail))]
    public void ReservationCreation_ShouldRequireEachField(string missing)
    {
        var booking = Booking();
        switch (missing)
        {
            case nameof(ReservationCreationDto.ScheduleId): booking.ScheduleId = Guid.Empty; break;
            case nameof(ReservationCreationDto.PassengerFirstName): booking.PassengerFirstName = ""; break;
            case nameof(ReservationCreationDto.PassengerIdentification): booking.PassengerIdentification = ""; break;
            case nameof(ReservationCreationDto.Detail): booking.Detail = ""; break;
        }

        var actual = new ReservationCreationValidator().Validate(booking);

        Assert.Contains(actual.Errors, error => error.PropertyName == missing);
    }

    [Fact]
    public void ReservationCreation_ShouldLimitTheDetailLength()
    {
        var booking = Booking();
        booking.Detail = new string('x', 65);

        Assert.False(new ReservationCreationValidator().Validate(booking).IsValid);
    }

    [Fact]
    public void ReservationUpdate_ShouldRequireTheReservationAndSchedule()
    {
        var update = new ReservationUpdateDto { ReservationId = Guid.Empty, ScheduleId = Guid.Empty, ReservationDate = DateTime.Today, Detail = "x" };

        var actual = new ReservationUpdateValidator().Validate(update);

        Assert.Contains(actual.Errors, error => error.PropertyName == nameof(ReservationUpdateDto.ReservationId));
        Assert.Contains(actual.Errors, error => error.PropertyName == nameof(ReservationUpdateDto.ScheduleId));
    }

    [Fact]
    public void PassengerLookup_ShouldRequireTheDocumentNumber()
    {
        Assert.False(new PassengerLookupValidator().Validate(new PassengerLookupDto { PassengerIdentification = "" }).IsValid);
        Assert.True(new PassengerLookupValidator().Validate(new PassengerLookupDto { PassengerIdentification = "1000123456" }).IsValid);
    }

    [Theory]
    [InlineData(1, 2, true)]
    [InlineData(1, 1, false)]
    [InlineData(0, 2, false)]
    public void RouteCreation_ShouldConnectTwoDifferentPoints(int origin, int destination, bool expectedValid)
    {
        var route = new RouteCreationDto { Description = "Medellín - Bogotá", OriginPointId = origin, DestinationPointId = destination };

        Assert.Equal(expectedValid, new RouteCreationValidator().Validate(route).IsValid);
    }

    [Theory]
    [InlineData(64, true)]
    [InlineData(65, false)]
    public void PointCreation_ShouldRespectTheDetailColumnLength(int length, bool expectedValid)
    {
        var point = new PointCreationDto { CityId = 1, Detail = new string('p', length) };

        Assert.Equal(expectedValid, new PointCreationValidator().Validate(point).IsValid);
    }

    [Fact]
    public void PointUpdate_ShouldRequireThePoint()
    {
        var point = new PointUpdateDto { PointId = 0, CityId = 1, Detail = "Terminal", IsActive = true };

        Assert.Contains(new PointUpdateValidator().Validate(point).Errors, error => error.PropertyName == nameof(PointUpdateDto.PointId));
    }

    [Theory]
    [InlineData("06:00", "15:00", true)]
    [InlineData("22:00", "06:00", true)]   // overnight
    [InlineData("00:00", "08:00", true)]   // midnight departure
    [InlineData("08:00", "08:00", false)]  // no Trip
    [InlineData("1.02:00", "08:00", false)] // not a time of day
    public void ScheduleCreation_ShouldAcceptTimesOfDayThatDiffer(string departure, string arrival, bool expectedValid)
    {
        var schedule = new ScheduleCreationDto { DepartureTime = TimeSpan.Parse(departure), ArrivalTime = TimeSpan.Parse(arrival), VehicleId = 1, RouteId = 1 };

        Assert.Equal(expectedValid, new ScheduleCreationValidator().Validate(schedule).IsValid);
    }

    [Fact]
    public void ScheduleUpdate_ShouldRejectEqualTimes()
    {
        var schedule = new ScheduleUpdateDto { ScheduleId = ScheduleId, DepartureTime = TimeSpan.FromHours(9), ArrivalTime = TimeSpan.FromHours(9), VehicleId = 1, RouteId = 1, IsActive = true };

        Assert.False(new ScheduleUpdateValidator().Validate(schedule).IsValid);
    }

    [Theory]
    [InlineData(2, true)]
    [InlineData(0, false)]
    public void UserUpdate_ShouldRequireARole(long roleId, bool expectedValid)
    {
        var user = new UserUpdateDto { UserId = 1, FirstName = "Ana", LastName = "Ruiz", IsActive = true, RoleId = roleId };

        Assert.Equal(expectedValid, new UserUpdateValidator().Validate(user).IsValid);
    }

    private static ReservationCreationDto Booking() => new()
    {
        ScheduleId = ScheduleId,
        ReservationDate = DateTime.Today.AddDays(3),
        PassengerFirstName = "John",
        PassengerLastName = "Doe",
        IdentificationTypeId = 1,
        PassengerIdentification = "1000123456",
        Detail = "Window seat"
    };
}
