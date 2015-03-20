using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMS
{
    public class BMSInfo
    {
        CommandTable _table;
        internal BMSInfo(CommandTable table)
        {
            _table = table;
        }

        private T GetValueOrDefault<T>(string key)
        {
            var t = _table.Get(key);
            if (t == null) 
            {
                return default(T);
            }

            var value = t.Value;
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return default(T);
            }
        }

        public int Player 
        {
            get { return GetValueOrDefault<int>("PLAYER"); }
        }
        public string Genre
        {
            get { return GetValueOrDefault<string>("GENRE"); }
        }
        public string Title
        {
            get { return GetValueOrDefault<string>("TITLE"); }
        }
        public string Artist
        {
            get { return GetValueOrDefault<string>("ARTIST"); }
        }
        public int BPM
        {
            get { return GetValueOrDefault<int>("BPM"); }
        }
        public string Midi
        {
            get { return GetValueOrDefault<string>("MIDIFILE"); }
        }
        public int Level
        {
            get { return GetValueOrDefault<int>("PLAYLEVEL"); }
        }
        public int Rank
        {
            get { return GetValueOrDefault<int>("RANK"); }
        }
        public int Volume
        {
            get { return GetValueOrDefault<int>("VOLWAV"); }
        }

        public string Stage 
        {
            get { return GetValueOrDefault<string>("STAGEFILE"); }

        }

        public int Total
        {
            get { return GetValueOrDefault<int>("TOTAL"); }
        }

        public int LNType
        {
            get { return GetValueOrDefault<int>("LNTYPE"); }
        }
    }
}
