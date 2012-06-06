using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MxitTestApp
{
    public class Translation
    {
        public int translation_id { get; private set; }
        public string name { get; private set; }
        public string full_name { get; private set; }
        public Translation(int translation_id, string full_name, string name)
        {
            this.translation_id = translation_id;
            this.name = name;
            this.full_name = full_name;
        }
    }
}
