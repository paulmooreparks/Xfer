using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

using ParksComputing.Xfer.Cli.Services;

namespace ParksComputing.Xfer.Cli.Commands;

[Command("post", "Send resources to the specified API endpoint via a POST request.")]
[Option(typeof(string), "--baseUrl", "The base URL of the API to send the POST request to.", new[] { "-b" }, IsRequired = false)]
[Option(typeof(string), "--endpoint", "The relative endpoint to send the POST request to. This is appended to the base URL.", new[] { "-e" }, IsRequired = true)]
[Option(typeof(string), "--payload", "Content to send with the request. If input is redirected, parameters can also be read from standard input.", new[] { "-p" }, Arity = ArgumentArity.ZeroOrOne)]
internal class PostCommand {
    public async Task<int> Execute(
        [OptionParam("--baseUrl")] string baseUrl,
        [OptionParam("--endpoint")] string endpoint,
        [OptionParam("--payload")] string payload,
        IHttpService httpService,
        IWorkspaceService workspaceService
        ) 
    {
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

        if (Console.IsInputRedirected) {
            var contentString = Console.In.ReadToEnd();
            payload = contentString.Trim();
        }

        var httpClient = new HttpClient();
        string responseContent;

        try {
            responseContent = await httpService.PostAsync(httpClient, baseUrl, endpoint, payload);
        }
        catch (HttpRequestException ex) {
            Console.Error.WriteLine($"Error: HTTP request failed - {ex.Message}");
            return Result.Error;
        }

        Console.WriteLine(responseContent);
        return Result.Success;
    }
}
