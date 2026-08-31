using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Extensions;
using MediaBrowser.Common.Net;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Webhook.Destinations.Ntfy;

/// <summary>
/// Client for the <see cref="NtfyOption"/>.
/// </summary>
public class NtfyClient : BaseClient, IWebhookClient<NtfyOption>
{
    private readonly ILogger<NtfyClient> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="NtfyClient"/> class.
    /// </summary>
    /// <param name="logger">Instance of the <see cref="ILogger{NtfyDestination}"/> interface.</param>
    /// <param name="httpClientFactory">Instance of the <see cref="IHttpClientFactory"/>.</param>
    public NtfyClient(ILogger<NtfyClient> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    /// <inheritdoc />
    public async Task SendAsync(NtfyOption option, Dictionary<string, object> data)
    {
        try
        {
            if (string.IsNullOrEmpty(option.WebhookUri))
            {
                throw new ArgumentException(nameof(option.WebhookUri));
            }

            if (!SendWebhook(_logger, option, data))
            {
                return;
            }

            var body = option.GetMessageBody(data);
            if (!SendMessageBody(_logger, option, body))
            {
                return;
            }

            _logger.LogDebug("SendAsync Body: {@Body}", body);

            var requestUri = $"{option.WebhookUri.TrimEnd('/')}/{option.Topic}";

            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Content = new StringContent(body, Encoding.UTF8, MediaTypeNames.Application.Json);

            if (option.UseCredentials)
            {
                if (!string.IsNullOrEmpty(option.Token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", option.Token);
                }
                else if (!string.IsNullOrEmpty(option.Username) && !string.IsNullOrEmpty(option.Password))
                {
                    var authenticationString = $"{option.Username}:{option.Password}";
                    var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authenticationString));
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
                }
            }

            using var response = await _httpClientFactory
                .CreateClient(NamedClient.Default)
                .SendAsync(request)
                .ConfigureAwait(false);
            await response.LogIfFailedAsync(_logger).ConfigureAwait(false);
        }
        catch (HttpRequestException e)
        {
            _logger.LogWarning(e, "Error sending notification");
        }
    }
}
