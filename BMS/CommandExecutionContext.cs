using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMS
{
    sealed class EXEIDS
    {
        public const string EXEID_BLOCK = "__BLOCK";
        public const string EXEID_RANDOM = "__RANDOM";
    }

    class CommandExecutionContext
    {
        const string EXEV_NONE = "0";
        const string EXEV_ONE = "1";


        Dictionary<string, string> contextInfo;

        public CommandExecutionContext()
        {
            contextInfo = new Dictionary<string, string>();
        }

        public void Set(string name, string value)
        {
            contextInfo[name] = value;
        }

        public string Get(string name)
        {
            string value;

            if (!contextInfo.TryGetValue(name, out value))
                return EXEV_NONE;

            return value;
        }

        public bool IsBlocking
        {

            get 
            {
                var v = Get(EXEIDS.EXEID_BLOCK);
                if (v == EXEV_ONE)
                    return true;

                return false;
            }
        }

        public void Block()
        {
            Set(EXEIDS.EXEID_BLOCK, EXEV_ONE);
        }

        public void Unblock()
        {
            Set(EXEIDS.EXEID_BLOCK, EXEV_NONE);
        }
    }
}
