using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using LuaInterface;

namespace GameBase
{
    public static class LuaCThread
    {
#if !UNITY_EDITOR && UNITY_IPHONE
        private const string SDLL = "__Internal";
#else
        private const string SDLL = "serioso";
#endif

        [DllImport(SDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern int sthread_lcreate(IntPtr L, int channel, int init_func, int update_func, int sleep_time);

        [DllImport(SDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern int sthread_create(int channel);

        [DllImport(SDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern void sthread_close(int channel);

        [DllImport(SDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern int sthread_send(int channel, int gID, int uID, byte[] msg, int msglen);

        [DllImport(SDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern int sthread_receive(int channel, ref int gID, ref int uID, byte[] arr, int capacity);


        abstract class MsgBase
        {
            internal enum Type
            {
                STRING = 0,
                PROTO = 1,
            }

            internal Type type;
            internal int toChannel;
            internal int gID;
            internal int uID;

            internal abstract byte[] GetMsg(ref int msgLen);
        }

        class ProtoMsg : MsgBase
        {
            internal SProto msg;

            internal ProtoMsg()
            {
                type = Type.PROTO;
            }

            internal override byte[] GetMsg(ref int msgLen)
            {
                if (msg == null)
                    return null;

                msgLen = msg.Pack();
                return msg.GetMsgBuf();
            }
        }

        class CThreadContext
        {
            private List<MsgBase> msgs = new List<MsgBase>();
            private object lockobj = new object();
            private object locksend = new object();
            private bool sending = false;

            internal int channel = -1;
            internal Action<int, int, int> receiveCall = null;

            internal void AddMsg(int toChannel, int gID, int uID, SProto msg)
            {
                ProtoMsg pm = new ProtoMsg();
                pm.gID = gID;
                pm.uID = uID;
                pm.msg = msg;
                pm.toChannel = toChannel;
                lock (lockobj)
                {
                    msgs.Add(pm);
                }
            }

            internal void Send()
            {
                if (sending)
                    return;

                if (msgs.Count <= 0)
                    return;

                lock (locksend)
                {
                    if (sending)
                        return;

                    sending = true;
                }

                //may contains timing sequence error
                ThreadTask.RunAsync(() =>
                {
                    List<MsgBase> list = new List<MsgBase>();
                    lock (lockobj)
                    {
                        list.AddRange(msgs);
                        msgs.Clear();
                    }

                    MsgBase mb;
                    for (int i = 0, count = list.Count; i < count; i++)
                    {
                        mb = list[i];
                        int msgLen = 0;
                        try
                        {
                            byte[] arr = mb.GetMsg(ref msgLen);
                            sthread_send(mb.toChannel, mb.gID, mb.uID, arr, msgLen);
                        }
                        catch (Exception e)
                        {
                            Debugger.LogError("cthread send error->" + e.ToString());
                        }
                    }

                    lock (locksend)
                    {
                        sending = false;
                    }
                },
                null);
            }

            internal void Receive(byte[] arr)
            {
                if (receiveCall == null)
                    return;

                if (arr == null)
                    return;

                int gID = -1;
                int uID = -1;
                int re = -1;
                ThreadTask.RunAsync(() =>
                {
                    re = sthread_receive(channel, ref gID, ref uID, arr, arr.Length);
                },
                ()=> 
                {
                    receiveCall(gID, uID, re);
                });
            }
        }

        private static Dictionary<int, LuaContext> createdThread = new Dictionary<int, LuaContext>();

        private static List<CThreadContext> cthreads = new List<CThreadContext>();

        public static int CreateThread(int channel, string recvFileName, string recvClassName, bool main, int sleepTime)
        {
            if (createdThread.ContainsKey(channel))
                return -1000;
            LuaContext luaContext;
            if (main)
            {
                luaContext = LuaManager.GetLuaContext();
            }
            else
            {
                //luaContext = new LuaContext(LuaManager.GetRegisterGameCall());
                luaContext = LuaContext.Create(LuaManager.GetRegisterGameCall());
                luaContext.RegisterLibs();

                DllHelper.SNavLua_Init(luaContext.GetLuaState().GetL());
            }

            luaContext.Require(recvFileName);
            LuaFunction initFunc = luaContext.GetFunction(recvClassName + ".Init");
            LuaFunction updateFunc = luaContext.GetFunction(recvClassName + ".Update");

            int re = sthread_lcreate(luaContext.GetLuaState().GetL(), channel, initFunc.GetReference(), updateFunc.GetReference(), sleepTime);

            if (re >= 0)
                createdThread.Add(channel, luaContext);

            return re;
        }

        public static int CreateCThread(int channel, Action<int, int, int> receiveCall)
        {
            if (createdThread.ContainsKey(channel))
                return -1000;

            int re = sthread_create(channel);
            if (re >= 0)
            {
                CThreadContext context = new CThreadContext();
                context.channel = channel;
                context.receiveCall = receiveCall;

                if (channel >= cthreads.Count)
                {
                    for (int i = cthreads.Count; i <= channel; i++)
                    {
                        cthreads.Add(null);
                    }
                }

                cthreads[channel] = context;
                createdThread.Add(channel, null);
            }

            return re;
        }

        private static CThreadContext GetCThreadContext(int channel)
        {
            if (channel < 0 || channel >= cthreads.Count)
                return null;
            return cthreads[channel];
        }

        public static int Receive(int channel, byte[] buf)
        {
            CThreadContext context = GetCThreadContext(channel);
            if (context == null)
                return -1;
            context.Receive(buf);
            return 0;
        }

        public static int SendMsg(int channel, int toChannel, int gID, int uID, SProto msg)
        {
            CThreadContext context = GetCThreadContext(channel);
            if (context == null)
                return -1;

            context.AddMsg(toChannel, gID, uID, msg);
            return 0;
        }

        public static void CThreadRun(int channel)
        {
            CThreadContext context = GetCThreadContext(channel);
            if (context == null)
                return;

            context.Send();
        }

        public static void CloseAll()
        {
            Dictionary<int, LuaContext>.Enumerator e = createdThread.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current.Value != null)
                {
                    sthread_close(e.Current.Key);
                }
            }

            createdThread.Clear();
        }
    }
}
