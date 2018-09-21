using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;

namespace GameBase
{
    public class WwwWorkpool : MonoBehaviour
    {
        public static WwwWorkpool instance = null;

        private bool inited = false;

        public int maxCapcity = 2;



        private void Init()
        {
            if (inited) return;

            for (int i = 0; i < maxCapcity; i++)
            {
                workItems.Add(new WwwWorkItem(""));
            }
            inited = true;

        }

        private SecurityLinkList<WorkTarget> targets = new SecurityLinkList<WorkTarget>();


        private void Awake()
        {
            instance = this;
            Init();
        }

        private void StartDownload(Func<IEnumerator> aFunc)
        {
            this.StartCoroutine(aFunc());
        }

        public class WwwWorkItem
        {
            public enum EnumWwwStatus
            {
                ready,
                working,
            }

            private bool removeImpurity;

            public WwwWorkItem(string url)
            {
                this.url = url;
            }

            public string url = "";


            public UnityWebRequest www = null;
            private int totalDownloadSize = -1;

            public EnumWwwStatus wwwStatus = EnumWwwStatus.ready;
            private string _dataPath;
            public string dataPath
            {
                get { return _dataPath; }
            }

            public void SetTotal(int size)
            {
                totalDownloadSize = size;
            }

            public void SetRemoveIMP(bool v)
            {
                removeImpurity = v;
            }

            public void StartDownload(string url)
            {
                this.url = url;
                WwwWorkpool.instance.StartDownload(Download);
            }

            private IEnumerable CheckReady()
            {
                while (true)
                {
                    if (wwwStatus == EnumWwwStatus.ready)
                        yield break;
                }
            }

            private IEnumerator Download()
            {
                if (this.wwwStatus == EnumWwwStatus.working)
                {
                    yield break;
                }
                if (www != null)
                {
                    www.Dispose();
                    www = null;
                }
                this.wwwStatus = EnumWwwStatus.working;

                {
                    www = UnityWebRequest.Get(this.url);
                    string tempPath = LoadAndToTemp.GenTempPath();
                    DownloadHandlerFile dhf = new DownloadHandlerFile(tempPath);
                    www.downloadHandler = dhf;
                    yield return www.SendWebRequest();

                    if (www.isDone && www.error == null)
                    {
                        _dataPath = tempPath;
                        wwwStatus = EnumWwwStatus.ready;
                        Dispose();
                        WwwWorkpool.instance.Notify(this, EnumMessageType.DownSuccess);
                    }
                    else if (www.error != null)
                    {
                        Debugger.LogError("download error->" + this.url + "^" + www.error);
                        _dataPath = null;
                        this.wwwStatus = EnumWwwStatus.ready;
                        Dispose();
                        WwwWorkpool.instance.Notify(this, EnumMessageType.HasError);
                    }
                    else
                    {
                        this.wwwStatus = EnumWwwStatus.ready;
                        _dataPath = null;
                        Dispose();
                        Debugger.LogError("www download done, but has error->" + www.error);
                        WwwWorkpool.instance.Notify(this, EnumMessageType.HasError);
                    }
                }
            }

            public void Dispose()
            {
                if (www != null)
                {
                    www.Dispose();
                    www = null;
                }
            }
        }

        public enum EnumMessageType
        {
            DownSuccess,
            HasError,
        }

        private void Notify(WwwWorkItem wwwWorkItem, EnumMessageType message)
        {
            try
            {
                if (links.ContainsKey(wwwWorkItem))
                {
                    WorkTarget target = links[wwwWorkItem];
                    switch (message)
                    {
                        case EnumMessageType.DownSuccess:
                            if (target != null)
                            {
                                if (target.successCallBack != null)
                                {
                                    target.successCallBack(target.url, wwwWorkItem.dataPath, target.successCallParam);
                                }
                            }
                            break;
                        case EnumMessageType.HasError:
                            if (target != null)
                            {
                                System.Action<string, string, System.Object> callback = target.successCallBack;

                                if (callback != null)
                                {
                                    callback(target.url, null, target.successCallParam);
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("www download call back error->" + message + "^" + e.ToString());
            }

            WwwWorkpool.instance.CheckTheQueueAndWork();
        }

        private class WorkTarget
        {
            public string url;
            public System.Action<string, string, System.Object> successCallBack;
            public System.Object successCallParam;

            private bool removeImpurity;
            private int totalSize;

            public bool GetRemoveIMP()
            {
                return removeImpurity;
            }

            public int GetTotal()
            {
                return totalSize;
            }

            public WorkTarget(string url, System.Action<string, string, System.Object> successCallBack, System.Object successCallParam, bool removeImpurity, int totalSize)
            {
                this.url = url;
                this.successCallBack = successCallBack;
                this.successCallParam = successCallParam;
                this.removeImpurity = removeImpurity;
                this.totalSize = totalSize;
            }
        }

        private Dictionary<WwwWorkItem, WorkTarget> links = new Dictionary<WwwWorkItem, WorkTarget>();

        private List<WwwWorkItem> workItems = new List<WwwWorkItem>(3);

        public void AddWork(string url, int totalSize, System.Action<string, string, System.Object> Callback, System.Object callParam, bool removeImpurity)
        {
            targets.AddFirst(new WorkTarget(url, Callback, callParam, removeImpurity, totalSize));

            CheckTheQueueAndWork();
        }

        private void CheckTheQueueAndWork()
        {
            if (targets.Count <= 0)
            {
                return;
            }
            else
            {
                foreach (var wwwWorkItem in workItems)
                {
                    if (wwwWorkItem.wwwStatus == WwwWorkItem.EnumWwwStatus.ready)
                    {
                        if (targets.Count > 0)
                        {
                            WorkTarget target = targets.Last();

                            targets.RemoveLast();

                            if (links.ContainsKey(wwwWorkItem))
                            {
                                links.Remove(wwwWorkItem);
                            }
                            links.Add(wwwWorkItem, target);

                            wwwWorkItem.SetRemoveIMP(target.GetRemoveIMP());
                            wwwWorkItem.StartDownload(target.url);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }
}