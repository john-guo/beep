using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMS
{
    public interface IActionGamePlayer
    {
        void PlayMidi(string filename);
        void InitMusics(Dictionary<string, string> musicResrouces);
        void InitImages(Dictionary<string, string> imageResrouces);

        void PlayBGM(string bgmNo);

        void PlayImage(string imgNo);

        void P1Action(Dictionary<string, string> actions);
        void P2Action(Dictionary<string, string> actions);
    }

    public class ActionPlayer
    {
        private static IEnumerable<int> playerActions = Enumerable.Range(1, 8);
        internal const string emptyBeat = "00";

        public ActionList Actions { get; private set; }
        private IActionGamePlayer rp;
        private float startTime;
        private string currentTrack;
        private int varyAction;
        private float varyStart;

        internal float lastPlay;
        internal int currentAction;
        internal float currentTime;

        private float thresdhold
        {
            get
            {
                return Actions.SPB - 0.1f;
            }
        }

        public ActionPlayer(ActionList actions, IActionGamePlayer realPlayer)
        {
            Actions = actions;
            rp = realPlayer;
            startTime = 0.0f;
            currentAction = -1;
            currentTrack = String.Empty;

            if (actions.Musics != null)
               realPlayer.InitMusics(actions.Musics);
    
            if (actions.Images != null)
                realPlayer.InitImages(actions.Images);

            lastPlay = 0;
        }

        internal float getPlayedTime()
        {
            return currentTime - startTime;
        }

        internal float getVaryTime()
        {
            return currentTime - varyStart;
        }

        private int getCurrentAction()
        {
            var played = getVaryTime();

            return varyAction + (int)Math.Floor(played * Actions.BPS);
        }

        private void intoTrack()
        {
            Debug.WriteLine(String.Format("Into Track {0} {1}", currentTrack, Actions.BPM));
        }

        private void initVariation()
        {
            varyStart = startTime;
            varyAction = 0;
        }

        public void Start(float gameTime)
        {
            startTime = gameTime;
            initVariation();
            if (!String.IsNullOrWhiteSpace(Actions.Information.Midi))
                rp.PlayMidi(Actions.Information.Midi);
            currentAction = -1;
        }


        public bool Play(float gameTime)
        {
            Dictionary<string, string> actions = new Dictionary<string, string>();

            currentTime = gameTime;

            var rcurrent = getCurrentAction();
            if (currentAction >= rcurrent)
            {
                if (gameTime - lastPlay > thresdhold)
                {
                    foreach (var p in playerActions)
                    {
                        var act = p.ToString();

                        actions[act] = emptyBeat;
                    }

                    rp.P1Action(actions);
                    rp.P2Action(actions);
                }
                return true;
            }

            currentAction++;
            if (currentAction >= Actions.Total)
                return false;
            
            var item = Actions.Items[currentAction];
            if (currentTrack != item.Track) 
            {
                intoTrack();
                currentTrack = item.Track;
            }

            Debug.WriteLine(String.Format("{0} {1} {2} {3} {4}", currentTrack, item.Number, Actions.BPM, Actions.BPS, getPlayedTime()));

            foreach (var bgm in item.BackgroundMusic)
            {
                rp.PlayBGM(bgm);
            }

            foreach (var sys in item.System)
            {
                var rtype = item.GetResourceType(sys.Key);

                if (rtype == ResourceType.None)
                {
                    DoSysSpecial(sys.Key, sys.Value);
                }
                else
                {
                    DoSys(rtype, sys.Value);
                }
            }

            actions.Clear();
            foreach (var p in playerActions)
            {
                var act = p.ToString();
                string value;
                if (!item.P1.TryGetValue(act, out value))
                    value = emptyBeat;

                actions[act] = value;
            }
            rp.P1Action(actions);

            actions.Clear();
            foreach (var p in playerActions)
            {
                var act = p.ToString();
                string value;
                if (!item.P2.TryGetValue(act, out value))
                    value = emptyBeat;
                actions[act] = value;
            }

            rp.P2Action(actions);

            lastPlay = gameTime;

            return true;
        }

        //internal class SpeedChangeEventArgs : EventArgs
        //{
        //    public float Speed {get;set;}
        //    public SpeedChangeEventArgs(float speed)
        //    {
        //        Speed = speed;
        //    }
        //}

        //internal event EventHandler<SpeedChangeEventArgs> OnSpeedChange;

        private void SetBPM(float bpm)
        {
            if (Convert.ToInt32(bpm) == 0)
            {
                bpm = -1;
                initVariation();
            }
            else
            {
                varyStart = currentTime;
                varyAction = currentAction;
            }

            Actions.TrackBPM = bpm;

            //if (OnSpeedChange != null)
            //    OnSpeedChange(this, new SpeedChangeEventArgs(Actions.BPM));

            Debug.WriteLine(String.Format("SetBPM {0} {1}", currentTrack, Actions.BPM));
        }

        private void DoSys(ResourceType type, string message)
        {
            switch (type)
            {
                case ResourceType.Music:
                    rp.PlayBGM(message);
                    break;
                case ResourceType.Image:
                    rp.PlayImage(message);
                    break;
                case ResourceType.BPM:
                    string bpm;
                    if (Actions.BPMs.TryGetValue(message, out bpm))
                    {
                        SetBPM(float.Parse(bpm));
                    }

                    break;
            }
        }

        private void DoSysSpecial(string action, string message)
        {
            switch (action)
            {
                case "2":
                    SetBPM(Actions.BPM * float.Parse(message));
                    break;
                case "3":
                    SetBPM(Convert.ToInt32(message, 16));
                    break;
            }
        }
    }
}
