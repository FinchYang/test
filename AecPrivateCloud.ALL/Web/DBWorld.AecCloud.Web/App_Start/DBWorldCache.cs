using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBWorld.AecCloud.Web
{
    public class DBWorldCache
    {
        private static readonly ConcurrentDictionary<string, string> Cache = new ConcurrentDictionary<string,string>(); 
        public static void Add(string key, string value)
        {
            if (!Cache.ContainsKey(key))
            {
                var ok = Cache.TryAdd(key, value);
                if (!ok)
                {
                    Cache.GetOrAdd(key, value);
                }
            }
            else
            {
                Cache[key] = value;
            }
        }

        public static string Get(string key)
        {
            string value = null;
            Cache.TryGetValue(key, out value);
            return value;
        }
    }
}