using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Moq;
using SDMNG.Data;
using SDMNG.Models;
using SpeedDiesel.Controllers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class StopsControllerTests
{
    private StopsController CreateControllerWithData(List<Stop> seedData)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Unique per test
            .Options;

        var context = new AppDbContext(options);
        context.Stops.AddRange(seedData);
        context.SaveChanges();

        var config = new Mock<IConfiguration>();
        var logger = new Mock<ILogger<AdminMessage>>();

        return new StopsController(context, config.Object, logger.Object);
    }

    [Fact]
    public void Index_ReturnsViewResult_WithAllStops()
    {
        var controller = CreateControllerWithData(new List<Stop>
        {
            new Stop { StopId = "1", StopName = "Stop A", Latitude = 1, Longitude = 1 },
            new Stop { StopId = "2", StopName = "Stop B", Latitude = 2, Longitude = 2 }
        });

        var result = controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Stop>>(viewResult.Model);
        Assert.Equal(2, model.Count());
    }

    [Fact]
    public async Task UserDetail_ReturnsStop_WhenValidId()
    {
        var controller = CreateControllerWithData(new List<Stop>
        {
            new Stop { StopId = "1", StopName = "Stop A", Latitude = 1, Longitude = 1 }
        });

        var result = await controller.UserDetail("1");

        var viewResult = Assert.IsType<ViewResult>(result);
        var stop = Assert.IsType<Stop>(viewResult.Model);
        Assert.Equal("Stop A", stop.StopName);
    }

    [Fact]
    public async Task Create_AddsStop_AndRedirects()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);
        var config = new Mock<IConfiguration>();
        var logger = new Mock<ILogger<AdminMessage>>();
        var controller = new StopsController(context, config.Object, logger.Object);

        var stop = new Stop
        {
            StopName = "New Stop",
            Latitude = 3,
            Longitude = 4
        };

        var result = await controller.Create(stop);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Single(context.Stops);
        Assert.Equal("New Stop", context.Stops.First().StopName);
    }

    [Fact]
    public async Task Modify_UpdatesStop_WhenValidData()
    {
        var stopId = Guid.NewGuid().ToString();
        var stop = new Stop { StopId = stopId, StopName = "Old Name", Latitude = 1, Longitude = 1 };

        var controller = CreateControllerWithData(new List<Stop> { stop });

        stop.StopName = "Updated Name";
        var result = await controller.Modify(stopId, stop);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public async Task DeleteConfirmed_RemovesStop_WhenValidId()
    {
        var stopId = Guid.NewGuid().ToString();

        var controller = CreateControllerWithData(new List<Stop>
        {
            new Stop { StopId = stopId, StopName = "To Delete", Latitude = 1, Longitude = 1 }
        });

        var result = await controller.DeleteConfirmed(stopId);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }
}
