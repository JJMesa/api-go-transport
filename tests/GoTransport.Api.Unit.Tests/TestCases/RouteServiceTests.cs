using System.Net;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.Route;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Services;
using GoTransport.Application.Specifications.Routes;
using GoTransport.Domain.Entities.App;
using GoTransport.Domain.Interfaces;
using Moq;

namespace GoTransport.Api.Unit.Tests.TestCases;

public class RouteServiceTests
{
    private readonly Mock<IRepository<Route>> _routeRepositoryMock = new();
    private readonly Mock<ICacheService<Route>> _cacheServiceMock = new();
    private readonly IRouteService _routeService;

    public RouteServiceTests()
    {
        _routeService = new RouteService(_routeRepositoryMock.Object, _cacheServiceMock.Object);
    }

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_ShouldSaveTheRouteAndDropTheCachedList()
    {
        // Arrange
        var route = new Route { RouteId = 1, Description = "Medellín - Bogotá", OriginPointId = 1, DestinationPointId = 2, IsActive = true };
        _routeRepositoryMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(route);

        // Act
        var actual = await _routeService.UpdateAsync(1, Update(originPointId: 1, destinationPointId: 3, description: "Medellín  -  Cali"), CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        Assert.Equal(3, route.DestinationPointId);
        Assert.Equal("Medellín - Cali", route.Description);
        _routeRepositoryMock.Verify(x => x.UpdateAsync(route, It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(x => x.Remove(CacheKey.Routes), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnConflictWhenAnotherRouteHasThePoints()
    {
        // Arrange
        _routeRepositoryMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new Route { RouteId = 1 });
        _routeRepositoryMock.Setup(x => x.AnyAsync(It.IsAny<DuplicationRouteSpecification>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var actual = await _routeService.UpdateAsync(1, Update(originPointId: 1, destinationPointId: 2), CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, actual.HttpCode);
        Assert.Contains(ErrorMessages.DuplicateRoute, actual.Errors!);
        _routeRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Route>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNotFoundForUnknownRoute()
    {
        // Act
        var actual = await _routeService.UpdateAsync(1, Update(originPointId: 1, destinationPointId: 2), CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, actual.HttpCode);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnBadRequestWhenUrlAndBodyIdsDiffer()
    {
        // Act
        var actual = await _routeService.UpdateAsync(2, Update(originPointId: 1, destinationPointId: 2), CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, actual.HttpCode);
        Assert.Contains(ErrorMessages.UrlAndBodyIdNotEqual, actual.Errors!);
    }

    #endregion UpdateAsync

    private static RouteUpdateDto Update(int originPointId, int destinationPointId, string description = "Route") => new()
    {
        RouteId = 1,
        Description = description,
        OriginPointId = originPointId,
        DestinationPointId = destinationPointId,
        IsActive = true
    };
}
