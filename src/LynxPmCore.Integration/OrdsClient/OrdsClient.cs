using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace LynxPmCore.Integration.OrdsClient;

internal sealed class OrdsClient(
    HttpClient httpClient,
    IOptions<OrdsClientOptions> options,
    ILogger<OrdsClient> logger) : IOrdsClient
{
    private readonly OrdsClientOptions _opts = options.Value;

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string service,
        TRequest request,
        string? userCode = null,
        CancellationToken ct = default)
        where TRequest : class
        where TResponse : class
    {
        var url = $"{_opts.BaseUrl.TrimEnd('/')}?Service={service}";
        if (!string.IsNullOrWhiteSpace(userCode))
            url += $"&UserCode={Uri.EscapeDataString(userCode)}";

        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_opts.Username}:{_opts.Password}"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

        logger.LogDebug("ORDS call: {Service} → {Url}", service, url);

        var response = await httpClient.PostAsync(url, content, ct);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        logger.LogDebug("ORDS response: {Response}", responseJson[..Math.Min(200, responseJson.Length)]);

        return JsonConvert.DeserializeObject<TResponse>(responseJson);
    }
}
