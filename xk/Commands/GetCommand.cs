using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

using ParksComputing.XferKit.Cli.Services;
using ParksComputing.XferKit.Workspace.Services;
using ParksComputing.XferKit.Http.Services;
using System.Net;
using ParksComputing.XferKit.Api;

namespace ParksComputing.XferKit.Cli.Commands;

[Command("get", "Retrieve resources from the specified API endpoint via a GET request.")]
[Argument(typeof(string), "endpoint", "The endpoint to send the GET request to.")]
[Option(typeof(string), "--baseurl", "The base URL of the API to send HTTP requests to.", new[] { "-b" }, IsRequired = false)]
[Option(typeof(IEnumerable<string>), "--parameters", "Query parameters to include in the request. If input is redirected, parameters can also be read from standard input.", new[] { "-p" }, AllowMultipleArgumentsPerToken = true, Arity = ArgumentArity.ZeroOrMore)]
[Option(typeof(IEnumerable<string>), "--headers", "Headers to include in the request.", new[] { "-h" }, AllowMultipleArgumentsPerToken = true, Arity = ArgumentArity.ZeroOrMore)]
[Option(typeof(IEnumerable<string>), "--cookies", "Cookies to include in the request.", new[] { "-c" }, AllowMultipleArgumentsPerToken = true, Arity = ArgumentArity.ZeroOrMore)]
internal class GetCommand {
    private readonly IHttpService _httpService;
    private readonly IWorkspaceService _workspaceService;
    private readonly XferKitApi _xk;

    public string ResponseContent { get; protected set; } = string.Empty;
    public int StatusCode { get; protected set; } = 0;
    public System.Net.Http.Headers.HttpResponseHeaders? Headers { get; protected set; } = default;

    public GetCommand(
        IHttpService httpService,
        IWorkspaceService workspaceService,
        XferKitApi xk
        ) 
    { 
        _httpService = httpService;
        _workspaceService = workspaceService;
        _xk = xk;
    }

    public async Task<int> Execute(
        [OptionParam("--baseurl")] string? baseUrl,
        [ArgumentParam("endpoint")] string endpoint,
        [OptionParam("--parameters")] IEnumerable<string> parameters,
        [OptionParam("--headers")] IEnumerable<string> headers,
        [OptionParam("--cookies")] IEnumerable<string> cookies
        ) {
        // Validate URL format
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var baseUri) || string.IsNullOrWhiteSpace(baseUri.Scheme)) {
            baseUrl ??= _workspaceService.ActiveWorkspace.BaseUrl;

            if (string.IsNullOrEmpty(baseUrl) || !Uri.TryCreate(new Uri(baseUrl), endpoint, out baseUri) || string.IsNullOrWhiteSpace(baseUri.Scheme)) {
                Console.Error.WriteLine($"Error: Invalid base URL: {baseUrl}");
                return Result.ErrorInvalidArgument;
            }
        }

        baseUrl = baseUri.ToString();
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

        int result = Result.Success;

        try {
            var response = await _xk.GetAsync(baseUrl, paramList, headers);

            if (response is null) {
                Console.Error.WriteLine($"Error: No response received from {baseUrl}");
                result = Result.Error;
            }
            else if (!response.IsSuccessStatusCode) {
                Console.Error.WriteLine($"{(int)response.StatusCode} {response.ReasonPhrase} at {baseUrl}");
                result = Result.Error;
            }

            Headers = _xk.Headers;
            ResponseContent = _xk.ResponseContent;
            StatusCode = _xk.StatusCode;
            // List<Cookie> responseCookies = cookieContainer.GetCookies(baseUri).Cast<Cookie>().ToList();
        }
        catch (HttpRequestException ex) {
            Console.Error.WriteLine($"Error: HTTP request failed - {ex.Message}");
            return Result.Error;
        }

        return result;
    }
}
