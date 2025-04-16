using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

using ParksComputing.XferKit.Workspace;
using ParksComputing.XferKit.Workspace.Services;

namespace ParksComputing.XferKit.Cli.Commands.StoreCommand;

[Command("list", "List all keys and their values", Parent = "store")]
internal class ListCommand {
    private readonly IStoreService _store;

    public ListCommand(
        IStoreService store
        ) 
    {
        _store = store;
    }

    public int Execute() {
        if (_store.Count == 0) {
            Console.Error.WriteLine($"{Constants.WarningChar} Store is empty.");
        }
        else {
            foreach (var kvp in _store) {
                Console.WriteLine($"{kvp.Key}: {kvp.Value}");
            }
        }

        return Result.Success;
    }
}
