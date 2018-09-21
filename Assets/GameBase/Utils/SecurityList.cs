using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;

namespace GameBase
{
   public class SecurityList<T>
   {
       private List<T> list = null;

       private object @lock = null;

       public SecurityList()
       {
           list = new List<T>();
           @lock=new object();
       }

       public T this[int index]
       {
          get
           {
               lock (@lock)
               {
                   if (list != null && list.Count > index)
                   {
                       return list[index];
                   }
                   return default(T);
               }

           }
           set
           {
               lock (@lock)
               {
                   if (list != null && list.Count > index)
                   {
                       list[index] = value;
                   }
               }
              
           }

       }

       public void Add(T value)
       {
           lock(@lock)
           {
               if (list != null)
               {
                   list.Add(value);
               }
           }
       }
       public void RemoveAt(int  value)
       {
           lock (@lock)
           {
               if (list != null)
               {
                   list.RemoveAt(value);
               }
           }
       }
       public void Clear()
       {
           lock (@lock)
           {
               if (list != null)
               {
                   list.Clear();
               }
           }
       }

       public int Count
       {
           get
           {
               lock (@lock)
               {
                   if (list != null)
                   {
                       return list.Count;
                   }
                   return -1;
               }
           }
        }

        public bool Contains(T value)
        {
            lock (@lock)
            {
                if (list != null)
                {
                    return list.Contains(value);
                }
                return false;
            }
        }
    }
}
