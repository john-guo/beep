using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMS
{
    public class ActionItem
    {
        public Dictionary<string, string> System { get; private set; } 
        public Dictionary<string, string> P1 { get; private set; }
        public Dictionary<string, string> P2 { get; private set; }

        public Dictionary<string, string> LP1 { get; private set; }
        public Dictionary<string, string> LP2 { get; private set; }


        /// <summary>
        /// Beat number start with 0.
        /// </summary>
        public int Number { get; private set; }

        public string Track { get; private set; }
        public List<string> BackgroundMusic { get; private set; }

        public ActionItem(string track, int number = 0)
        {
            System = new Dictionary<string, string>();
            P1 = new Dictionary<string, string>();
            P2 = new Dictionary<string, string>();
            LP1 = new Dictionary<string, string>();
            LP2 = new Dictionary<string, string>();
            BackgroundMusic = new List<string>();

            Track = track;
            Number = number;
        }
        
        public Dictionary<string, string> GetItemFromType(string type)
        {
            switch (type)
            {
                case "1":
                    return P1;
                case "2":
                    return P2;
                case "5":
                    return LP1;
                case "6":
                    return LP2;
                case "0":
                default:
                    return System;
            }
        }

        public ResourceType GetResourceType(string sysAction)
        {
            switch (sysAction)
            {
                case "1":
                    return ResourceType.Music;
                case "4":
                case "5":
                case "6":
                    return ResourceType.Image;
                case "8":
                    return ResourceType.BPM;
                default:
                    return ResourceType.None;
            }
        }
    }
}
