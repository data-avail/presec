using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Presec.Service.Cahching
{
    public interface ICache<T> : IDisposable
    {
        void Set(string key, T obj);

        T Get(string key);

        void Remove(string Key);
    }
}