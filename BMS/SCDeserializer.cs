using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMS
{
    class SCDeserializer 
    {
        const string sharp = "#";
        const string star = "*";
        CommandExecutionContext context;

        public string[] Lines { get; private set; }

        public SCDeserializer(string fileName, Encoding encode)
        {
            Lines = File.ReadAllLines(fileName, encode);
            context = new CommandExecutionContext();
        }

        public void Deserializer(out CommandTable cmdTable, out ChannelTable channelTable)
        {
            cmdTable = new CommandTable();
            channelTable = new ChannelTable();

            foreach (var line in Lines)
            {
                var trline = line.TrimStart();
                if (!trline.StartsWith(sharp))
                {
                    if (!trline.StartsWith(star))
                        Debug.WriteLine(line);
                    continue;
                }

                try
                {
                    var cmdRaw = trline.Substring(1);
                    if (SCItem.IsChannel(cmdRaw))
                    {
                        if (context.IsBlocking)
                            continue;
                        channelTable.Add(new ChannelItem(cmdRaw));
                    }
                    else
                    {
                        var cmd = new CommandItem(cmdRaw);

                        if (cmdTable.Execute(context, cmd))
                            continue;

                        if (context.IsBlocking)
                            continue;
                        cmdTable.Add(cmd);
                    }

                }
                catch
                {
                    Debug.WriteLine(line);
                }
            }
        }
    }
}
