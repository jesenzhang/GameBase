using System.Collections.Generic;
using UnityEngine;

namespace GameBase
{
    public class ZLText
    {
        private static System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding(true);
        private string[] lines;


        public ZLText(byte[] data)
        {
            if (data == null)
                return;
            string str = encoding.GetString(data);
            int index = str.IndexOf("\r\n");
            if(index >= 0)
                lines = str.Split(new string[] { "\r\n" }, System.StringSplitOptions.None);
            else
                lines = str.Split(new string[] { "\n" }, System.StringSplitOptions.None);
        }

        public ZLText()
        {
        }

        public void Dispose()
        {
            lines = null;
        }

        public string[] ReadArr(string property)
        {
            List<string> list = Read(property);
            if (list != null)
                return list.ToArray();
            else
                return null;
        }

        public List<string> Read(string property)
        {
            if (lines == null)
                return null;

            List<string> list = new List<string>();
            string strr;
            bool find = false;
            property = "#" + property;
            for (int i = 1, count = lines.Length; i < count; i++)
            {
                strr = lines[i];
                if (strr == null || strr.Length < 2)
                    continue;

                if (strr[0] == '#')
                {
                    if (find)
                        break;

                    if (strr == property)
                        find = true;
                }

                if (!find)
                    continue;

                if (strr[0] != '@')
                    continue;

                list.Add(strr.Substring(1));
            }

            return list;
        }

        public Dictionary<string, List<string>> ReadAll()
        {
            if (lines == null)
                return null;

            Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();

            string curProperty = null;
            List<string> list = new List<string>();
            string strr;
            for (int i = 1, count = lines.Length; i < count; i++)
            {
                strr = lines[i];
                if (strr == null || strr.Length < 2)
                    continue;

                if (strr[0] == '#')
                {
                    if (curProperty != null)
                    {
                        dic.Add(curProperty, list);
                        curProperty = null;
                        list = new List<string>();
                    }

                    curProperty = strr.Substring(1);
                }

                if (strr[0] != '@')
                    continue;

                list.Add(strr.Substring(1));
            }

            if (curProperty != null)
            {
                dic.Add(curProperty, list);
                curProperty = null;
            }

            return dic;
        }

        public void ReadAll(System.Action<string, List<string>> cp)
        {
            if (lines == null || cp == null)
                return;

            List<string> vList = new List<string>();
            string curProperty = null;

            string strr;
            for (int i = 1, count = lines.Length; i < count; i++)
            {
                strr = lines[i];
                if (strr == null || strr.Length < 2)
                    continue;

                if (strr[0] == '#')
                {
                    if (curProperty != null)
                    {
                        cp(curProperty, vList);
                        curProperty = null;
                        vList.Clear();
                    }

                    curProperty = strr.Substring(1);
                }

                if (strr[0] != '@')
                    continue;

                vList.Add(strr.Substring(1));
            }

            if (curProperty != null)
            {
                cp(curProperty, vList);
            }
        }

        public byte[] Write(Dictionary<string, List<string>> data)
        {
            if (data == null)
                return null;
            if (data.Count == 0)
                return null;

            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.Append("\n");
            Dictionary<string, List<string>>.Enumerator e = data.GetEnumerator();
            while (e.MoveNext())
            {
                builder.Append("#");
                builder.Append(e.Current.Key);
                builder.Append("\n");

                for (int i = 0, count = e.Current.Value.Count; i < count; i++)
                {
                    builder.Append("@");
                    builder.Append(e.Current.Value[i]);
                    builder.Append("\n");
                }

                builder.Append("\n");
            }

            return encoding.GetBytes(builder.ToString());
        }
    }
}
