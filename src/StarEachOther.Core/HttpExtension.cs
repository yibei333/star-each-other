using SharpDevLib;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace StarEachOther.Core;

public static class HttpExtension
{
    public static async Task<HttpResponse<string>> GetText(string url)
    {
        try
        {
            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
            var text = await client.GetStringAsync(url);
            return new HttpResponse<string>(true, text);
        }
        catch (Exception ex)
        {
            if (Config.ProxyUrl.IsNullOrWhiteSpace()) return new HttpResponse<string>(false, ex.Message);

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
                var text = await client.GetStringAsync(url);
                return new HttpResponse<string>(true, text);
            }
            catch
            {
                return new HttpResponse<string>(false, ex.Message);
            }
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