namespace Aufgabe3_RestfulApi.Model;

public class Customer
{
    public Customer(string name, string industry, string country)
    {
        Name = name;
        Industry = industry;
        Country = country;
    }

    public int Id { get; private set; }
    public string Name { get; set; }
    public string Industry { get; set; }
    public string Country { get; set; }

    public List<Project> Projects { get; } = new();
}