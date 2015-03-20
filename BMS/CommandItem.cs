using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMS
{
    class CommandItem : SCItem
    {
        public static string ToCmd(string key)
        {
            return key.ToUpper();
        }


        public CommandItem(string cmd) : base(cmd)
        {
            var slice = cmd.Split(CommandDelimiter);
            Key = slice[0];

            Value = String.Empty;
            if (slice.Length <= 1)
                return;

            var first = cmd.IndexOf(CommandDelimiter);
            Value = Normalize(cmd.Substring(first + 1));
        }

        public string Key { get; set; }
        public string Value { get; set; }

        public string Cmd
        {
            get
            {
                return ToCmd(Key);
            }
        }
    }
}
