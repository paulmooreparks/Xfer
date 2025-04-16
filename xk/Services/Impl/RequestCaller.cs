using System.CommandLine;

using Cliffer;
using ParksComputing.XferKit.Cli.Commands;

namespace ParksComputing.XferKit.Cli.Services.Impl;

public class RequestCaller
{
    public string WorkspaceName { get; set; }
    public string RequestName { get; set; }
    public string BaseUrl { get; set; }
    public RootCommand RootCommand { get; }
    public SendCommand SendCommand { get; }

    public RequestCaller(
        RootCommand rootCommand,
        SendCommand sendCommand,
        string workspaceName,
        string requestName,
        string? baseUrl
        )
    {
        RootCommand = rootCommand;
        SendCommand = sendCommand;
        WorkspaceName = workspaceName;
        RequestName = requestName;
        BaseUrl = baseUrl!;
    }

    public object? RunRequest(params object?[]? args)
    {
        if (SendCommand is not null) {
            var result = SendCommand.DoCommand(WorkspaceName, RequestName, BaseUrl, null, null, null, null, null, args);
            return SendCommand.CommandResult;
        }

        return null;
    }
}
