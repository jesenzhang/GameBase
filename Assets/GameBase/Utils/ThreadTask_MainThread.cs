using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GameBase
{
    public partial class ThreadTask
    {
        public struct DelayedQueueItem
        {
            public float time;
            public Action action;
        }

        private List<Action> _actions = new List<Action>();
        private List<DelayedQueueItem> _delayed = new List<DelayedQueueItem>();
        private List<DelayedQueueItem> _currentDelayed = new List<DelayedQueueItem>();

        private List<Action> _currentActions = new List<Action>();



        public static void QueueOnMainThread(Action action)
        {
            QueueOnMainThread(action, 0f);
        }

        public static void QueueOnMainThread(Action action, float time)
        {
            if (time != 0)
            {
                lock (Current._delayed)
                {
                    Current._delayed.Add(new DelayedQueueItem { time = Time.time + time, action = action });
                }
            }
            else
            {
                lock (Current._actions)
                {
                    Current._actions.Add(action);
                }
            }
        }

        // Update is called once per frame  
        void Update()
        {
            lock (_actions)
            {
                _currentActions.Clear();
                _currentActions.AddRange(_actions);
                _actions.Clear();
            }

            if (_currentActions.Count > 0)
            {
                for (int i = 0, count = _currentActions.Count; i < count; i++)
                    _currentActions[i]();
            }

            lock (_delayed)
            {
                _currentDelayed.Clear();

                if (_delayed.Count > 0)
                {
                    DelayedQueueItem item;
                    float time = Time.time;
                    for (int i = 0, count = _delayed.Count; i < count; i++)
                    {
                        item = _delayed[i];
                        if (item.time < time)
                        {
                            _currentDelayed.Add(item);
                            _delayed.RemoveAt(i);
                            i--;
                            count--;
                        }
                    }
                }
            }


            if (_currentDelayed.Count > 0)
            {
                for (int i = 0, count = _currentDelayed.Count; i < count; i++)
                    _currentDelayed[i].action();
            }
        }
    }
}
