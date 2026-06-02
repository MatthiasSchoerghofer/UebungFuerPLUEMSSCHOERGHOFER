using System.Collections.Generic;

namespace Aufgabe3_RestfulApi.Model;

public class EquipmentTag
{
    protected EquipmentTag()
    {
    }

    public EquipmentTag(string id, string text)
    {
        Id = id;
        Text = text;
    }

    public string Id { get; private set; } = string.Empty;
    public string Text { get; set; } = string.Empty;

    public List<Device> Devices { get; } = new();
}
