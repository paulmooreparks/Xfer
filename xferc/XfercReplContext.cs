using System.Reflection;

namespace ParksComputing.Xferc;

internal class XfercReplContext : Cliffer.DefaultReplContext {
    public string Title => "Xfer CLI Application";
    public override string[] GetPopCommands() => [];

    public override string GetTitleMessage() {
        Assembly assembly = Assembly.GetExecutingAssembly();
        Version? version = assembly.GetName().Version;
        string versionString = version?.ToString() ?? "Unknown";
        return $"{Title} v{versionString}";
    }
}
