using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.XferKit.Api.Store;

public interface IStoreApi {
    object? get(string key);
    void set(string key, object value);
    void delete(string key);
    void clear();
    IEnumerable<string> keys { get; }
    IEnumerable<object> values { get; }
}
