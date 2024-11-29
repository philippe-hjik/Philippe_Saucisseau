using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsSaucisseau.Classes.Enveloppes
{
    public class EnveloppeDemandeCatalogue
    {
        /* 
            type 2 DEMANDE_CATALOGUE
         */
        private int _type;
        private string _guid;
        private string _content;

        public string Guid { get => _guid; set => _guid = value; }
        public string Content { get => _content; set => _content = value; }
        public int Type { get => _type; set => _type = value; }
    }
}
