using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace BMS
{
    class IndexedTable
    {
        public const string INDEXNAME_WAV = "WAV";
        public const string INDEXNAME_BMP = "BMP";
        public const string INDEXNAME_BPM = "BPM";

        const int indexedNameLength = 3;

        static HashSet<string> IndexedNames;

        static IndexedTable()
        {
            IndexedNames = new HashSet<string>();
            IndexedNames.Add(INDEXNAME_WAV);
            IndexedNames.Add(INDEXNAME_BMP);
            IndexedNames.Add(INDEXNAME_BPM);
        }

        public static bool IsIndexed(string cmd)
        {
            foreach (var name in IndexedNames)
            {
                if (cmd.StartsWith(name) && cmd.Length > name.Length)
                    return true;
            }

            return false;
        }

        Dictionary<string, Dictionary<string, string>> table;
        public IndexedTable()
        {
            table = new Dictionary<string, Dictionary<string, string>>();
        }

        public void Add(string cmd, string value)
        {
            var key = cmd.Substring(0, indexedNameLength);
            var index = cmd.Substring(indexedNameLength);

            Add(key, index, value);
        }

        private void Add(string key, string index, string value)
        {
            if (!table.ContainsKey(key))
            {
                table[key] = new Dictionary<string, string>();
            }

            table[key][index] = value;
        }

        public Dictionary<string, string> this[string key]
        {
            get
            {
                Dictionary<string, string> value;
                if (!table.TryGetValue(key, out value))
                    return null;

                return value;
            }
        }
    }
}
