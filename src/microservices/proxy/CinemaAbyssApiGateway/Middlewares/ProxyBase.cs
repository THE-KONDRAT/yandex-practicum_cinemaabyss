using System.Net.Http.Headers;

namespace CinemaAbyssApiGateway.Middlewares;

public abstract class ProxyBase(HttpClient httpClient)
{
    protected async Task ProxyRequest(HttpContext context, string targetBaseUrl)
    {
        var targetUri = new Uri($"{targetBaseUrl}{context.Request.Path}{context.Request.QueryString}");
        var requestMessage = new HttpRequestMessage(new HttpMethod(context.Request.Method), targetUri);
        CopyRequestHeaders(context, requestMessage);
        CopyRequestBody(context, requestMessage);
        AddForwardedHeaders(context, requestMessage);
    
        var response = await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);
        context.Response.StatusCode = (int)response.StatusCode;
    
        CopyResponseHeaders(context, response);
    
        await response.Content.CopyToAsync(context.Response.Body);
    }

    private static void CopyRequestHeaders(HttpContext context, HttpRequestMessage  requestMessage)
    {
        foreach (var header in context.Request.Headers)
        {
            if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase)) continue;
        
            if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
            {
                requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }
    }
    
    private static void CopyResponseHeaders(HttpContext context, HttpResponseMessage  response)
    {
        foreach (var header in response.Headers.Concat(response.Content.Headers))
        {
            context.Response.Headers[header.Key] = header.Value.ToArray();
        }
    }

    private static void CopyRequestBody(HttpContext context, HttpRequestMessage  request)
    {
        if (!(context.Request.ContentLength > 0) && !context.Request.Headers.ContainsKey("Transfer-Encoding")) return;

        request.Content = new StreamContent(context.Request.Body);
        if (context.Request.ContentType == null) return;
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(context.Request.ContentType);
    }

    private static void AddForwardedHeaders(HttpContext context, HttpRequestMessage request)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var proto = context.Request.Scheme;
        var host = context.Request.Host.ToString();

        request.Headers.Add("X-Forwarded-For", clientIp);
        request.Headers.Add("X-Forwarded-Proto", proto);
        request.Headers.Add("X-Forwarded-Host", host);
    
        request.Headers.Add("Forwarded", $"for={clientIp};proto={proto};host={host}");

    }
}