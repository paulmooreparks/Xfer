using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ParksComputing.Xfer.Lang.Attributes;

namespace ParksComputing.XferKit.Workspace.Models;

public class ScriptDefinition {
    [XferProperty("name")]
    public string? Name { get; set; }
    [XferProperty("description")]
    public string? Description { get; set; }
    [XferProperty("initScript")]
    public string? InitScript { get; set; }
    [XferProperty("script")]
    public string? Script { get; set; }
    [XferProperty("arguments")]
    public Dictionary<string, KeyValuePair<string, string>> Arguments { get; set; } = [];

    public void Merge(ScriptDefinition parentScript) {
        if (parentScript is null) {
            return;
        }

        Name ??= parentScript.Name;
        Description ??= parentScript.Description;
        InitScript ??= parentScript.Script;
        Script ??= parentScript.Script;
    }
}

public class Argument {
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Description { get; set; }
}
