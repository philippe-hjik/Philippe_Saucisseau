using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WinFormsSaucisseau.Classes.Interfaces;

namespace WinFormsSaucisseau.Classes.Enveloppes
{
    public class EnvoieFichier
    {
        /* 
            type 4 ENVOIE_FICHIER
         */

        public string Content { get; set; }
        public MediaData FileInfo { get; set; }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }

    }
}
