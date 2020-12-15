using Microsoft.EntityFrameworkCore;
using System;

namespace ScopedFilterMessageScheduler
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options)
            : base(options)
        {
        }

        public DbSet<Person> Persons { get; set; }
    }

    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
