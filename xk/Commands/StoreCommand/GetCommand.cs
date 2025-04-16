using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

using ParksComputing.XferKit.Workspace;
using ParksComputing.XferKit.Workspace.Services;

namespace ParksComputing.XferKit.Cli.Commands.StoreCommand;

[Command("get", "Retrieve the value for a given key", Parent = "store")]
[Argument(typeof(string), "key", "The key to retrieve")]
internal class GetCommand {
    private readonly IStoreService _store;

    public GetCommand(
        IStoreService store
        ) {
        _store = store;
    }

    public int Execute(
        string key
        ) 
    {
        if (_store.TryGetValue(key, out var value)) {
            Console.WriteLine(value);
        }
        else {
            Console.Error.WriteLine($"{Constants.ErrorChar} Key '{key}' not found.");
        }
        return 0;
    }
}
