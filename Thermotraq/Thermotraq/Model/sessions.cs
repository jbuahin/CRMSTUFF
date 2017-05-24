using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thermotraq.Model
{
    public class sessions
    {
        public string uid { get; set; }
        public int min_temp { get; set; }
        public int max_temp { get; set; }
        public int receiver_id { get; set; }
    }
}
