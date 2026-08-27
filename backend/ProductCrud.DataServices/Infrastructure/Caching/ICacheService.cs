using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductCrud.DataServices.Infrastructure.Caching;

public interface ICacheService
{
    bool TryGetValue<T>(string key, out T? value);

    void Set<T>(
        string key,
        T value,
        TimeSpan expiration);

    void Remove(string key);
}