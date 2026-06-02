using System;

namespace Aufgabe3_RestfulApi.Model;

public class Reservation
{
    protected Reservation()
    {
    }

    public Reservation(Device device, Member member, DateTime reservedFrom, DateTime reservedUntil, string purpose)
    {
        Device = device;
        Member = member;
        ReservedFrom = reservedFrom;
        ReservedUntil = reservedUntil;
        Purpose = purpose;
    }

    public int Id { get; private set; }
    public DateTime ReservedFrom { get; set; }
    public DateTime ReservedUntil { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Planned;
    public string Purpose { get; set; } = string.Empty;
    public string? ReturnNote { get; set; }

    public Device Device { get; set; } = null!;
    public Member Member { get; set; } = null!;
}
