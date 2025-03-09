using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

using ParksComputing.XferKit.Cli.Services;
using ParksComputing.XferKit.Workspace.Services;
using ParksComputing.XferKit.Http.Services;
using ParksComputing.XferKit.Api;

namespace ParksComputing.XferKit.Cli.Commands;

[Command("post", "Send resources to the specified API endpoint via a POST request.")]
[Option(typeof(string), "--baseurl", "The base URL of the API to send HTTP requests to.", new[] { "-b" }, IsRequired = false)]
[Argument(typeof(string), "endpoint", "The endpoint to send the POST request to.")]
[Option(typeof(string), "--payload", "Content to send with the request. If input is redirected, content can also be read from standard input.", new[] { "-p" }, Arity = ArgumentArity.ZeroOrOne)]
[Option(typeof(IEnumerable<string>), "--headers", "Headers to include in the request.", new[] { "-h" }, AllowMultipleArgumentsPerToken = true, Arity = ArgumentArity.ZeroOrMore)]
internal class PostCommand {
    private readonly XferKitApi _xk;

    public string ResponseContent { get; protected set; } = string.Empty;
    public int StatusCode { get; protected set; } = 0;
    public System.Net.Http.Headers.HttpResponseHeaders? Headers { get; protected set; } = default;

    public PostCommand(
        XferKitApi xk
        ) 
    {
        _xk = xk;
    }

    public async Task<int> Execute(
        [OptionParam("--baseurl")] string? baseUrl,
        [ArgumentParam("endpoint")] string endpoint,
        [OptionParam("--payload")] string payload,
        [OptionParam("--headers")] IEnumerable<string> headers
        ) 
    {
        // Validate URL format
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var baseUri) || string.IsNullOrWhiteSpace(baseUri.Scheme)) {
            baseUrl ??= _xk.activeWorkspace.BaseUrl;

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

        int result = Result.Success;

        try {
            var response = await _xk.Http.postAsync(baseUrl, payload, headers);

            if (response is null) {
                Console.Error.WriteLine($"Error: No response received from {baseUrl}");
                result = Result.Error;
            }
            else if (!response.IsSuccessStatusCode) {
                Console.Error.WriteLine($"{(int)response.StatusCode} {response.ReasonPhrase} at {baseUrl}");
                result = Result.Error;
            }

            Headers = _xk.Http.headers;
            ResponseContent = _xk.Http.responseContent;
            StatusCode = _xk.Http.statusCode;
            // List<Cookie> responseCookies = cookieContainer.GetCookies(baseUri).Cast<Cookie>().ToList();
        }
        catch (HttpRequestException ex) {
            Console.Error.WriteLine($"Error: HTTP request failed - {ex.Message}");
            return Result.Error;
        }

        return result;
    }
}
