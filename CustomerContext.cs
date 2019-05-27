using Client.Models;
using Microsoft.EntityFrameworkCore;

namespace Client
{
    public class CustomerContext : DbContext
    {
        public CustomerContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<ContactPerson> ContactPersons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().HasData(new Customer
            {
                CustomerId = 1,
                Name = "Company1",
                NameExtension = "Company1 extension"
            }, new Customer
            {
                CustomerId = 2,
                Name = "Company2",
                NameExtension = "Company2 extension"
            });

            modelBuilder.Entity<ContactPerson>().HasData(new ContactPerson
            {
                ContactPersonId = 1,
                Name = "John Doe"
            });
        }
    }
}
