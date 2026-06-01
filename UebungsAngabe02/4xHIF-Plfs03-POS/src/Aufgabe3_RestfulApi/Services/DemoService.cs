
namespace Aufgabe3_RestfulApi.Services;
// Todo: only for demonstration, should be replaced/removed!
public interface IDemoService {
    List<string> GetAll();

}

// Todo: only for demonstration, should be replaced/removed!
public class DemoService : IDemoService {
    public List<string> GetAll()
    {
        return ["Response", "from", "demo", "Aufgabe 3!"];
    }
}