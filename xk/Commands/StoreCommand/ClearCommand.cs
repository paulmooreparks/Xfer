using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

using ParksComputing.XferKit.Workspace;
using ParksComputing.XferKit.Workspace.Services;

namespace ParksComputing.XferKit.Cli.Commands.StoreCommand;

[Command("clear", "Clear all keys from the store", Parent = "store")]
internal class ClearCommand {
    private readonly IStoreService _store;

    public ClearCommand(
        IStoreService store
        ) 
    {
        _store = store;
    }

    public int Execute() {
        _store.Clear();
        Console.WriteLine("Store cleared.");

        return Result.Success;
    }
}
