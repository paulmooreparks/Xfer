namespace ParksComputing.Xfer.Cli.Services;

public class ConsoleScriptObject {
    private readonly Dictionary<string, int> _counts = new();
    private int _groupDepth = 0;

    private string GetIndent() => new string(' ', _groupDepth * 2);

    public void Log(params object[] args) => Console.WriteLine(GetIndent() + string.Join(" ", args));
    public void Info(params object[] args) => Console.WriteLine(GetIndent() + "[INFO] " + string.Join(" ", args));
    public void Warn(params object[] args) => Console.WriteLine(GetIndent() + "[WARN] " + string.Join(" ", args));
    public void Error(params object[] args) => Console.Error.WriteLine(GetIndent() + "[ERROR] " + string.Join(" ", args));
    public void Debug(params object[] args) => Console.WriteLine(GetIndent() + "[DEBUG] " + string.Join(" ", args));

    public void Trace(params object[] args) {
        Console.WriteLine(GetIndent() + "[TRACE] " + string.Join(" ", args));
        Console.WriteLine(Environment.StackTrace);
    }

    public void Assert(bool condition, params object[] args) {
        if (!condition) {
            Console.Error.WriteLine(GetIndent() + "[ASSERT] " + (args.Length > 0 ? string.Join(" ", args) : "Assertion failed"));
        }
    }

    public void Count(string label = "default") {
        if (!_counts.ContainsKey(label)) {
            _counts[label] = 0;
        }
        _counts[label]++;
        Console.WriteLine(GetIndent() + $"{label}: {_counts[label]}");
    }

    public void CountReset(string label = "default") {
        if (_counts.ContainsKey(label)) {
            _counts[label] = 0;
        }
    }

    public void Group(string label = "") {
        Console.WriteLine(GetIndent() + (string.IsNullOrEmpty(label) ? "[Group]" : $"[Group: {label}]"));
        _groupDepth++;
    }

    public void GroupEnd() {
        if (_groupDepth > 0)
            _groupDepth--;
    }

    public void Table(IEnumerable<object> data) {
        if (data == null || !data.Any()) {
            Console.WriteLine(GetIndent() + "(empty table)");
            return;
        }

        var properties = data.First().GetType().GetProperties();
        if (properties.Length == 0) {
            Console.WriteLine(GetIndent() + "(No properties found)");
            return;
        }

        // Print header
        var headers = properties.Select(p => p.Name).ToList();
        Console.WriteLine(GetIndent() + string.Join(" | ", headers));
        Console.WriteLine(GetIndent() + new string('-', headers.Sum(h => h.Length + 3)));

        // Print rows
        foreach (var row in data) {
            var values = properties.Select(p => p.GetValue(row, null)?.ToString() ?? "").ToList();
            Console.WriteLine(GetIndent() + string.Join(" | ", values));
        }
    }

    public void Dump(object? data, string label = "Dump", int depth = 0) {
        if (depth > 10) {
            Console.WriteLine(GetIndent() + "[ERROR] Maximum recursion depth reached.");
            return;
        }

        Console.WriteLine(GetIndent() + $"[{label}]");
        Console.WriteLine(GetIndent() + new string('-', label.Length + 2));

        DumpRecursive(data, depth);
    }

    private void DumpRecursive(object? data, int depth) {
        if (data == null) {
            Console.WriteLine(GetIndent() + "null");
            return;
        }

        if (data is IDictionary<string, object> dict) {
            foreach (var kvp in dict) {
                Console.Write(GetIndent() + $"{kvp.Key}: ");

                if (kvp.Value is IDictionary<string, object> subDict) {
                    Console.WriteLine();
                    _groupDepth++;
                    DumpRecursive(subDict, depth + 1);
                    _groupDepth--;
                }
                else if (kvp.Value is IEnumerable<object> list) {
                    Console.WriteLine();
                    _groupDepth++;
                    foreach (var item in list) {
                        DumpRecursive(item, depth + 1);
                    }
                    _groupDepth--;
                }
                else {
                    Console.WriteLine(FormatValue(kvp.Value));
                }
            }
        }
        else if (data is IEnumerable<object> list) {
            foreach (var item in list) {
                DumpRecursive(item, depth + 1);
            }
        }
        else {
            Console.WriteLine(GetIndent() + FormatValue(data));
        }
    }

    private string FormatValue(object? value) {
        return value switch {
            null => "null",
            string str => $"\"{str}\"",
            IEnumerable<object> list => $"[{string.Join(", ", list.Select(FormatValue))}]",
            _ => value.ToString() ?? "null"
        };
    }
}
