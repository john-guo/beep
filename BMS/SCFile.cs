using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMS
{
    public class SCFile
    {
        SCDeserializer deserializer;
        CommandTable commandT;
        ChannelTable channelT;
        List<BMSTrack> tracks;

        private SCFile(string fileName, Encoding encode)
        {
            deserializer = new SCDeserializer(fileName, encode);
            deserializer.Deserializer(out commandT, out channelT);
            tracks = new List<BMSTrack>();
            GenTracks();
        }

        private void GenTracks()
        {
            foreach (var p in channelT.Table)
            {
                var tr = new BMSTrack();
                tr.Number = p.Key;
                tr.Channels = new List<BMSChannel>();

                foreach(var l in p.Value)
                {
                    var ch = new BMSChannel();
                    ch.Type = l.Type;
                    ch.Action = l.Command;
                    if (ch.IsSystemBeat)
                    {
                        ch.Beats = new List<Beat>() { new Beat() { Resource = l.Message } };
                    }
                    else
                    {
                        ch.Beats =
                            l.Message.Where((c, i) => i % 2 == 0)
                            .Zip(l.Message.Where((c, i) => i % 2 != 0),
                                (a, b) => String.Format("{0}{1}", a, b))
                                .Select(s => new Beat() { Resource = s }).ToList();
                    }

                    tr.Channels.Add(ch);
                }

                tr.Channels = tr.Channels.OrderBy(c => c.Type).ThenBy(c => c.Action).ToList();

                tracks.Add(tr);
            }

            tracks = tracks.OrderBy(t => t.Number).ToList();
        }

        public static SCFile Open(string fileName, Encoding encode = null) 
        {
            if (encode == null)
                encode = Encoding.Default;
            return new SCFile(fileName, encode);
        }

        public ActionList ToActions()
        {
            ActionList actions = new ActionList();
            actions.Information = new BMSInfo(commandT);
            actions.Images = commandT.ImageResources;
            actions.Musics = commandT.MusicResources;
            actions.BPMs = commandT.BPMResources;

            SortedDictionary<int, ActionItem> dt = new SortedDictionary<int, ActionItem>();

            int beatCount = 0;
            foreach (var t in tracks)
            {
                int currentCount = 0;
                foreach (var ch in t.Channels)
                {
                    for (int i = 0; i < ch.Beats.Count; ++i)
                    {
                        ActionItem item;
                        var index = beatCount + i;
                        if (!dt.TryGetValue(index, out item))
                        {
                            item = new ActionItem(t.Number, index);
                            dt[index] = item;
                        }
                        if (ch.IsSystemBGM)
                        {
                            item.BackgroundMusic.Add(ch.Beats[i].Resource);
                        }
                        else
                        {
                            var d = item.GetItemFromType(ch.Type);
                            d[ch.Action] = ch.Beats[i].Resource;
                        }
                    }

                    if (currentCount < ch.Beats.Count)
                        currentCount = ch.Beats.Count;
                }

                beatCount += currentCount;
            }

            actions.Items = dt.Values.ToList();

            return actions;
        }
    }
}
