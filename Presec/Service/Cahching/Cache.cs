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

        public T Get(string key)
        {
            return _isActive ? _client.Get<T>(key) : default(T);
        }

        public void Set(string key, T val)
        {
            if (_isActive) _client.Store(StoreMode.Add, key, val);
        }

        public void Remove(string key)
        {
            if (_isActive) _client.Remove(key);
        }
        

        public void Dispose()
        {
            if (_isActive) _client.Dispose();
        }
    }
}