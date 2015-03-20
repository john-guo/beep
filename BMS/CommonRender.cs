using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using System.Drawing;
using System.IO;

namespace BMS
{
    public abstract class CommonRender : IActionGamePlayer, IDisposable
    {
        Dictionary<string, CachedSound> sounds;
        Dictionary<string, Image> images;
        Dictionary<string, string> actionBGMP1;
        Dictionary<string, string> actionBGMP2;

        public bool AutoPlay { get; set; }
        
        public ActionGameLayout GameLayout { get; set;}

        protected CommonRender()
        {
            AudioPlaybackEngine.Instance.Init();

            sounds = new Dictionary<string, CachedSound>();
            images = new Dictionary<string, Image>();
            actionBGMP1 = null;
            actionBGMP2 = null;

            AutoPlay = false;
            GameLayout = null;
        }

        private T tryGet<T>(Dictionary<string, T> table, string key) where T : class
        {
            T value;
            if (!table.TryGetValue(key, out value))
            {
                return null;
            }

            return value;
        }

        private CachedSound getSound(string key)
        {
            return tryGet(sounds, key);
        }

        protected Image getImage(string key)
        {
            return tryGet(images, key);
        }


        public void PlayMidi(string filename)
        {
            AudioPlaybackEngine.Instance.PlaySound(filename);
        }

        public void InitMusics(Dictionary<string, string> musicResrouces)
        {
            foreach (var pair in musicResrouces)
            {
                if (!File.Exists(pair.Value))
                    continue;
                sounds[pair.Key] = new CachedSound(pair.Value);
            }
        }

        public void InitImages(Dictionary<string, string> imageResrouces)
        {
            foreach (var pair in imageResrouces)
            {
                if (!File.Exists(pair.Value))
                    continue;
                images[pair.Key] = Image.FromFile(pair.Value);
            }
        }

        public void PlayBGM(string bgmNo)
        {
            var sound = getSound(bgmNo);
            if (sound == null)
                return;

            AudioPlaybackEngine.Instance.PlaySound(sound);
        }

        public void P1Hit(string action)
        {
            if (actionBGMP1 == null)
                return;

            string bgmNo;
            if (!actionBGMP1.TryGetValue(action, out bgmNo))
                return;

            PlayBGM(bgmNo);
        }

        public void P2Hit(string action)
        {
            if (actionBGMP2 == null)
                return;

            string bgmNo;
            if (!actionBGMP2.TryGetValue(action, out bgmNo))
                return;

            PlayBGM(bgmNo);
        }


        public void P1Action(Dictionary<string, string> actions)
        {
            actionBGMP1 = new Dictionary<string, string>(actions);

            if (!AutoPlay)
                return;
            //if (GameLayout != null)
            //{
            //    var acts = actions.Where(pair => pair.Value != ActionPlayer.emptyBeat).Select(pair => Convert.ToInt32(pair.Key)).ToArray();
            //    GameLayout.Hit(acts);
            //}

            List<int> hitActions = new List<int>();
            foreach (var pair in actions)
            {
                var empty = pair.Value == ActionPlayer.emptyBeat;

                P1ActionImage(pair.Key, pair.Value, empty);
                PlayBGM(pair.Value);

                if (!empty)
                    hitActions.Add(Convert.ToInt32(pair.Key));
            }

            if (GameLayout != null)
            {
                GameLayout.Hit(hitActions.ToArray(), AutoPlay);
            }
        }

        public void P2Action(Dictionary<string, string> actions)
        {
            actionBGMP2 = new Dictionary<string, string>(actions);

            if (!AutoPlay)
                return;

            List<int> hitActions = new List<int>();
            foreach (var pair in actions)
            {
                var empty = pair.Value == ActionPlayer.emptyBeat;

                P2ActionImage(pair.Key, pair.Value, empty);
                PlayBGM(pair.Value);

                if (!empty)
                    hitActions.Add(Convert.ToInt32(pair.Key));
            }

            if (GameLayout != null)
            {
                GameLayout.Hit(hitActions.ToArray(), AutoPlay, a => a.P2);
            }
        }

        public void Dispose()
        {
            AudioPlaybackEngine.Instance.Dispose();
        }


        protected abstract void P1ActionImage(string action, string bgmNo, bool emptyBeat);
        protected abstract void P2ActionImage(string action, string bgmNo, bool emptyBeat);
        public abstract void PlayImage(string imgNo);
    }
}
