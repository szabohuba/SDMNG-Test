using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SDMNG.Controllers;
using SDMNG.Data;
using SDMNG.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace TestSDMNG.ControllerTests
{
    public class AdminTaskControllerTests
    {
        private AdminTaskController CreateControllerWithData(List<AdminTask> seedData)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);
            context.AdminTasks.AddRange(seedData);
            context.SaveChanges();

            var config = new Mock<IConfiguration>();
            var logger = new Mock<ILogger<AdminTask>>();

            return new AdminTaskController(context, config.Object, logger.Object);
        }

        [Fact]
        public void Index_ReturnsAllTasks_OrderedCorrectly()
        {
            var tasks = new List<AdminTask>
            {
                new AdminTask { Id = "1", Title = "Task A", DueUntil = DateTime.UtcNow.AddDays(1), IsResolved = false },
                new AdminTask { Id = "2", Title = "Task B", DueUntil = DateTime.UtcNow.AddDays(2), IsResolved = true }
            };

            var controller = CreateControllerWithData(tasks);

            var result = controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<AdminTask>>(viewResult.Model);

            Assert.Equal(2, model.Count);
            Assert.False(model.First().IsResolved); 
        }

        [Fact]
        public void Detail_ReturnsCorrectTask_WhenIdExists()
        {
            var task = new AdminTask { Id = "123", Title = "Test Task" };
            var controller = CreateControllerWithData(new List<AdminTask> { task });

            var result = controller.Detail("123");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AdminTask>(viewResult.Model);

            Assert.Equal("Test Task", model.Title);
        }

        [Fact]
        public void Create_Post_AddsTask_AndRedirects()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new AppDbContext(options);
            var config = new Mock<IConfiguration>();
            var logger = new Mock<ILogger<AdminTask>>();
            var controller = new AdminTaskController(context, config.Object, logger.Object);

            var task = new AdminTask
            {
                Title = "New Task",
                Description = "Something important",
                DueUntil = DateTime.UtcNow.AddDays(3)
            };

            var result = controller.Create(task);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);

            Assert.Single(context.AdminTasks);
            Assert.Equal("New Task", context.AdminTasks.First().Title);
        }

       
    }
}
