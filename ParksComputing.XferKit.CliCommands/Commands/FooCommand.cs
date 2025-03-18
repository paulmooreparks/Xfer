using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

namespace ParksComputing.XferKit.CliCommands.Commands;

[Command("foostuff", "Do foo stuff.")]
public class FooCommand {
    public int Execute() { 
        Console.WriteLine("Foo command executed");
        return Result.Success;
    }
}
