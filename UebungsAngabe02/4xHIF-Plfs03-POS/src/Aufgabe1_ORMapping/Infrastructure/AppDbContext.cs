using Aufgabe1_ORMapping.Model;
using Bogus;
using Bogus.DataSets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aufgabe1_ORMapping.Infrastructure;


public class AppDbContext : DbContext {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Todo: only for demonstration, should be replaced!
    public DbSet<Model.Person> People => Set<Model.Person>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //modelBuilder.Entity<Person>()...

        base.OnModelCreating(modelBuilder);
    }

    // Todo: implement queries
}