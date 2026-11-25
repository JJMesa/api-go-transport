using GoTransport.Application.Dtos.City;
using GoTransport.Application.Dtos.Department;
using GoTransport.Application.Dtos.Vehicle;
using GoTransport.Application.Parameters;
using GoTransport.Application.Validators.City;
using GoTransport.Application.Validators.Department;
using GoTransport.Application.Validators.Vehicle;

namespace GoTransport.Api.Unit.Tests.TestCases;

/// <summary>
/// Input that the database would reject (or that makes no sense) must fail validation with a 400,
/// not reach SQL Server and fail there.
/// </summary>
public class ValidatorColumnLimitsTests
{
    [Theory]
    [InlineData(32, true)]
    [InlineData(33, false)]
    public void CityCreation_ShouldRespectTheDescriptionColumnLength(int length, bool expectedValid)
    {
        var actual = new CityCreationValidator().Validate(new CityCreationDto { Description = new string('a', length), DepartmentId = 1 });

        Assert.Equal(expectedValid, actual.IsValid);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("Antioquia", true)]
    [InlineData("A department name over thirty-two characters", false)]
    public void DepartmentCreation_ShouldRequireADescriptionThatFitsTheColumn(string description, bool expectedValid)
    {
        var actual = new DepartmentCreationValidator().Validate(new DepartmentCreationDto { Description = description });

        Assert.Equal(expectedValid, actual.IsValid);
    }

    [Theory]
    [InlineData(-5, false)]
    [InlineData(0, false)]
    [InlineData(40, true)]
    public void VehicleCreation_ShouldRequireAPositiveCapacity(int capacity, bool expectedValid)
    {
        var actual = new VehicleCreationValidator().Validate(new VehicleCreationDto { ManufacturerId = 1, LicensePlate = "ABC123", Model = 2022, Capacity = capacity });

        Assert.Equal(expectedValid, actual.IsValid);
    }

    [Theory]
    [InlineData(1949, false)]
    [InlineData(2022, true)]
    [InlineData(3000, false)]
    public void VehicleCreation_ShouldRequireAPlausibleModelYear(int modelYear, bool expectedValid)
    {
        var actual = new VehicleCreationValidator().Validate(new VehicleCreationDto { ManufacturerId = 1, LicensePlate = "ABC123", Model = modelYear, Capacity = 40 });

        Assert.Equal(expectedValid, actual.IsValid);
    }

    [Fact]
    public void VehicleUpdate_ShouldRejectANegativeCapacity()
    {
        var actual = new VehicleUpdateValidator().Validate(new VehicleUpdateDto { VehicleId = 1, Capacity = -1 });

        Assert.False(actual.IsValid);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-3, 1)]
    [InlineData(4, 4)]
    public void PageNumber_ShouldBeClampedWhenBound(int requested, int expected)
    {
        // Model binding sets properties after the parameterless constructor.
        var parameters = new PaginationParameters { PageNumber = requested };

        Assert.Equal(expected, parameters.PageNumber);
    }
}
