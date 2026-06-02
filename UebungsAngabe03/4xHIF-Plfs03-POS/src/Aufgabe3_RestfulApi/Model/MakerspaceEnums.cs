namespace Aufgabe3_RestfulApi.Model;

public enum DeviceType
{
    Notebook,
    Camera,
    Microcontroller,
    Tool,
    Printer,
    Sensor,
    Other
}

public enum DeviceStatus
{
    Available,
    Reserved,
    Borrowed,
    Maintenance,
    Retired
}

public enum ReservationStatus
{
    Planned,
    Active,
    Completed,
    Cancelled,
    Overdue
}

public enum TicketSeverity
{
    Low,
    Medium,
    High,
    Critical
}
