using Aufgabe3_RestfulApi.Model;
using Microsoft.EntityFrameworkCore;

namespace Aufgabe3_RestfulApi.Infrastructure;

public static class Seeding
{
    public static async Task SeedAsync(this ProjectDBContext db)
    {
        if (await db.Projects.AnyAsync())
        {
            return;
        }

        var csharp = new Skill("CS", "CSharp", "Backend");
        var efCore = new Skill("EF", "Entity Framework Core", "Backend");
        var aspNet = new Skill("API", "ASP.NET Core", "Backend");
        var sql = new Skill("SQL", "SQL", "Database");
        var angular = new Skill("ANG", "Angular", "Frontend");
        var react = new Skill("RCT", "React", "Frontend");
        var ux = new Skill("UX", "UX Design", "Design");
        var testing = new Skill("TEST", "Testing", "Quality");
        var devOps = new Skill("DEVOPS", "DevOps", "Infrastructure");
        var docker = new Skill("DOCKER", "Docker", "Infrastructure");

        var anna = new Employee("Anna Berger", "Development", "Backend Developer", 78)
        {
            Office = new OfficeAddress("Hauptplatz 1", "Wien", "AT"),
            Skills = { csharp, efCore, aspNet, sql, testing }
        };

        var max = new Employee("Max Steiner", "Development", "Fullstack Developer", 72)
        {
            Office = new OfficeAddress("Bahnhofstrasse 4", "Graz", "AT"),
            Skills = { csharp, aspNet, angular, react, sql }
        };

        var sara = new Employee("Sara Novak", "Design", "UX Designer", 65)
        {
            Office = new OfficeAddress("Ring 12", "Wien", "AT"),
            Skills = { ux, testing }
        };

        var lukas = new Employee("Lukas Hofer", "Operations", "DevOps Engineer", 82)
        {
            Office = new OfficeAddress("Industriestrasse 8", "Linz", "AT"),
            Skills = { devOps, docker, sql, testing }
        };

        var emma = new Employee("Emma Klein", "Development", "Junior Developer", 45)
        {
            Office = new OfficeAddress("Main Street 10", "Berlin", "DE"),
            Skills = { csharp, react, testing }
        };

        var noah = new Employee("Noah Fischer", "Development", "Backend Developer", 70)
        {
            Office = new OfficeAddress("Tech Park 3", "Muenchen", "DE"),
            Skills = { csharp, efCore, aspNet, docker }
        };

        var customers = new List<Customer>
        {
            new("Alpine Bank", "Finance", "AT"),
            new("MediCare Systems", "Healthcare", "AT"),
            new("ShopSphere GmbH", "E-Commerce", "DE"),
            new("EduCloud AG", "Education", "CH"),
            new("LogiTrans", "Logistics", "AT")
        };

        var bankingApp = new Project(
            "Mobile Banking Relaunch",
            new DateTime(2026, 1, 15),
            new DateTime(2026, 9, 30),
            120000)
        {
            Customer = customers[0],
            Status = ProjectStatus.Active
        };

        var clinicPortal = new Project(
            "Clinic Appointment Portal",
            new DateTime(2026, 2, 1),
            new DateTime(2026, 8, 15),
            85000)
        {
            Customer = customers[1],
            Status = ProjectStatus.Active
        };

        var shopMigration = new Project(
            "Shop Migration Platform",
            new DateTime(2025, 11, 1),
            new DateTime(2026, 6, 30),
            64000)
        {
            Customer = customers[2],
            Status = ProjectStatus.Paused
        };

        var learningDashboard = new Project(
            "Learning Analytics Dashboard",
            new DateTime(2026, 3, 1),
            new DateTime(2026, 12, 20),
            97000)
        {
            Customer = customers[3],
            Status = ProjectStatus.Planned
        };

        var logisticsApi = new Project(
            "Logistics Tracking API",
            new DateTime(2025, 9, 10),
            new DateTime(2026, 4, 30),
            54000)
        {
            Customer = customers[4],
            Status = ProjectStatus.Done
        };

        var tasks = new List<TaskItem>
        {
            new("Implement login API", "JWT login endpoint with refresh token support", new DateTime(2026, 3, 15), TaskPriority.High)
            {
                Project = bankingApp,
                Assignee = anna,
                State = TaskState.Done,
                EstimatedHours = 18,
                RequiredSkills = { csharp, aspNet, testing }
            },
            new("Build transaction overview", "Return filtered account transactions for mobile clients", new DateTime(2026, 5, 10), TaskPriority.Critical)
            {
                Project = bankingApp,
                Assignee = max,
                State = TaskState.InProgress,
                EstimatedHours = 32,
                RequiredSkills = { csharp, aspNet, sql }
            },
            new("Security review", "Review API endpoints and permission checks", new DateTime(2026, 5, 20), TaskPriority.Critical)
            {
                Project = bankingApp,
                Assignee = lukas,
                State = TaskState.Blocked,
                EstimatedHours = 20,
                RequiredSkills = { testing, devOps, aspNet }
            },

            new("Appointment search", "Search appointments by doctor, date and status", new DateTime(2026, 4, 5), TaskPriority.High)
            {
                Project = clinicPortal,
                Assignee = noah,
                State = TaskState.Review,
                EstimatedHours = 24,
                RequiredSkills = { csharp, efCore, aspNet }
            },
            new("Patient UI prototype", "Clickable prototype for booking workflow", new DateTime(2026, 3, 25), TaskPriority.Medium)
            {
                Project = clinicPortal,
                Assignee = sara,
                State = TaskState.Done,
                EstimatedHours = 16,
                RequiredSkills = { ux, testing }
            },
            new("Database optimization", "Improve query performance for appointment history", new DateTime(2026, 6, 10), TaskPriority.High)
            {
                Project = clinicPortal,
                Assignee = anna,
                State = TaskState.Open,
                EstimatedHours = 28,
                RequiredSkills = { efCore, sql }
            },

            new("Import product catalog", "Import old catalog data into new schema", new DateTime(2026, 2, 28), TaskPriority.Critical)
            {
                Project = shopMigration,
                Assignee = emma,
                State = TaskState.Blocked,
                EstimatedHours = 34,
                RequiredSkills = { csharp, sql, efCore }
            },
            new("Frontend checkout migration", "Rebuild checkout flow in SPA", new DateTime(2026, 4, 18), TaskPriority.High)
            {
                Project = shopMigration,
                Assignee = max,
                State = TaskState.InProgress,
                EstimatedHours = 40,
                RequiredSkills = { angular, testing }
            },
            new("Container setup", "Docker setup for local development and staging", new DateTime(2026, 5, 2), TaskPriority.Medium)
            {
                Project = shopMigration,
                Assignee = lukas,
                State = TaskState.Open,
                EstimatedHours = 18,
                RequiredSkills = { docker, devOps }
            },

            new("Analytics API design", "Define endpoints for course and student analytics", new DateTime(2026, 7, 1), TaskPriority.High)
            {
                Project = learningDashboard,
                Assignee = noah,
                State = TaskState.Open,
                EstimatedHours = 30,
                RequiredSkills = { csharp, aspNet, efCore }
            },
            new("Dashboard charts", "Implement frontend chart views", new DateTime(2026, 8, 10), TaskPriority.Medium)
            {
                Project = learningDashboard,
                Assignee = null,
                State = TaskState.Open,
                EstimatedHours = 26,
                RequiredSkills = { react, ux }
            },

            new("Tracking endpoint", "Endpoint for live shipment tracking", new DateTime(2025, 12, 1), TaskPriority.High)
            {
                Project = logisticsApi,
                Assignee = anna,
                State = TaskState.Done,
                EstimatedHours = 22,
                RequiredSkills = { csharp, aspNet, sql }
            },
            new("Deployment pipeline", "Automated deployment for tracking API", new DateTime(2026, 1, 10), TaskPriority.Medium)
            {
                Project = logisticsApi,
                Assignee = lukas,
                State = TaskState.Done,
                EstimatedHours = 14,
                RequiredSkills = { devOps, docker, testing }
            }
        };

        var assignments = new List<ProjectAssignment>
        {
            new()
            {
                Project = bankingApp,
                Employee = anna,
                RoleInProject = "Backend Lead",
                AssignedFrom = new DateTime(2026, 1, 15),
                AllocationPercent = 70
            },
            new()
            {
                Project = bankingApp,
                Employee = max,
                RoleInProject = "Fullstack Developer",
                AssignedFrom = new DateTime(2026, 2, 1),
                AllocationPercent = 60
            },
            new()
            {
                Project = bankingApp,
                Employee = lukas,
                RoleInProject = "Security / DevOps",
                AssignedFrom = new DateTime(2026, 2, 10),
                AllocationPercent = 40
            },

            new()
            {
                Project = clinicPortal,
                Employee = noah,
                RoleInProject = "Backend Developer",
                AssignedFrom = new DateTime(2026, 2, 1),
                AllocationPercent = 80
            },
            new()
            {
                Project = clinicPortal,
                Employee = sara,
                RoleInProject = "UX Designer",
                AssignedFrom = new DateTime(2026, 2, 15),
                AllocationPercent = 50
            },
            new()
            {
                Project = clinicPortal,
                Employee = anna,
                RoleInProject = "Database Consultant",
                AssignedFrom = new DateTime(2026, 4, 1),
                AllocationPercent = 30
            },

            new()
            {
                Project = shopMigration,
                Employee = emma,
                RoleInProject = "Junior Developer",
                AssignedFrom = new DateTime(2025, 11, 10),
                AllocationPercent = 80
            },
            new()
            {
                Project = shopMigration,
                Employee = max,
                RoleInProject = "Frontend Lead",
                AssignedFrom = new DateTime(2025, 11, 1),
                AllocationPercent = 70
            },
            new()
            {
                Project = shopMigration,
                Employee = lukas,
                RoleInProject = "DevOps Engineer",
                AssignedFrom = new DateTime(2026, 1, 1),
                AllocationPercent = 30
            },

            new()
            {
                Project = learningDashboard,
                Employee = noah,
                RoleInProject = "API Architect",
                AssignedFrom = new DateTime(2026, 3, 1),
                AllocationPercent = 50
            },
            new()
            {
                Project = learningDashboard,
                Employee = sara,
                RoleInProject = "UX Research",
                AssignedFrom = new DateTime(2026, 3, 15),
                AllocationPercent = 40
            },

            new()
            {
                Project = logisticsApi,
                Employee = anna,
                RoleInProject = "Backend Developer",
                AssignedFrom = new DateTime(2025, 9, 10),
                AssignedTo = new DateTime(2026, 4, 30),
                AllocationPercent = 60
            },
            new()
            {
                Project = logisticsApi,
                Employee = lukas,
                RoleInProject = "Deployment",
                AssignedFrom = new DateTime(2025, 10, 1),
                AssignedTo = new DateTime(2026, 4, 30),
                AllocationPercent = 40
            }
        };

        var timeEntries = new List<TimeEntry>
        {
            new(new DateTime(2026, 2, 3), 6.5m, "Login endpoint implementation")
            {
                TaskItem = tasks[0],
                Employee = anna,
                Billable = true
            },
            new(new DateTime(2026, 2, 4), 5m, "Login tests")
            {
                TaskItem = tasks[0],
                Employee = anna,
                Billable = true
            },
            new(new DateTime(2026, 4, 10), 7.5m, "Transaction query implementation")
            {
                TaskItem = tasks[1],
                Employee = max,
                Billable = true
            },
            new(new DateTime(2026, 4, 11), 4m, "Transaction DTO mapping")
            {
                TaskItem = tasks[1],
                Employee = max,
                Billable = true
            },
            new(new DateTime(2026, 4, 12), 3.5m, "Security review preparation")
            {
                TaskItem = tasks[2],
                Employee = lukas,
                Billable = false
            },

            new(new DateTime(2026, 3, 2), 8m, "Appointment filtering")
            {
                TaskItem = tasks[3],
                Employee = noah,
                Billable = true
            },
            new(new DateTime(2026, 3, 3), 6m, "Appointment query tests")
            {
                TaskItem = tasks[3],
                Employee = noah,
                Billable = true
            },
            new(new DateTime(2026, 2, 20), 5.5m, "Booking prototype")
            {
                TaskItem = tasks[4],
                Employee = sara,
                Billable = true
            },
            new(new DateTime(2026, 4, 15), 4m, "Database profiling")
            {
                TaskItem = tasks[5],
                Employee = anna,
                Billable = true
            },

            new(new DateTime(2026, 1, 5), 6m, "Catalog import analysis")
            {
                TaskItem = tasks[6],
                Employee = emma,
                Billable = true
            },
            new(new DateTime(2026, 1, 6), 7m, "Import script prototype")
            {
                TaskItem = tasks[6],
                Employee = emma,
                Billable = true
            },
            new(new DateTime(2026, 2, 14), 8m, "Checkout migration")
            {
                TaskItem = tasks[7],
                Employee = max,
                Billable = true
            },
            new(new DateTime(2026, 2, 15), 6.5m, "Checkout validation")
            {
                TaskItem = tasks[7],
                Employee = max,
                Billable = true
            },
            new(new DateTime(2026, 3, 10), 3m, "Docker compose setup")
            {
                TaskItem = tasks[8],
                Employee = lukas,
                Billable = true
            },

            new(new DateTime(2026, 3, 20), 6m, "Analytics endpoint planning")
            {
                TaskItem = tasks[9],
                Employee = noah,
                Billable = true
            },

            new(new DateTime(2025, 10, 1), 7m, "Tracking API")
            {
                TaskItem = tasks[11],
                Employee = anna,
                Billable = true
            },
            new(new DateTime(2025, 10, 2), 5m, "Tracking API tests")
            {
                TaskItem = tasks[11],
                Employee = anna,
                Billable = true
            },
            new(new DateTime(2025, 11, 5), 4m, "Pipeline setup")
            {
                TaskItem = tasks[12],
                Employee = lukas,
                Billable = true
            }
        };

        await db.Skills.AddRangeAsync(
            csharp, efCore, aspNet, sql, angular, react, ux, testing, devOps, docker);

        await db.Employees.AddRangeAsync(
            anna, max, sara, lukas, emma, noah);

        await db.Customers.AddRangeAsync(customers);

        await db.Projects.AddRangeAsync(
            bankingApp, clinicPortal, shopMigration, learningDashboard, logisticsApi);

        await db.TaskItems.AddRangeAsync(tasks);
        await db.ProjectAssignments.AddRangeAsync(assignments);
        await db.TimeEntries.AddRangeAsync(timeEntries);

        await db.SaveChangesAsync();
    }
}