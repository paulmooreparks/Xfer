using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.XferKit.Cli.Services;
public interface IScriptCliBridge {
    System.CommandLine.RootCommand? RootCommand { get; set; }
    int RunCommand(string commandName, params object?[] args);
}
