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
            using var client = new HttpClient();
            var text = await client.GetStringAsync(url);
            return new HttpResponse<string>(true, text);
        }
        catch
        {
            try
            {
                var handler = new HttpClientHandler
                {
                    UseProxy = true,
                    Proxy = new WebProxy("http://localhost:7890")
                };

                using var client = new HttpClient(handler);
                var text = await client.GetStringAsync(url);
                return new HttpResponse<string>(true, text);
            }
            catch (Exception ex)
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