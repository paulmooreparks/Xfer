using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ParksComputing.XferKit.Workspace.Services;

namespace ParksComputing.XferKit.Api.Store.Impl;
internal class StoreApi : IStoreApi {
    private readonly IStoreService _storeService;

    public StoreApi(
        IStoreService storeService
        ) 
    {
        _storeService = storeService;
    }

    public void clear() => _storeService.Clear();

    public void delete(string key) => _storeService.Delete(key);

    public object? get(string key) => _storeService.Get(key);

    public void set(string key, object value) => _storeService.Set(key, value);

    public string[] keys => _storeService.Keys.ToArray();
    public object[] values => _storeService.Values.ToArray();
}
