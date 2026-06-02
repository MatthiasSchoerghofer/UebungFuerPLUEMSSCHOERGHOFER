using System.Collections.Generic;

namespace Aufgabe3_RestfulApi.Model;

public class Room
{
    protected Room()
    {
    }

    public Room(string roomNumber, string name, int floor)
    {
        RoomNumber = roomNumber;
        Name = name;
        Floor = floor;
    }

    public int Id { get; private set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Floor { get; set; }

    public List<Device> Devices { get; } = new();
}
