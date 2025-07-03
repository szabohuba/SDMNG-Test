using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SDMNG.Data;
using SDMNG.Models;
using Xunit;

namespace TestSDMNG.ModelTests
{
    public class ContactTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task AddContact_SavesCorrectly()
        {
            using var context = GetInMemoryDbContext();

            var contact = new Contact
            {
                Id = "contact001",
                UserName = "testuser",
                Email = "test@example.com",
                FullName = "Test Elek",
                PWString = "securepassword",
                Street = "Fő utca 1.",
                Zipcode = "1234",
                Active = true
            };

            context.Contacts.Add(contact);
            await context.SaveChangesAsync();

            var savedContact = await context.Contacts.FirstOrDefaultAsync(c => c.Id == "contact001");

            Assert.NotNull(savedContact);
            Assert.Equal("testuser", savedContact.UserName);
            Assert.Equal("Test Elek", savedContact.FullName);
        }

        [Fact]
        public async Task UpdateContact_ModifiesValues()
        {
            using var context = GetInMemoryDbContext();

            var contact = new Contact
            {
                Id = "contact002",
                UserName = "originaluser",
                Email = "original@example.com",
                FullName = "Original Name",
                PWString = "originalpw",
                Street = "Old Street 5.",
                Zipcode = "9999",
                Active = false
            };

            context.Contacts.Add(contact);
            await context.SaveChangesAsync();

            contact.FullName = "Updated Name";
            contact.Active = true;
            context.Contacts.Update(contact);
            await context.SaveChangesAsync();

            var updatedContact = await context.Contacts.FirstOrDefaultAsync(c => c.Id == "contact002");

            Assert.Equal("Updated Name", updatedContact.FullName);
            Assert.True(updatedContact.Active);
        }

        [Fact]
        public async Task DeleteContact_RemovesFromDatabase()
        {
            using var context = GetInMemoryDbContext();

            var contact = new Contact
            {
                Id = "contact003",
                UserName = "deleteuser",
                Email = "delete@example.com",
                FullName = "Delete Me",
                PWString = "pwdelete",
                Street = "Delete Street 9.",
                Zipcode = "0000",
                Active = true
            };

            context.Contacts.Add(contact);
            await context.SaveChangesAsync();

            context.Contacts.Remove(contact);
            await context.SaveChangesAsync();

            var deleted = await context.Contacts.FindAsync("contact003");
            Assert.Null(deleted);
        }
    }
}
