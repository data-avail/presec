using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Presec.Service.Models;
using Enyim.Caching;
using Enyim.Caching.Memcached;
using System.Configuration;


namespace Presec.Service.Cahching
{
    public class Cache<T> : ICache<T>
    {
        public Cache()
        {
            _isActive = ConfigurationManager.AppSettings["Environment"] != "Debug";
            if (_isActive) _client = new MemcachedClient();
        }

        private readonly MemcachedClient _client = null;
        private readonly bool _isActive = false;

        public T Get(string term)
        {

            return _isActive ? _client.Get<T>(term) : default(T);
        }

        public void Set(string term, T GeoSuggestion)
        {
            if (_isActive) _client.Store(StoreMode.Add, term, GeoSuggestion);
        }

        public void Remove(string term)
        {
            if (_isActive) _client.Remove(term);
        }
        

        public void Dispose()
        {
            if (_isActive) _client.Dispose();
        }
    }
}