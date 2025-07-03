
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SDMNG.Controllers;
using SDMNG.Data;
using SDMNG.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace TestSDMNG.ControllerTests
{
    public class TransportRouteControllerTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);
            return context;
        }

        [Fact]
        public async Task Index_ReturnsViewWithRoutes()
        {
            var context = GetDbContext();
            var logger = Mock.Of<ILogger<TransportRouteController>>();

            var stop = new Stop
            {
                StopId = "stop1",
                StopName = "Main Street",
                Latitude = 47,
                Longitude = 19
            };

            var transportRoute = new TransportRoute
            {
                TransportRoutesId = "route1",
                TransportRoutesName = "Route A",
                RouteStop = new List<RouteStop>()
            };

            var routeStop = new RouteStop
            {
                RouteStopId = "rs1",
                TransportRouteId = transportRoute.TransportRoutesId,
                StopId = stop.StopId,
                RoutStopName = stop.StopName,
                SequenceNumber = 1,
                Stop = stop
            };

            transportRoute.RouteStop.Add(routeStop);

            context.Stops.Add(stop);
            context.TransportRoutes.Add(transportRoute);
            context.RouteStops.Add(routeStop);
            await context.SaveChangesAsync();

            var controller = new TransportRouteController(context, logger);

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<TransportRoute>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal("Route A", model[0].TransportRoutesName);
        }

        [Fact]
        public async Task Create_Post_CreatesRoute()
        {
            var context = GetDbContext();
            var logger = Mock.Of<ILogger<TransportRouteController>>();

            var controller = new TransportRouteController(context, logger);

            var route = new TransportRoute
            {
                TransportRoutesId = Guid.NewGuid().ToString(),
                TransportRoutesName = "Test Route"
            };

            var result = await controller.Create(route);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Single(context.TransportRoutes);
        }

        [Fact]
        public async Task DeleteConfirmed_DeletesRoute_WhenNoSchedule()
        {
            var context = GetDbContext();
            var logger = Mock.Of<ILogger<TransportRouteController>>();

            var route = new TransportRoute
            {
                TransportRoutesId = "route2",
                TransportRoutesName = "To Delete",
                RouteStop = new List<RouteStop>()
            };

            context.TransportRoutes.Add(route);
            await context.SaveChangesAsync();

            var controller = new TransportRouteController(context, logger);

            var result = await controller.DeleteConfirmed("route2");

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Empty(context.TransportRoutes);
        }
    }
}
