namespace Aufgabe3_RestfulApi.Model;

public class Skill
{
    public Skill(string code, string name, string category)
    {
        Code = code;
        Name = name;
        Category = category;
    }

    public int Id { get; private set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }

    public List<TaskItem> Tasks { get; } = new();
    public List<Employee> Employees { get; } = new();
}