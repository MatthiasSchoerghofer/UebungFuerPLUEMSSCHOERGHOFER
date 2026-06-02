using System.Collections.Generic;

namespace Aufgabe3_RestfulApi.Model;

public class Member
{
    protected Member()
    {
    }

    public Member(string firstName, string lastName, string email, MakerspaceAddress address)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Address = address;
    }

    public int Id { get; private set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsLocked { get; set; }
    public MakerspaceAddress Address { get; set; } = null!;

    public List<Reservation> Reservations { get; } = new();
}
