using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Permissions;
using System.Text;

namespace GameBase
{
    public class SecurityDictionary<T1,T2>
    {

        private Dictionary<T1, T2> dictionary = null;

        private object @lock = null;

        public SecurityDictionary()
       {
           dictionary = new Dictionary<T1,T2>();
           @lock=new object();
       }

       public T2 this[T1 key]
       {
          get
           {
               lock (@lock)
               {
                   if (dictionary != null && dictionary.ContainsKey(key))
                   {
                       return dictionary[key];
                   }
                   return default(T2);
               }

           }
           set
           {
               lock (@lock)
               {
                   if (dictionary != null && dictionary.ContainsKey(key))
                   {
                       dictionary[key] = value;
                   }
               }
              
           }
       }

        public bool ContainsKey(T1 key)
        {
            lock (@lock)
            {
                if (dictionary != null)
                {
                    return dictionary.ContainsKey(key);
                }
                return false;
            }
        }
        public void Add(T1  key, T2 value )
        {

            lock (@lock)
            {
                if (dictionary != null && !dictionary.ContainsKey(key))
                {
                  
                    dictionary.Add(key,value);
                }
            }
        }
        public void Remove(T1  key)
        {

            lock (@lock)
            {
                if (dictionary != null && dictionary.ContainsKey(key))
                {
                    dictionary.Remove(key);

                }
            }
        }
        public void Clear()
        {

            lock (@lock)
            {
                if (dictionary != null)
                {
                    dictionary.Clear();
                }
            }
        }

        public Dictionary<T1,T2>.KeyCollection Keys
        {
            get
            {
                lock (@lock)
                {
                    if (dictionary != null)
                    {
                        return dictionary.Keys;
                    }
                    return null;
                }
            }

        }
        public Dictionary<T1, T2>.ValueCollection Values
        {
            get
            {
                lock (@lock)
                {
                    if (dictionary != null)
                    {
                        return dictionary.Values;
                    }
                    return null;
                }
            }

        }
        public int  Count
        {
            get
            {
                lock (@lock)
                {
                    if (dictionary != null)
                    {
                        return dictionary.Count;
                    }
                    return -1;
                }
            }

        }
    }
}
