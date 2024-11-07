using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace StarEachOther.Core;

public static class HttpExtension
{
    static int _lastSuccessType;

    public static async Task<HttpResponse<string>> GetText(string url)
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            if (_lastSuccessType == 1) return await GetWithProxy(url, null);
            else
            {
                var result = await GetWithDefaultSetting(url);
                if (result.Success) return result;
                return await GetWithProxy(url, result.Data);
            }
        }
        else
        {
            return await GetWithDefaultSetting(url);
        }
    }

    public static async Task<HttpResponse<string>> GetWithDefaultSetting(string url)
    {
        try
        {
            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
            var response = await client.GetAsync(url);
            var text = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new Exception(text);
            _lastSuccessType = 0;
            return new HttpResponse<string>(true, text);
        }
        catch (Exception ex)
        {
            return new HttpResponse<string>(false, ex.Message);
        }
    }

    public static async Task<HttpResponse<string>> GetWithProxy(string url, string? lastMessage)
    {
        try
        {
            var handler = new HttpClientHandler
            {
                UseProxy = true,
                Proxy = new WebProxy(Config.ProxyUrl)
            };

            using var client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
            var response = await client.GetAsync(url);
            var text = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new Exception(text);
            _lastSuccessType = 1;
            return new HttpResponse<string>(true, text);
        }
        catch (Exception ex)
        {
            _lastSuccessType = 0;
            return new HttpResponse<string>(false, lastMessage ?? ex.Message);
        }
    }
}

public class HttpResponse<T>
{
    internal HttpResponse(bool success, T data)
    {
        Success = success;
        Data = data;
    }

    public bool Success { get; }
    public T Data { get; }
}