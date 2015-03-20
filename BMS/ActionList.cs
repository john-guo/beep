using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMS
{
    public enum ResourceType
    {
        None,
        Music,
        Image,
        BPM
    }


    public class ActionList
    {
        public float TrackBPM
        {
            get; set; 
        }

        public float BPM 
        { 
            get
            {
                if (TrackBPM > 0)
                    return TrackBPM;

                return Information.BPM;
            }
        }
        public List<ActionItem> Items { get; set; }

        public float BPS
        {
            get
            {
                return BPM / 60.0f;
            }
        }

        public float SPB
        {
            get
            {
                return 1.0f / BPS;
            }
        }

        public int Total 
        {
            get 
            {
                return Items.Count;
            }
        }

        public float TotalInSeconds
        {
            get
            {
                return (float)Math.Ceiling(Total * SPB);
            }
        }

        private int first = -1;
        public int First
        {
            get
            {
                if (first >= 0)
                    return first;

                first = Items.First(item =>
                {
                    foreach (var b in item.P1)
                    {
                        if (!String.IsNullOrWhiteSpace(b.Value) && b.Value != "00")
                            return true;
                    }
                    foreach (var b in item.P2)
                    {
                        if (!String.IsNullOrWhiteSpace(b.Value) && b.Value != "00")
                            return true;
                    }
                    return false;
                }).Number;

                return first;
            }
        }

        public float FirstInSeconds
        {
            get
            {
                return (float)Math.Ceiling(First * SPB); 
            }
        }

        public ActionList()
        {
            TrackBPM = -1;
        }

        public BMSInfo Information
        {
            get;
            set;
        }

        public Dictionary<string, string> Musics
        {
            get; set;
        }

        public Dictionary<string, string> Images
        {
            get; set;
        }

        public Dictionary<string, string> BPMs
        {
            get; set;
        }

    }


}
