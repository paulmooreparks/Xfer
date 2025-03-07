namespace ParksComputing.XferKit.Cli.Services;

internal interface ICommandSplitter
{
    IEnumerable<string> Split(string commandLine);
}