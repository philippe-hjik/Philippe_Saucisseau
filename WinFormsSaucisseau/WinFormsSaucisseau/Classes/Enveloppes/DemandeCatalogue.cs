using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinFormsSaucisseau.Classes.Interfaces;
using System.Text.Json;

namespace WinFormsSaucisseau.Classes.Enveloppes
{
    public class DemandeCatalogue : IJsonSerializableMessage
    {
        /* 
            type 2 DEMANDE_CATALOGUE
         */
        public string Content { get; set; }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true});
        }
    }
}
