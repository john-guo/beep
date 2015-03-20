using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMS
{
    using exeCmds = ExecutableCommands;

    class CommandTable
    {
        static Dictionary<string, exeCmds.ExecutableCommand> executableCommands = new Dictionary<string, exeCmds.ExecutableCommand>();

        static void AddExecutableCommand(exeCmds.ExecutableCommand exeCmd)
        {
            executableCommands.Add(CommandItem.ToCmd(exeCmd.Name), exeCmd);
        }

        static CommandTable()
        {
            AddExecutableCommand(new exeCmds.CmdRandom());
            AddExecutableCommand(new exeCmds.CmdIf());
            AddExecutableCommand(new exeCmds.CmdEndif());
        }

        IndexedTable resources;
        Dictionary<string, CommandItem> _innerTable;

        public CommandTable()
        {
            resources = new IndexedTable();
            _innerTable = new Dictionary<string, CommandItem>();
        }

        public string this[string key] 
        {
            get 
            {
                var item = _innerTable[CommandItem.ToCmd(key)];
                return item.Value;
            }
        }

        public void Add(CommandItem item)
        {
            if (IndexedTable.IsIndexed(item.Cmd))
            {
                resources.Add(item.Cmd, item.Value);
                return;
            }

            _innerTable[item.Cmd] = item;
        }

        public CommandItem Get(string key)
        {
            CommandItem value;
            if (!_innerTable.TryGetValue(CommandItem.ToCmd(key), out value))
                return null;

            return value;
        }

        public bool Execute(CommandExecutionContext context, CommandItem item)
        {
            exeCmds.ExecutableCommand exeCmd;
            if (!executableCommands.TryGetValue(item.Cmd, out exeCmd))
                return false;

            exeCmd.Run(context, item.Value);
            return true;
        }

        public Dictionary<string, string> MusicResources
        {
            get
            {
                return resources[IndexedTable.INDEXNAME_WAV];
            }
        }

        public Dictionary<string, string> ImageResources
        {
            get
            {
                return resources[IndexedTable.INDEXNAME_BMP];
            }
        }

        public Dictionary<string, string> BPMResources
        {
            get
            {
                return resources[IndexedTable.INDEXNAME_BPM];
            }
        }

    }
}
