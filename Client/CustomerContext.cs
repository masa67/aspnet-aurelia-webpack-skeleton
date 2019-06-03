using Client.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Client
{
    public class CustomerContext : DbContext
    {
        public CustomerContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Address> Addresses { get; set; }
        public DbSet<ContactPerson> ContactPersons { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<PhoneNumber> PhoneNumbers { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<RoleGroup> RoleGroup { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().HasData(new Customer
            {
                Id = 1,
                Name = "Company1",
                NameExtension = "Company1 extension"
            }, new Customer
            {
                Id = 2,
                Name = "Company2",
                NameExtension = "Company2 extension"
            });

            modelBuilder.Entity<Address>().HasData(new Address
            {
                Id = 1,
                City = "London",
                CustomerId = 1

            }, new Address
            {
                Id = 2,
                City = "New York",
                CustomerId = 2
            });

            modelBuilder.Entity<ContactPerson>().HasData(new ContactPerson
            {
                Id = 1,
                Name = "John",
                CustomerId = 1
            }, new ContactPerson
            {
                Id = 2,
                Name = "Jane",
                CustomerId = 1
            });

            modelBuilder.Entity<PhoneNumber>().HasData(new PhoneNumber
            {
                Id = 1,
                Value = "123",
                ContactPersonId = 1
            }, new PhoneNumber
            {
                Id = 2,
                Value = "456",
                ContactPersonId = 1
            }, new PhoneNumber
            {
                Id = 3,
                Value = "999",
                ContactPersonId = 2
            });

            modelBuilder.Entity<RoleGroup>().HasData(new RoleGroup
            {
                Id = 1,
                ContactPersonId = 1
            },
            new RoleGroup
            {
                Id = 2,
                ContactPersonId = 2
            });

            modelBuilder.Entity<Role>().HasData(new Role
            {
                Id = 1,
                Value = "editor",
                RoleGroupId = 1
            },
            new Role
            {
                Id = 2,
                Value = "author",
                RoleGroupId = 1
            },
            new Role
            {
                Id = 3,
                Value = "admin",
                RoleGroupId = 2
            },
            new Role
            {
                Id = 4,
                Value = "editor",
                RoleGroupId = 2
            });
        }
    }
}
