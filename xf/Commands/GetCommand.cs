using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

using ParksComputing.Xfer.Cli.Services;

namespace ParksComputing.Xfer.Cli.Commands;

[Command("get", "Retrieve resources from the specified API endpoint via a GET request.")]
[Option(typeof(string), "--baseUrl", "The base URL of the API to send the GET request to.", new[] { "-b" }, IsRequired = false)]
[Option(typeof(string), "--endpoint", "The relative endpoint to send the GET request to. This is appended to the base URL.", new[] { "-e" }, IsRequired = true)]
[Option(typeof(IEnumerable<string>), "--parameters", "Query parameters to include in the request. If input is redirected, parameters can also be read from standard input.", new[] { "-p" }, AllowMultipleArgumentsPerToken = true, Arity = ArgumentArity.ZeroOrMore)]
internal class GetCommand {
    public async Task<int> Execute(
        [OptionParam("--baseUrl")] string baseUrl,
        [OptionParam("--endpoint")] string endpoint,
        [OptionParam("--parameters")] IEnumerable<string> parameters,
        IHttpService httpService,
        IWorkspaceService workspaceService
        ) {
        baseUrl ??= workspaceService.BaseUrl ?? string.Empty;

        if (string.IsNullOrWhiteSpace(baseUrl)) {
            Console.Error.WriteLine("Error: Base URL is required but was not provided.");
            return Result.ErrorInvalidArgument;
        }

        // Validate URL format
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri) || string.IsNullOrWhiteSpace(baseUri.Scheme)) {
            Console.Error.WriteLine($"Error: Invalid base URL: {baseUrl}");
            return Result.ErrorInvalidArgument;
        }

        var paramList = new List<string>();

        if (parameters is not null) { 
            paramList.AddRange(parameters!);
        }

        if (Console.IsInputRedirected) {
            var paramString = Console.In.ReadToEnd();
            paramString = paramString.Trim();
            var inputParams = paramString.Split(' ', StringSplitOptions.None);

            foreach (var param in inputParams) {
                paramList.Add(param);
            }
        }

        var httpClient = new HttpClient();
        string responseContent;

        try {
            responseContent = await httpService.GetAsync(httpClient, baseUrl, endpoint, paramList);
        }
        catch (HttpRequestException ex) {
            Console.Error.WriteLine($"Error: HTTP request failed - {ex.Message}");
            return Result.Error;
        }

        Console.WriteLine(responseContent);
        return Result.Success;
    }
}
