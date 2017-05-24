using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thermotraq.Model
{

    public class chips
    {
        public string uid { get; set; }
        public int uses { get; set; }
        public int sessions_remaining { get; set; }

        public string name { get; set; }

        public int company_id { get; set; }
        public bool active { get; set; }


    }
}
