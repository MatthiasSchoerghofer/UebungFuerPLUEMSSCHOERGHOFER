using Aufgabe3_RestfulApi.Model;
using Microsoft.EntityFrameworkCore;

namespace Aufgabe3_RestfulApi.Infrastructure;

public class ProjectDBContext : DbContext
{
    public ProjectDBContext(DbContextOptions<ProjectDBContext> options) : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<ProjectAssignment> ProjectAssignments => Set<ProjectAssignment>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>()
            .OwnsOne(e => e.Office);

        modelBuilder.Entity<Project>()
            .HasOne(p => p.Customer)
            .WithMany(c => c.Projects)
            .HasForeignKey(p => p.CustomerId);

        modelBuilder.Entity<Project>()
            .Property(p => p.Status)
            .HasConversion<string>();

        modelBuilder.Entity<TaskItem>()
            .Property(t => t.Priority)
            .HasConversion<string>();

        modelBuilder.Entity<TaskItem>()
            .Property(t => t.State)
            .HasConversion<string>();

        modelBuilder.Entity<TaskItem>()
            .HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId);

        modelBuilder.Entity<TaskItem>()
            .HasOne(t => t.Assignee)
            .WithMany(e => e.AssignedTasks)
            .HasForeignKey(t => t.AssigneeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<TaskItem>()
            .HasMany(t => t.RequiredSkills)
            .WithMany(s => s.Tasks);

        modelBuilder.Entity<Employee>()
            .HasMany(e => e.Skills)
            .WithMany(s => s.Employees);

        modelBuilder.Entity<ProjectAssignment>()
            .HasKey(pa => new { pa.ProjectId, pa.EmployeeId });

        modelBuilder.Entity<ProjectAssignment>()
            .HasOne(pa => pa.Project)
            .WithMany(p => p.Assignments)
            .HasForeignKey(pa => pa.ProjectId);

        modelBuilder.Entity<ProjectAssignment>()
            .HasOne(pa => pa.Employee)
            .WithMany(e => e.ProjectAssignments)
            .HasForeignKey(pa => pa.EmployeeId);

        modelBuilder.Entity<TimeEntry>()
            .HasOne(te => te.TaskItem)
            .WithMany(t => t.TimeEntries)
            .HasForeignKey(te => te.TaskItemId);

        modelBuilder.Entity<TimeEntry>()
            .HasOne(te => te.Employee)
            .WithMany(e => e.TimeEntries)
            .HasForeignKey(te => te.EmployeeId);

        modelBuilder.Entity<Project>()
            .Property(p => p.Budget)
            .HasPrecision(12, 2);

        modelBuilder.Entity<Employee>()
            .Property(e => e.HourlyRate)
            .HasPrecision(10, 2);

        modelBuilder.Entity<TimeEntry>()
            .Property(te => te.Hours)
            .HasPrecision(5, 2);

        modelBuilder.Entity<Customer>()
            .HasIndex(c => c.Name);

        modelBuilder.Entity<Project>()
            .HasIndex(p => p.Name);

        modelBuilder.Entity<TaskItem>()
            .HasIndex(t => t.DueDate);
    }
}