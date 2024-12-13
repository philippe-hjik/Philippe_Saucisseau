using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WinFormsSaucisseau.Classes.Interfaces;

namespace WinFormsSaucisseau
{
    public class EnvoieCatalogue : IJsonSerializableMessage
    {
        /* 
            type 1 ENVOIE_CATALOGUE
         */
        public List<MediaData> Content { get; set; }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
