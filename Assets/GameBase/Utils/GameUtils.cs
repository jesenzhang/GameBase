using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Example;

namespace GameBase
{
    public class GameUtils
    {
        

        #region stringBuild
        private static System.Text.StringBuilder _stringBuilder = new System.Text.StringBuilder();
        public static System.Text.StringBuilder stringBuilder
        {
            get { return _stringBuilder; }
        }


        public static void StringBuilderClear()
        {
            _stringBuilder.Remove(0, _stringBuilder.Length);
        }
        #endregion

        #region memoryStream
        private static System.IO.MemoryStream _ms = new System.IO.MemoryStream();
        public static System.IO.MemoryStream ms
        {
            get { return _ms; }
        }
        public static void MemoryStreamClear()
        {
            if (_ms == null)
                return;
            _ms.Position = 0;
            _ms.SetLength(0);
        }
        public static void DisposeMemoryStream()
        {
            _ms.Dispose();
            _ms = null;
        }
        #endregion

        #region  UnDelegateFileList

        private static List<string> unDeleteFileNameList = new List<string>();
        public static void SetUnDeleteFile(string fileName)
        {
            if (unDeleteFileNameList.Contains(fileName))
                return;
            unDeleteFileNameList.Add(fileName);
        }

        public static bool IsUnDeleteFile(string fileName)
        {
            return unDeleteFileNameList.Contains(fileName);

        }
        #endregion

        public static string GetSuffixOfURL()
        {
            string url = "?";
            string arg = "random=";
            System.Random random = new System.Random(unchecked((int)(System.DateTime.Now.Ticks)));
            return url + arg + random.Next(1, 1000).ToString();
        }

        public static int GetCurrentThreadID()
        {
            return System.Threading.Thread.CurrentThread.ManagedThreadId;
        }


        private static string MD5BytesToString(byte[] arr)
        {
            StringBuilder md5StrBuilder = new StringBuilder(arr.Length);
            for (int i = 0, count = arr.Length; i < count; i++)
            {
                md5StrBuilder.Append(arr[i].ToString("X").PadLeft(2, '0'));
            }
            return md5StrBuilder.ToString().ToLower();
        }

        public static void FileMD5Value(String filepath, Action<string> call)
        {
            string str = null;
            ThreadTask.RunAsync(() =>
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] md5ch;
                using (FileStream fs = File.OpenRead(filepath))
                {
                    md5ch = md5.ComputeHash(fs);
                }
                md5.Clear();

                str = MD5BytesToString(md5ch);
            },
            ()=> 
            {
                call(str);
            });
        }

        public static void StringMD5Value(string str, Action<string> call)
        {
            string reStr = null;
            ThreadTask.RunAsync(() =>
            {
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                byte[] bytValue, bytHash;
                bytValue = System.Text.Encoding.UTF8.GetBytes(str);
                bytHash = md5.ComputeHash(bytValue);
                md5.Clear();

                reStr = MD5BytesToString(bytHash);
            },
            ()=> 
            {
                call(reStr);
            });
        }

        public static void BytesMD5Value(byte[] datas, Action<string> call)
        {
            BytesMD5Value(datas, datas.Length, call);
        }

        public static string SyncBytesMD5Value(byte[] datas, int dataLen)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] bytHash;
            bytHash = md5.ComputeHash(datas, 0, dataLen);
            md5.Clear();

            return MD5BytesToString(bytHash);
        }

        public static string SyncStreamMd5Value(Stream s)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] bytHash = md5.ComputeHash(s);
            md5.Clear();

            return MD5BytesToString(bytHash);
        }

        public static void BytesMD5Value(byte[] datas, int dataLen, Action<string> call)
        {
            string str = null;
            ThreadTask.RunAsync(() =>
            {
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                byte[] bytHash;
                bytHash = md5.ComputeHash(datas, 0, dataLen);
                md5.Clear();

                str = MD5BytesToString(bytHash);
            }, 
            ()=> 
            {
                call(str);
            });
        }

        public static Rect RectFToRect(RectF rect)
        {
            return new Rect(rect.X,rect.Y, rect.Width, rect.Height);
        }

        private static Vector3 convertV3 = Vector3.zero;
        public static Vector3 Vector3fToVector3(Vector3f v3)
        {
            convertV3.x = v3.X;
            convertV3.y = v3.Y;
            convertV3.z = v3.Z;

            return convertV3;
        }

        public static Vector3f Vector3ToVector3f(Vector3 v3)
        {
            Vector3f v3f = new Vector3f();
            v3f.X = v3.x;
            v3f.Y = v3.y;
            v3f.Z = v3.z;

            return v3f;
        }

        private static int QuickSortUnit<T>(List<T> array, int low, int high, QuickSortCompare<T> compare)
        {
            T key = array[low];
            while (low < high)
            {
                /*从后向前搜索比key小的值*/
                //while (array[high] >= key && high > low)
                while ((!compare(array[high], key)) && high > low)
                    --high;
                /*比key小的放左边*/
                array[low] = array[high];
                /*从前向后搜索比key大的值，比key大的放右边*/
                while ((compare(array[low], key)) && high > low)
                    ++low;
                /*比key大的放右边*/
                array[high] = array[low];
            }
            /*左边都比key小，右边都比key大。//将key放在游标当前位置。//此时low等于high */
            array[low] = key;
            return high;
        }

        public delegate bool QuickSortCompare<T>(T t1, T t2);
        public static void QuickSort<T>(List<T> array, int low, int high, QuickSortCompare<T> compare)
        {
            if (compare == null)
                return;
            if (low >= high)
                return;
            /*完成一次单元排序*/
            int index = QuickSortUnit(array, low, high, compare);
            /*对左边单元进行排序*/
            QuickSort(array, low, index - 1, compare);
            /*对右边单元进行排序*/
            QuickSort(array, index + 1, high, compare);
        }
    }
}
