using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

using ParksComputing.Xfer.Cli.Services;
using ParksComputing.Xfer.Workspace.Services;
using ParksComputing.Xfer.Http.Services;

namespace ParksComputing.Xfer.Cli.Commands;

[Command("post", "Send resources to the specified API endpoint via a POST request.")]
[Argument(typeof(string), "endpoint", "The endpoint to send the POST request to.")]
[Option(typeof(string), "--payload", "Content to send with the request. If input is redirected, content can also be read from standard input.", new[] { "-p" }, Arity = ArgumentArity.ZeroOrOne)]
[Option(typeof(IEnumerable<string>), "--headers", "Headers to include in the request.", new[] { "-h" }, AllowMultipleArgumentsPerToken = true, Arity = ArgumentArity.ZeroOrMore)]
internal class PostCommand {
    public async Task<int> Execute(
        [ArgumentParam("endpoint")] string endpoint,
        [OptionParam("--payload")] string payload,
        [OptionParam("--headers")] IEnumerable<string> headers,
        IHttpService httpService,
        IWorkspaceService workspaceService
        ) 
    {
        var baseUrl = string.Empty;

        // Validate URL format
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var baseUri) || string.IsNullOrWhiteSpace(baseUri.Scheme)) {
            baseUrl = workspaceService.BaseUrl;

            if (string.IsNullOrEmpty(baseUrl) || !Uri.TryCreate(new Uri(baseUrl), endpoint, out baseUri) || string.IsNullOrWhiteSpace(baseUri.Scheme)) {
                Console.Error.WriteLine($"Error: Invalid base URL: {baseUrl}");
                return Result.ErrorInvalidArgument;
            }
        }

        baseUrl = baseUri.ToString();

        if (Console.IsInputRedirected) {
            var payloadString = Console.In.ReadToEnd();
            payload = payloadString.Trim();
        }

        var httpClient = new HttpClient();
        string responseContent = string.Empty;
        int result = Result.Success;

        try {
            var response = await httpService.PostAsync(baseUrl, payload, headers);

            if (!response.IsSuccessStatusCode) {
                Console.Error.WriteLine($"{(int)response.StatusCode} {response.ReasonPhrase} at {baseUrl}");
                result = Result.Error;
            }

            responseContent = await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException ex) {
            Console.Error.WriteLine($"Error: HTTP request failed - {ex.Message}");
            return Result.Error;
        }

        Console.WriteLine(responseContent);
        return result;
    }
}
