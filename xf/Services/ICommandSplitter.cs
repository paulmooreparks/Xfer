namespace ParksComputing.Xfer.Cli.Services;

internal interface ICommandSplitter
{
    IEnumerable<string> Split(string commandLine);
}