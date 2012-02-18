using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Presec.Service.Models;
using Enyim.Caching;
using Enyim.Caching.Memcached;

namespace Presec.Service.Cahching
{
    public class Cache<T> : ICache<T>
    {
        public Cache()
        {}

        private readonly MemcachedClient _client = new MemcachedClient();

        public T Get(string term)
        {
            return _client.Get<T>(term);
        }

        public void Set(string term, T GeoSuggestion)
        {
            _client.Store(StoreMode.Add, term, GeoSuggestion);
        }

        public void Remove(string term)
        {
            _client.Remove(term);
        }
        

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}