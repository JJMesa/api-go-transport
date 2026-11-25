using System.Net;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.Point;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Services;
using GoTransport.Application.Specifications.Points;
using GoTransport.Domain.Entities.App;
using GoTransport.Domain.Entities.Bas;
using GoTransport.Domain.Interfaces;
using Moq;

namespace GoTransport.Api.Unit.Tests.TestCases;

public class PointServiceTests
{
    private readonly Mock<IRepository<Point>> _pointRepositoryMock = new();
    private readonly IPointService _pointService;

    public PointServiceTests()
    {
        _pointService = new PointService(_pointRepositoryMock.Object, new Mock<ICacheService<Point>>().Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnThePointWithItsCity()
    {
        // Arrange
        var point = new Point
        {
            PointId = 1,
            Detail = "Terminal del Norte",
            CityId = 1,
            City = new City { CityId = 1, Description = "Medellín", DepartmentId = 1, IsActive = true },
            IsActive = true
        };
        _pointRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<PointSpecification>(), It.IsAny<CancellationToken>())).ReturnsAsync(point);

        // Act
        var actual = await _pointService.GetByIdAsync(1, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        Assert.Equal("Terminal del Norte", actual.Data!.Detail);
        Assert.Equal("Medellín", actual.Data.City!.Description);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnBadRequestForUrlAndBodyIdMismatch()
    {
        // Act
        var actual = await _pointService.UpdateAsync(2, new PointUpdateDto { PointId = 1, Detail = "Zona Sur", CityId = 1, IsActive = true }, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, actual.HttpCode);
        Assert.Contains(ErrorMessages.UrlAndBodyIdNotEqual, actual.Errors!);
        _pointRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Point>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
