using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using SDMNG.Data;
using SDMNG.Models;
using SpeedDiesel.Controllers;
using Xunit;
using static QRCoder.PayloadGenerator.SwissQrCode;

namespace TestSDMNG.ControllerTests
{
    public class BusControllerTests
    {
        

        private AppDbContext GetContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private IWebHostEnvironment GetMockWebHostEnvironment(string path)
        {
            var mock = new Mock<IWebHostEnvironment>();
            mock.Setup(m => m.WebRootPath).Returns(path);
            mock.Setup(m => m.EnvironmentName).Returns("Development"); 
            return mock.Object;
        }


        [Fact]
        public async Task Create_Post_SavesBus_WhenImageUploaded()
        {
            var context = GetContext();
            var logger = Mock.Of<ILogger<AdminMessage>>();
            var webHost = GetMockWebHostEnvironment(Path.GetTempPath());
            var controller = new BusController(context, logger, webHost);

            var contact = new SDMNG.Models.Contact
            {
                Id = "driver123",
                FullName = "Teszt Sándor",
                UserName = "driver@test.com",
                Email = "driver@test.com",
                Street = "Teszt utca 12.",
                Zipcode = "1234",
                Active = true,
                PWString = "driverpass123"
            };

            var bus = new Bus
            {
                BusNumber = "B123",
                Capacity = 50,
                BusType = "Electric",
                 ContactId = contact.Id
            };

            var fileMock = new Mock<IFormFile>();
            var content = "Fake image content";
            var fileName = "test.png";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(stream.Length);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default)).Returns((Stream target, System.Threading.CancellationToken token) => stream.CopyToAsync(target));

            var result = await controller.Create(bus, fileMock.Object);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Single(context.Buses);
        }

        [Fact]
        public async Task Modify_Post_UpdatesBus()
        {
            var context = GetContext();
            var logger = Mock.Of<ILogger<AdminMessage>>();
            var webHost = GetMockWebHostEnvironment(Path.GetTempPath());

           var attachment = new Attachment
            {
                Id = "att001",
                FileName = "maintenance.pdf",
                FileType = "application/pdf",
                FilePath = "/files/bus001/maintenance.pdf",
                BusId = "bus001"
            };

            var contact = new SDMNG.Models.Contact
            {
                Id = "contact001",
                UserName = "driver@example.com",
                Email = "driver@example.com",
                FullName = "John Doe",
                PWString = "securepw123",
                Street = "Main Street 1",
                Zipcode = "12345",
                Active = true
            };

            var originalBus = new Bus
            {
                BusId = Guid.NewGuid().ToString(),
                BusNumber = "B001",
                Capacity = 40,
                BusType = "Diesel",
                ImageUrl = "/old/path.png",
                ContactId = contact.Id

            };

            context.Contacts.Add(contact);  
            context.Buses.Add(originalBus);
            context.Attachments.Add(attachment);
            await context.SaveChangesAsync();

            var controller = new BusController(context, logger, webHost);
            originalBus.BusNumber = "B002";
            originalBus.Capacity = 60;

            var result = await controller.Modify(originalBus, null);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);

            var updatedBus = context.Buses.First();
            Assert.Equal("B002", updatedBus.BusNumber);
            Assert.Equal(60, updatedBus.Capacity);
        }

        
    }
}
