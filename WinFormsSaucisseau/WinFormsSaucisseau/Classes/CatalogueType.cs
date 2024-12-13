using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsSaucisseau.Classes
{
    public enum MessageType
    {
        ENVOIE_CATALOGUE,
        ENVOIE_FICHIER,
        DEMANDE_CATALOGUE
    }

    public class GenericEnvelope
    {
        public MessageType MessageType { get; set; }
        public string SenderId { get; set; }
        public string EnveloppeJson { get; set; }
    }
}
