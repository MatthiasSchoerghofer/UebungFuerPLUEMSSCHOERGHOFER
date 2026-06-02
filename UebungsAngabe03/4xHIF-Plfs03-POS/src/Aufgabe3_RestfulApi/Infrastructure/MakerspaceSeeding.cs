using System;
using Aufgabe3_RestfulApi.Model;

namespace Aufgabe3_RestfulApi.Infrastructure;

public static class MakerspaceSeeding
{
    public static int SeedMakerspace(this MakerspaceContext db)
    {
        var roomLab = new Room("3.14", "Hardware Lab", 3);
        var roomStudio = new Room("2.08", "Media Studio", 2);
        var roomStorage = new Room("E.02", "Ausgabe", 0);

        var tagPortable = new EquipmentTag("portable", "mobil");
        var tagExpensive = new EquipmentTag("expensive", "hochwertig");
        var tagBeginner = new EquipmentTag("beginner", "einsteigerfreundlich");
        var tagMeasurement = new EquipmentTag("measurement", "messen");

        var anna = new Member(
            "Anna",
            "Lechner",
            "anna.lechner@example.school",
            new MakerspaceAddress("Schulgasse 4", "Wien", "1010", "AT"));
        var ben = new Member(
            "Ben",
            "Kaya",
            "ben.kaya@example.school",
            new MakerspaceAddress("Bahnhofstrasse 8", "Wien", "1020", "AT"));
        var clara = new Member(
            "Clara",
            "Novak",
            "clara.novak@example.school",
            new MakerspaceAddress("Hauptplatz 1", "St. Poelten", "3100", "AT"))
        {
            IsLocked = true
        };

        var camera = new Device("CAM-2026-001", "Sony ZV-E10", DeviceType.Camera, 799.90m, new DateTime(2025, 10, 12), roomStudio)
        {
            Status = DeviceStatus.Available,
            Tags = { tagPortable, tagExpensive }
        };
        var notebook = new Device("NB-2026-003", "Framework Laptop 13", DeviceType.Notebook, 1299m, new DateTime(2026, 1, 20), roomStorage)
        {
            Status = DeviceStatus.Borrowed,
            Tags = { tagPortable, tagExpensive }
        };
        var printer = new Device("PRN-2025-002", "Bambu Lab A1 mini", DeviceType.Printer, 329m, new DateTime(2025, 5, 2), roomLab)
        {
            Status = DeviceStatus.Maintenance,
            Tags = { tagBeginner }
        };
        var sensorKit = new Device("SEN-2024-009", "Sensor Kit Umwelt", DeviceType.Sensor, 89.50m, new DateTime(2024, 9, 14), roomLab)
        {
            Status = DeviceStatus.Available,
            Tags = { tagMeasurement, tagBeginner }
        };

        var activeReservation = new Reservation(
            notebook,
            anna,
            new DateTime(2026, 6, 1, 8, 0, 0),
            new DateTime(2026, 6, 7, 16, 0, 0),
            "Diplomprojekt Praesentation")
        {
            Status = ReservationStatus.Active
        };

        var plannedReservation = new Reservation(
            camera,
            ben,
            new DateTime(2026, 6, 12, 10, 0, 0),
            new DateTime(2026, 6, 12, 14, 0, 0),
            "Interview aufnehmen");

        var ticket = new MaintenanceTicket(
            printer,
            TicketSeverity.High,
            "Druckbett muss neu kalibriert werden.",
            new DateTime(2026, 5, 29, 11, 30, 0));

        db.AddRange(roomLab, roomStudio, roomStorage);
        db.AddRange(tagPortable, tagExpensive, tagBeginner, tagMeasurement);
        db.AddRange(anna, ben, clara);
        db.AddRange(camera, notebook, printer, sensorKit);
        db.AddRange(activeReservation, plannedReservation, ticket);

        return db.SaveChanges();
    }
}
