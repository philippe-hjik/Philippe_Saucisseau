using System.Text.Json;
namespace WinFormsSaucisseau.Classes.Interfaces
{
    public interface IJsonSerializableMessage
    {
        public string ToJson();
    }
}
