using System.Text.Json;
using WinFormsSaucisseau.Classes.Interfaces;

namespace WinFormsSaucisseau;

public class DemandeFichier : IJsonSerializableMessage
{
    public string FileName { get; set; }


    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }
}