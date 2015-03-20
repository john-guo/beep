using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMS
{
    class ExecutableCommands
    {
        public abstract class ExecutableCommand
        {
            public abstract string Name { get; }
            public abstract void Run(CommandExecutionContext context, string parameters);
        }

        public class CmdRandom : ExecutableCommand
        {
            Random rnd;

            public CmdRandom()
            {
                rnd = new Random();
            }

            public override string Name
            {
                get { return "random"; }
            }

            public override void Run(CommandExecutionContext context, string parameters)
            {
                var num = rnd.Next(1, Int32.Parse(parameters) + 1).ToString();
                context.Set(EXEIDS.EXEID_RANDOM, num);
            }

        }

        public class CmdIf : ExecutableCommand
        {
            public override string Name
            {
                get { return "if"; }
            }

            public override void Run(CommandExecutionContext context, string parameters)
            {
                var num = context.Get(EXEIDS.EXEID_RANDOM);
                if (num == parameters)
                    context.Unblock();
                else
                    context.Block();
            }
        }

        public class CmdEndif: ExecutableCommand
        {
            public override string Name
            {
                get { return "endif"; }
            }

            public override void Run(CommandExecutionContext context, string parameters)
            {
                context.Unblock();
            }
        }

    }
}
