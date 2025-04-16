using Microsoft.Extensions.DependencyInjection;

using ParksComputing.XferKit.DataStore.Services;
using ParksComputing.XferKit.DataStore.Services.Impl;
namespace ParksComputing.XferKit.DataStore;

public static class DataStoreServiceCollectionExtensions {
    public static IServiceCollection AddXferKitDataStore(this IServiceCollection services, string databasePath) {
        services.AddSingleton<IKeyValueStore>(_ => new SqliteKeyValueStore(databasePath));
        return services;
    }
}