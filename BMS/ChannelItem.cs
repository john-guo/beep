using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMS
{
    class ChannelItem : SCItem
    {
        public ChannelItem(string cmd) : base(cmd)
        {
            var slice = cmd.Split(ChannelDelimiter);

            Track = slice[0].Substring(0, 3);
            Int32.Parse(Track);

            Type = slice[0].Substring(3, 1);
            Command = slice[0].Substring(4, 1);
            Message = Normalize(slice[1]);
        }

        public string Track { get; set; }
        public string Type { get; set; }
        public string Command { get; set; }
        public string Message { get; set; }
    }
}
