using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMS
{
    public class ActionGameLayout
    {
        public const int ACTION_NONE = 0; 
        public const int ACTION_NORMAL = 1;
        public const int ACTION_GREAT = 2;
        public const int ACTION_PERFECT = 3;
        public const int ACTION_MISSED = 4;
        public const int ACTION_WRONG = 5;

        int box_width;
        int box_height;

        int layout_cols;
        int layout_rows;

        int hit_row;

        int[,] layout;
        int firstHeight;

        float downspeed;

        int hitThreshold = 5;
        Dictionary<int, int[]> hitStatistic;

        public int[,] Layout { get { return layout; } }

        public int FirstHeight 
        { 
            get 
            {
                return firstHeight; 
            }
            private set
            {
                firstHeight = value;
            }
        }

        ActionPlayer player;

        public ActionGameLayout(ActionPlayer p, int cols, int rows, int hitRow, int boxWidth, int boxHeight)
        {
            player = p;
            box_width = boxWidth;
            box_height = boxHeight;

            layout_cols = cols;
            layout_rows = rows;

            hit_row = hitRow;

            layout = new int[cols, rows];

            hitStatistic = new Dictionary<int, int[]>();
        }

        public float GetHitTime(int row = 0)
        {
            if (row >= hit_row)
                return 0;

            return (hit_row - row) * downspeed;
        }

        private void Clear()
        {
            Array.Clear(layout, 0, layout.Length);
        }

        public bool Process(Func<ActionItem, Dictionary<string, string>> selector = null)
        {
            Clear();

            downspeed = player.Actions.BPS;

            if (selector == null)
            {
                selector = a => a.P1;
            }

            float showOffset = player.Actions.SPB;
            if (player.currentAction == -1)
            {
                var hitTime = GetHitTime();
                float occurTime = 0;
                if (hitTime <= player.Actions.FirstInSeconds)
                {
                    occurTime = player.Actions.FirstInSeconds - hitTime;
                }

                var playtime = player.getPlayedTime();
                if (playtime >= occurTime)
                {
                    var realCount = (playtime - occurTime) * downspeed;

                    var showCount = (float)Math.Ceiling(realCount);
                    showOffset = showCount - realCount;
                    
                    int count = (int)showCount;
                    if (count >= player.Actions.Items.Count)
                        count = player.Actions.Items.Count - 1;

                    for (int i = 0; i <= count; ++i)
                    {
                        var item = player.Actions.Items[i];

                        foreach (var pair in selector(item))
                        {
                            var col = Convert.ToInt32(pair.Key) - 1;

                            setLayout(col, count - i, item.Number + 1, pair.Value);
                        }
                    }

                    showOffset = 1.0f - showOffset;
                }
            }
            else
            {
                if (player.currentAction > player.Actions.Items.Count + layout_rows - hit_row)
                    return false;

                var count = hit_row;
                if (count + player.currentAction + 1 >= player.Actions.Items.Count)
                    count = player.Actions.Items.Count - player.currentAction - 2;

                for (int i = 0; i <= count; ++i)
                {
                    var item = player.Actions.Items[player.currentAction + i + 1];

                    foreach (var pair in selector(item))
                    {
                        var col = Convert.ToInt32(pair.Key) - 1;

                        setLayout(col, hit_row - i, item.Number + 1, pair.Value);
                    }
                }

                for (int i = hit_row + 1; i < layout_rows; ++i)
                {
                    var index = player.currentAction - (i - (hit_row + 1));
                    if (index < 0)
                        break;
                    if (index >= player.Actions.Items.Count)
                        continue;

                    var item = player.Actions.Items[index];
                    foreach (var pair in selector(item))
                    {
                        var col = Convert.ToInt32(pair.Key) - 1;

                        setLayout(col, i, item.Number + 1, pair.Value);
                    }
                }

                showOffset = (player.currentTime - player.lastPlay);
            }

            FirstHeight = (int)Math.Ceiling(box_height * showOffset / player.Actions.SPB);

            return true;
        }
    
        /// <summary>
        /// 
        /// </summary>
        /// <returns>Beat number start with 1.</returns>
        public int GetCurrentBeatNumber()
        {
            var current = getCurrentAction();
            if (current == null)
                return 0;

            return current.Number + 1;
        }

        private void setLayout(int col, int row, int beatNo, string beat)
        {
            int[] hits;
            bool isEmpty = beat == ActionPlayer.emptyBeat;
            if (hitStatistic.TryGetValue(beatNo, out hits))
            {
                if (hits[col] == ACTION_NORMAL)
                    isEmpty = true;
            }

            layout[col, row] = isEmpty ? 0 : beatNo;
        }

        private ActionItem getAction(int index, Func<int, int> modifier = null)
        {
            if (index < 0 || index >= player.Actions.Items.Count)
            {
                if (modifier == null)
                    return null;
                index = modifier(index);
            }
            return player.Actions.Items[index];
        }

        private ActionItem getCurrentAction()
        {
            return getAction(player.currentAction);
        }
        
        public int[] Hit(int[] actions, bool autoPlay = false, Func<ActionItem, Dictionary<string, string>> selector = null)
        {
            if (actions.Length == 0)
                return new int[] {};
            return Hit(GetCurrentBeatNumber(), actions, autoPlay, selector);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="beatNo">Beat number start with 1.</param>
        /// <param name="actions">value is beat action start with 1.</param>
        /// <param name="selector"></param>
        /// <returns>Index is beat action start with 0.</returns>
        private int[] Hit(int beatNo, int[] actions, bool autoPlay, Func<ActionItem, Dictionary<string, string>> selector)
        {
            int[] result = new int[layout_cols];

            if (beatNo == 0)
                return result;

            if (selector == null)
            {
                selector = a => a.P1;
            }

            int[] hits;
            if (!hitStatistic.TryGetValue(beatNo, out hits))
            {
                hits = new int[layout_cols];
                hitStatistic[beatNo] = hits;
            }


            ActionItem item;
            var ishit = false;
            //if (!autoPlay && FirstHeight >= hitThreshold)
            if (FirstHeight >= hitThreshold)
            {
                item = getCurrentAction();
                foreach (var pair in selector(item))
                {
                    if (pair.Value == ActionPlayer.emptyBeat)
                        continue;

                    var action = Convert.ToInt32(pair.Key);
                    var hitAct = action - 1;

                    if (hits[hitAct] != ACTION_NONE)
                    {
                        ishit = true;
                        break;
                    }
                }
            }

            if (ishit)
            {
                item = getAction(player.currentAction + 1, i => player.Actions.Total - 1);
            }
            else
            {
                item = getCurrentAction();
            }
            

            foreach (var pair in selector(item))
            {
                if (pair.Value == ActionPlayer.emptyBeat)
                    continue;

                var action = Convert.ToInt32(pair.Key);
                var hitAct = action - 1;

                var index = Array.IndexOf(actions, action);
                if (index < 0)
                {
                    result[hitAct] = ACTION_MISSED;
                }
                else
                {
                    if (hits[hitAct] != ACTION_NONE)
                    {
                        result[hitAct] = ACTION_WRONG;
                        continue;
                    }

                    result[hitAct] = hits[hitAct] = ACTION_NORMAL;

                    actions[index] = ACTION_NONE;
                }
            }
            
            for (int i = 0; i < actions.Length; ++i)
            {
                if (actions[i] == ACTION_NONE)
                    continue;

                result[actions[i] - 1] = ACTION_WRONG;
            }

            return result;
        }
    }
}
