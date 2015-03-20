using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMS
{
    public class BMSChannel
    {
        public string Type { get; set; }
        public string Action { get; set; }
        public List<Beat> Beats { get; set; }

        public bool IsSystemBGM
        {
            get
            {
                return Type == "0" && Action == "1";
            }
        }

        public bool IsSystemBeat
        {
            get
            {
                return Type == "0" && Action == "2";
            }
        }
    }
}
