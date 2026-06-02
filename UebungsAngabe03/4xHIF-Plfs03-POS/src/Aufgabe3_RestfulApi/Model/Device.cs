using System;
using System.Collections.Generic;

namespace Aufgabe3_RestfulApi.Model;

public class Device
{
    protected Device()
    {
    }

    public Device(
        string inventoryNumber,
        string name,
        DeviceType deviceType,
        decimal replacementValue,
        DateTime purchasedOn,
        Room room)
    {
        InventoryNumber = inventoryNumber;
        Name = name;
        DeviceType = deviceType;
        ReplacementValue = replacementValue;
        PurchasedOn = purchasedOn;
        Room = room;
    }

    public int Id { get; private set; }
    public string InventoryNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DeviceType DeviceType { get; set; }
    public DeviceStatus Status { get; set; } = DeviceStatus.Available;
    public decimal ReplacementValue { get; set; }
    public DateTime PurchasedOn { get; set; }
    public string? Note { get; set; }

    public Room Room { get; set; } = null!;
    public List<EquipmentTag> Tags { get; } = new();
    public List<Reservation> Reservations { get; } = new();
    public List<MaintenanceTicket> MaintenanceTickets { get; } = new();
}
