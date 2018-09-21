using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace GameBase
{
    public partial class ThreadTask : MonoBehaviour
    {
        public static int maxThreads = 8;
        static int numThreads;

        private static ThreadTask _current;
        private int _count;
        public static ThreadTask Current
        {
            get
            {
                Initialize();
                return _current;
            }
        }

        private static bool initialized;

        private class TaskItem
        {
            private int m_parentThreadID = -1;
            private Action m_begin = null;
            private Action m_end = null;
            private bool m_valid = false;

            internal void Init(Action begin, Action end)
            {
                if (m_valid)
                    return;
                m_parentThreadID = GameUtils.GetCurrentThreadID();
                m_begin = begin;
                m_end = end;
                m_valid = true;
            }

            internal void Execute()
            {
                CallBegin();
                CallEnd();
            }

            private void CallBegin()
            {
                if(m_begin != null)
                    m_begin();
            }

            private void CallEnd()
            {
                if (!m_valid)
                    return;
                if (m_end == null)
                    return;
                QueueOnMainThread(m_end);
            }

            internal bool IsValid()
            {
                return m_valid;
            }

            internal void Dispose()
            {
                m_begin = null;
                m_end = null;
                m_parentThreadID = -1;
                m_valid = false;
            }
        }

        private static List<TaskItem> _items = new List<TaskItem>();


        void Awake()
        {
            _current = this;
            initialized = true;
        }

        private static void Initialize()
        {
            if (!initialized)
            {
                if (GameCommon.IsMainThread())
                {
                    if (!Application.isPlaying)
                        return;
                    initialized = true;
                    var g = new GameObject("ThreadTask");
                    UnityEngine.Object.DontDestroyOnLoad(g);
                    _current = g.AddComponent<ThreadTask>();
                }
            }
        }

        private static TaskItem GenTaskItem()
        {
            TaskItem item = null;
            for (int i = 0, count = _items.Count; i < count; i++)
            {
                item = _items[i];
                if (!item.IsValid())
                    return item;
            }

            item = new TaskItem();
            _items.Add(item);
            return item;
        }

        public static Thread RunAsync(Action begin, Action end)
        {
            if (!GameCommon.IsMainThread())
            {
                if(begin != null)
                    begin();
                if (end != null)
                    end();
                return null;
            }

            Initialize();
            while (numThreads >= maxThreads)
            {
                Thread.Sleep(1);
            }

            Interlocked.Increment(ref numThreads);
            TaskItem item;
            {
                item = GenTaskItem();
                item.Init(begin, end);
            }

            ThreadPool.QueueUserWorkItem(RunAction, item);
            return null;
        }

        private static void RunAction(object param)
        {
            TaskItem item = (TaskItem)param;
            try
            {
                item.Execute();
                item.Dispose();
            }
            catch
            {
            }
            finally
            {
                item.Dispose();
                Interlocked.Decrement(ref numThreads);
            }
        }

        void OnDisable()
        {
            if (_current == this)
            {
                _current = null;
            }
        }

        void Start()
        {
        }
    }
}
