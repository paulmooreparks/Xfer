using System.CommandLine;

using Cliffer;
using ParksComputing.XferKit.Cli.Commands;

namespace ParksComputing.XferKit.Cli.Services.Impl;

public class RequestCaller
{
    private readonly IClifferCli? _cli;

    public string WorkspaceName { get; set; }
    public string RequestName { get; set; }
    public string BaseUrl { get; set; }
    public RootCommand RootCommand { get; }
    public SendCommand SendCommand { get; }

    public RequestCaller(
        IClifferCli? cli,
        RootCommand rootCommand,
        SendCommand sendCommand,
        string workspaceName,
        string requestName,
        string? baseUrl
        )
    {
        _cli = cli;
        RootCommand = rootCommand;
        SendCommand = sendCommand;
        WorkspaceName = workspaceName;
        RequestName = requestName;
        BaseUrl = baseUrl!;
    }

    public object? RunRequest(params object?[]? args)
    {
        if (_cli is null) {
            return null;
        }

        if (SendCommand is not null) {
            var result = SendCommand.DoCommand(WorkspaceName, RequestName, BaseUrl, null, null, null, null, null, args, _cli);
            return SendCommand.CommandResult;
        }

        return null;
    }
}
