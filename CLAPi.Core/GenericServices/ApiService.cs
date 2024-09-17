namespace CLAPi.Core.GenericServices;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

public class ApiService(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<TResponse?> GetAsync<TResponse>(string url, Dictionary<string, string>? headers = null, string? token = null)
    {
        try
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

            if (!string.IsNullOrEmpty(token))
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            if (headers != null)
            {
                foreach (var item in headers)
                {
                    requestMessage.Headers.Add(item.Key, item.Value);
                }
            }

            var response = await _httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TResponse>(jsonResponse);
        }
        catch (Exception ex)
        {
            ErrorFormats.ThrowValidationException(ex.Message, nameof(ex.Message));
            return default;
        }
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest requestContent, Dictionary<string, string>? headers = null, string? token = null)
    {
        try
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);

            if (!string.IsNullOrEmpty(token))
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            if (headers != null)
            {
                foreach (var item in headers)
                {
                    requestMessage.Headers.Add(item.Key, item.Value);
                }
            }

            var jsonRequest = JsonSerializer.Serialize(requestContent);
            requestMessage.Content = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TResponse>(jsonResponse);
        }
        catch (Exception ex)
        {
            ErrorFormats.ThrowValidationException(ex.Message, nameof(ex.Message));
            return default;
        }
    }
}
