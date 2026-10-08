using GoTransport.Application.Dtos.City;
using GoTransport.Application.Dtos.User;
using GoTransport.Application.Dtos.Vehicle;
using GoTransport.Application.Mappings;
using GoTransport.Domain.Entities.App;
using GoTransport.Domain.Entities.Bas;

namespace GoTransport.Api.Unit.Tests.TestCases;

public class MappingExtensionsTests
{
    [Fact]
    public void CityToDto_ShouldMapNestedDepartment()
    {
        // Arrange
        var city = new City
        {
            CityId = 3,
            Description = "city 3",
            DepartmentId = 2,
            Department = new Department { DepartmentId = 2, Description = "department 2", IsActive = true },
            IsActive = true
        };

        // Act
        var actual = city.ToDto();

        // Assert
        Assert.Equal(3, actual.CityId);
        Assert.Equal("city 3", actual.Description);
        Assert.True(actual.IsActive);
        Assert.Equal(2, actual.Department.DepartmentId);
        Assert.Equal("department 2", actual.Department.Description);
        Assert.True(actual.Department.IsActive);
    }

    [Fact]
    public void ToDto_ShouldMapUnloadedNavigationToNullAndNullIsActiveToFalse()
    {
        // Arrange
        var reservation = new Reservation { ReservationId = Guid.NewGuid(), PassengerIdentification = "123" };

        // Act
        var actual = reservation.ToDto();

        // Assert
        Assert.Null(actual.Schedule);
        Assert.Null(actual.IdentificationType);
        Assert.False(actual.IsActive);
    }

    [Fact]
    public void ReservationToDto_ShouldMapWholeScheduleGraph()
    {
        // Arrange
        var city = new City { CityId = 1, Description = "city 1", Department = new Department { DepartmentId = 1, Description = "department 1" } };
        var reservation = new Reservation
        {
            ReservationId = Guid.NewGuid(),
            IdentificationType = new IdentificationType { IdentificationTypeId = 1, Description = "CC" },
            Schedule = new Schedule
            {
                ScheduleId = Guid.NewGuid(),
                Duration = TimeSpan.FromHours(2),
                Vehicle = new Vehicle { VehicleId = 4, LicensePlate = "ABC123", Manufacturer = new Manufacturer { ManufacturerId = 9, Description = "manufacturer 9" } },
                Route = new Route
                {
                    RouteId = 5,
                    OriginPoint = new Point { PointId = 1, Detail = "origin", City = city },
                    DestinationPoint = new Point { PointId = 2, Detail = "destination", City = city }
                }
            }
        };

        // Act
        var actual = reservation.ToDto();

        // Assert
        Assert.Equal("CC", actual.IdentificationType.Description);
        Assert.Equal(TimeSpan.FromHours(2), actual.Schedule.Duration);
        Assert.Equal("manufacturer 9", actual.Schedule.Vehicle!.Manufacturer.Description);
        Assert.Equal("origin", actual.Schedule.Route!.OriginPoint.Detail);
        Assert.Equal("department 1", actual.Schedule.Route.DestinationPoint.City.Department.Description);
    }

    [Fact]
    public void UserToDto_ShouldMapIdToUserId()
    {
        // Arrange
        var user = new User { Id = 10, FirstName = "Jane", LastName = "Doe", Email = "jane@test.com", IsActive = null };

        // Act
        var actual = user.ToDto();

        // Assert
        Assert.Equal(10, actual.UserId);
        Assert.Equal("jane@test.com", actual.Email);
        Assert.Null(actual.IsActive);
    }

    [Fact]
    public void UserCreationToEntity_ShouldUseEmailAsUserName()
    {
        // Arrange
        var userCreation = new UserCreationDto { FirstName = "Jane", LastName = "Doe", Email = "jane@test.com", Password = "Secret#123", RoleId = 1 };

        // Act
        var actual = userCreation.ToEntity();

        // Assert
        Assert.Equal("jane@test.com", actual.Email);
        Assert.Equal("jane@test.com", actual.UserName);
        Assert.Null(actual.PasswordHash);
    }

    [Fact]
    public void RoleToDto_ShouldMapIdToRoleId()
    {
        // Arrange
        var role = new Role { Id = 2, Name = "Administrador" };

        // Act
        var actual = role.ToDto();

        // Assert
        Assert.Equal(2, actual.RoleId);
        Assert.Equal("Administrador", actual.Name);
    }

    [Fact]
    public void CityUpdateApplyTo_ShouldUpdateFieldsAndKeepKeyAndNavigation()
    {
        // Arrange
        var department = new Department { DepartmentId = 1, Description = "department 1" };
        var city = new City { CityId = 1, Description = "city 1", DepartmentId = 1, Department = department, IsActive = true };
        var cityUpdate = new CityUpdateDto { CityId = 1, Description = "city 1 updated", DepartmentId = 2, IsActive = false };

        // Act
        cityUpdate.ApplyTo(city);

        // Assert
        Assert.Equal(1, city.CityId);
        Assert.Equal("city 1 updated", city.Description);
        Assert.Equal(2, city.DepartmentId);
        Assert.False(city.IsActive);
        Assert.Same(department, city.Department);
    }

    [Fact]
    public void VehicleUpdateApplyTo_ShouldOnlyUpdateCapacity()
    {
        // Arrange
        var vehicle = new Vehicle { VehicleId = 4, ManufacturerId = 9, LicensePlate = "ABC123", Model = 2020, Capacity = 40, IsActive = true };
        var vehicleUpdate = new VehicleUpdateDto { VehicleId = 4, Capacity = 12 };

        // Act
        vehicleUpdate.ApplyTo(vehicle);

        // Assert
        Assert.Equal(12, vehicle.Capacity);
        Assert.Equal("ABC123", vehicle.LicensePlate);
        Assert.Equal(2020, vehicle.Model);
        Assert.Equal(9, vehicle.ManufacturerId);
        Assert.True(vehicle.IsActive);
    }
}
