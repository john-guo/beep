using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMS
{
    abstract class SCItem
    {
        const int channelDelPos = 5;
        protected const char CommandDelimiter = ' ';
        protected const char ChannelDelimiter = ':';
        protected string Normalize(string cmd)
        {
            return cmd.TakeUntil(@"//").Trim();
        }

        protected SCItem(string cmd)
        {
            Raw = cmd;
        }

        public string Raw { get; set; }

        public static bool IsChannel(string cmd)
        {
            if (cmd.Length <= channelDelPos)
                return false;

            return cmd[channelDelPos] == ChannelDelimiter;
        }
    }
}
