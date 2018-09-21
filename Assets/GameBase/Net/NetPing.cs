

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameBase
{
    public class NetPing
    {
        enum PingState
        {
            PingNotConnect = 0,
            PingIng = 1,
            PingOK = 2,
        }

        public delegate void NetPingCallBack(bool connected, float delay);
        private PingState state = PingState.PingNotConnect;
        private int index = -1;

        private static NetPing[] pingArr = new NetPing[20];


        private NetPing()
        {
        }

        private static NetPing GenPing()
        {
            NetPing ping = null;
            for (int i = 0, count = pingArr.Length; i < count; i++)
            {
                ping = pingArr[i];
                if (ping == null)
                {
                    ping = new NetPing();
                    pingArr[i] = ping;
                }

                if (ping.index < 0)
                {
                    ping.index = i;
                    return ping;
                }
            }

            return null;
        }

        public static void Ping(string ip, LuaInterface.LuaFunction func, int funcID)
        {
            if (func == null)
                return;

            NetPing ping = GenPing();
            if (ping == null)
            {
                Debug.LogError("ping num is to much");
                return;
            }

            if (Config.Detail_Debug_Log())
                Debug.Log("----------net ping 0->" + ip);
            CoroutineHelper.CreateCoroutineHelper(ping.PingConnect(ip, (state, delay) => 
            {
                ping.index = -1;
                LuaManager.CallFunc_V(func, state, delay, funcID);
            }));
        }

        public static void Ping(string ip, NetPingCallBack callback)
        {
            if (callback == null)
                return;
            NetPing ping = GenPing();
            if (ping == null)
            {
                Debug.LogError("ping num is to much");
                return;
            }

            CoroutineHelper.CreateCoroutineHelper(ping.PingConnect(ip, (state, delay) => 
            {
                ping.index = -1;
                callback(state, delay);
            }));
        }

        private IEnumerator PingConnect(string ip, NetPingCallBack callback)
        {
            if(Config.Detail_Debug_Log())
                Debug.Log("----------net ping 1->" + ip);
            state = PingState.PingNotConnect;
            if (ip == null)
            {
                if (callback != null)
                    callback(false, -1);
                yield break;
            }

            state = PingState.PingIng;
            Ping ping = new Ping(ip);

            int nTime = 0;

            if(Config.Detail_Debug_Log())
                Debug.Log("----------net ping 2->" + ip);
            while (!ping.isDone)
            {
                yield return new WaitForSeconds(0.1f);

                if (nTime > 20) //2秒
                {
                    nTime = 0;
                    state = PingState.PingNotConnect;
                    if (callback != null)
                        callback(false, -1);
                    yield break;
                }
                nTime++;
            }

            if(Config.Detail_Debug_Log())
                Debug.Log("----------net ping 3->" + ip + "^" + ping.isDone + "^" + ping.time);

            if (ping.isDone)
            {
                state = PingState.PingOK;
                if (callback != null)
                {
                    callback(true, ping.time);
                }
                yield break;
            }
        }
    }
}