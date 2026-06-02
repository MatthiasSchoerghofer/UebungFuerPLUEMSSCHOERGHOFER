using System;

namespace Aufgabe3_RestfulApi.Model;

public class MaintenanceTicket
{
    protected MaintenanceTicket()
    {
    }

    public MaintenanceTicket(Device device, TicketSeverity severity, string description, DateTime createdAt)
    {
        Device = device;
        Severity = severity;
        Description = description;
        CreatedAt = createdAt;
    }

    public int Id { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public TicketSeverity Severity { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsOpen => ClosedAt is null;

    public Device Device { get; set; } = null!;
}
