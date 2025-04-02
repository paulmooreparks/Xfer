using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ParksComputing.Xfer.Lang.Attributes;

namespace ParksComputing.XferKit.Workspace.Models;

public class MacroDefinition {
    [XferProperty("name")]
    public required string Name { get; set; } = string.Empty;
    [XferProperty("description")]
    public required string Description { get; set; } = string.Empty;
    [XferProperty("command")]
    public required string Command { get; set; } = string.Empty;

    public void Merge(MacroDefinition parentMacro) {
        if (parentMacro is null) {
            return;
        }

        Name ??= parentMacro.Name;
        Description ??= parentMacro.Description;
        Command ??= parentMacro.Command;
    }
}
