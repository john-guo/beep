using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMS
{
    class ChannelTable
    {
        Dictionary<string, List<ChannelItem>> _innerTable = new Dictionary<string, List<ChannelItem>>();

        public List<ChannelItem> this[string track]
        {
            get 
            {
                return _innerTable[track];
            }
        }

        public Dictionary<string, List<ChannelItem>> Table
        {
            get
            {
                return _innerTable;
            }
        }

        public void Add(ChannelItem item)
        {
            if (!_innerTable.ContainsKey(item.Track))
            {
                _innerTable[item.Track] = new List<ChannelItem>();
            }

            _innerTable[item.Track].Add(item);
        }
    }
}
