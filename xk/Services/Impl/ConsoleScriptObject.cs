namespace ParksComputing.XferKit.Cli.Services;

public class ConsoleScriptObject {
    private static Dictionary<string, int> _counts = new();
    private static int _groupDepth = 0;

    private static string GetIndent() => new string(' ', _groupDepth * 2);

    public static void log(string s) => Console.WriteLine(GetIndent() + s);
    public static void log(params object[] args) => Console.WriteLine(GetIndent() + string.Join(" ", args));
    public static void info(params object[] args) => Console.WriteLine(GetIndent() + "[INFO] " + string.Join(" ", args));
    public static void warn(params object[] args) => Console.WriteLine(GetIndent() + "[WARN] " + string.Join(" ", args));
    public static void error(params object[] args) => Console.Error.WriteLine(GetIndent() + "[ERROR] " + string.Join(" ", args));
    public static void debug(params object[] args) => Console.WriteLine(GetIndent() + "[DEBUG] " + string.Join(" ", args));

    public static void trace(params object[] args) {
        Console.WriteLine(GetIndent() + "[TRACE] " + string.Join(" ", args));
        Console.WriteLine(Environment.StackTrace);
    }

    public static void assert(bool condition, params object[] args) {
        if (!condition) {
            Console.Error.WriteLine(GetIndent() + "[ASSERT] " + (args.Length > 0 ? string.Join(" ", args) : "Assertion failed"));
        }
    }

    public static void count(string label = "default") {
        if (!_counts.ContainsKey(label)) {
            _counts[label] = 0;
        }
        _counts[label]++;
        Console.WriteLine(GetIndent() + $"{label}: {_counts[label]}");
    }

    public static void countReset(string label = "default") {
        if (_counts.ContainsKey(label)) {
            _counts[label] = 0;
        }
    }

    public static void group(string label = "") {
        Console.WriteLine(GetIndent() + (string.IsNullOrEmpty(label) ? "[Group]" : $"[Group: {label}]"));
        _groupDepth++;
    }

    public static void groupEnd() {
        if (_groupDepth > 0)
            _groupDepth--;
    }

    public static void table(IEnumerable<object> data) {
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

    public static void dump(object? data, string label = "Dump", int depth = 0) {
        if (depth > 10) {
            Console.WriteLine(GetIndent() + "[ERROR] Maximum recursion depth reached.");
            return;
        }

        Console.WriteLine(GetIndent() + $"[{label}]");
        Console.WriteLine(GetIndent() + new string('-', label.Length + 2));

        dumpRecursive(data, depth);
    }

    private static void dumpRecursive(object? data, int depth) {
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
                    dumpRecursive(subDict, depth + 1);
                    _groupDepth--;
                }
                else if (kvp.Value is IEnumerable<object> list) {
                    Console.WriteLine();
                    _groupDepth++;
                    foreach (var item in list) {
                        dumpRecursive(item, depth + 1);
                    }
                    _groupDepth--;
                }
                else {
                    Console.WriteLine(formatValue(kvp.Value));
                }
            }
        }
        else if (data is IEnumerable<object> list) {
            foreach (var item in list) {
                dumpRecursive(item, depth + 1);
            }
        }
        else {
            Console.WriteLine(GetIndent() + formatValue(data));
        }
    }

    private static string formatValue(object? value) {
        return value switch {
            null => "null",
            string str => $"\"{str}\"",
            IEnumerable<object> list => $"[{string.Join(", ", list.Select(formatValue))}]",
            _ => value.ToString() ?? "null"
        };
    }
}
