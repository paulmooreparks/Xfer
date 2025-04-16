using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

using ParksComputing.XferKit.Workspace;
using ParksComputing.XferKit.Workspace.Services;

namespace ParksComputing.XferKit.Cli.Commands.StoreCommand;

[Command("set", "Retrieve the value for a given key", Parent = "store")]
[Argument(typeof(string), "key", "The key to set")]
[Argument(typeof(string), "key", "The value to set")]
internal class SetCommand {
    private readonly IStoreService _store;

    public SetCommand(
        IStoreService store
        ) 
    {
        _store = store;
    }

    public int Execute(
        string key,
        string value
        ) 
    {
        _store[key] = value;
        Console.WriteLine($"Set key '{key}' to '{value}'.");
        return Result.Success;
    }
}
