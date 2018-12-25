using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MorningBank.Cache
{
    public class NullObjectCache : IWebCache
    {
        public void Remove(string key)
        {
            
        }

        public T Retrieve<T>(string key)
        {
            return default(T);
        }

        public void Store(string key, object obj)
        {
            
        }
    }
}