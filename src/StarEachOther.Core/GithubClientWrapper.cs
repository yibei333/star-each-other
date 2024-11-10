using Octokit;
using SharpDevLib;
using SharpDevLib.Cryptography;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace StarEachOther.Core;

public class GithubClientWrapper
{
    //https://github.com/settings/developers -> OAuth Apps
    string _clientId = "your client id";
    string _clientSecret = "your client secret";
    string? _csrf;
    readonly GitHubClient _client;

    public GithubClientWrapper()
    {
        _client = new GitHubClient(new ProductHeaderValue(Config.GithubOAuthAppName));
    }

    public event AsyncEventHandler<Exception>? RequestException;

    public event AsyncEventHandler<AuthorizationException>? UnAuthenticated;

    public bool IsAuthenticated { get; private set; }

    public bool ShowSupport { get; private set; }

    public async Task<bool> Request(Func<GitHubClient, Task> request)
    {
        try
        {
            if (!IsAuthenticated)
            {
                if (UnAuthenticated is not null) await UnAuthenticated.Invoke(new AuthorizationException());
                return false;
            }
            await request(_client);
            return true;
        }
        catch (AuthorizationException ex)
        {
            IsAuthenticated = false;
            if (UnAuthenticated is not null) await UnAuthenticated.Invoke(ex);
        }
        catch (Exception ex)
        {
            if (RequestException is not null) await RequestException.Invoke(ex);
        }
        return false;
    }

    public async Task Initialize()
    {
        try
        {
            var supportResponse = await HttpExtension.GetText(Config.GithubShowSupportUrl);
            if (supportResponse.Success && supportResponse.Data == "1") ShowSupport = true;

            var secret = ResourceExtension.GetText("Secret.txt").Trim();
            var remoteRepsonse = await HttpExtension.GetText(Config.GithubSecretUrl);
            if (!remoteRepsonse.Success) throw new Exception(remoteRepsonse.Data);
            var remote = remoteRepsonse.Data.Trim();

            var key = secret.Utf8Decode();
            var iv = "0000000000000000".Utf8Decode();
            var enccryptedData = remote.HexStringDecode();

            using var aes = Aes.Create();
            aes.Padding = PaddingMode.PKCS7;
            aes.Mode = CipherMode.CBC;
            aes.SetKey(key);
            aes.SetIV(iv);
            var decrypted = aes.Decrypt(enccryptedData).Utf8Encode();

            var array = decrypted.SplitToList(';');
            _clientId = array[0];
            _clientSecret = array[1];

            var cachedToken = GetCachedToken();
            if (cachedToken.NotNullOrWhiteSpace())
            {
                IsAuthenticated = true;
                _client.Credentials = new Credentials(cachedToken);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"网络不佳\r\n{ex.Message}\r\n可能你需要使用代理", ex);
        }
    }

    public Uri GetAuthUrl()
    {
        _csrf = DateTime.Now.ToUtcTimestamp().ToString();
        var request = new OauthLoginRequest(_clientId)
        {
            Scopes = { "user", "notifications", "public_repo" },
            State = _csrf,
            RedirectUri = new Uri(Config.Url.CombinePath(Config.SigninCallbackEndpoint))
        };
        var oauthLoginUrl = _client.Oauth.GetGitHubLoginUrl(request);
        return oauthLoginUrl;
    }

    internal async Task<bool> GetAccessToken(string state, string code)
    {
        try
        {
            if (state.NotNullOrWhiteSpace() && code.NotNullOrWhiteSpace())
            {
                if (state == _csrf)
                {
                    var request = new OauthTokenRequest(_clientId, _clientSecret, code);
                    var token = await _client.Oauth.CreateAccessToken(request);
                    _client.Credentials = new Credentials(token.AccessToken);
                    IsAuthenticated = true;
                    SetCachedToken(token.AccessToken, token.ExpiresIn);
                    return true;
                }
            }
        }
        catch { }
        return false;
    }

    static string? GetCachedToken()
    {
        try
        {
            var path = GetCachePath();
            if (File.Exists(path))
            {
                var text = File.ReadAllText(path);
                var cache = text.DeSerialize<TokenCache>();
                if (cache.Expires > DateTime.Now.ToUtcTimestamp() && cache.Token.NotNullOrWhiteSpace()) return cache.Token;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
        return null;
    }

    static void SetCachedToken(string token, long expires)
    {
        try
        {
            if (expires <= 0)
            {
                expires = 24 * 60 * 60 * 1000;//默认为不过期,但是安全起见设置为一天
            }

            var path = GetCachePath();
            path.CreateFileIfNotExist();
            var cache = new TokenCache { Token = token, Expires = DateTime.Now.ToUtcTimestamp() + expires };
            var text = cache.Serialize();
            File.WriteAllText(path, text);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    static string GetCachePath()
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            var path = AppDomain.CurrentDomain.BaseDirectory.CombinePath("../userdata/.cache");
            new FileInfo(path).Directory.FullName.CreateDirectoryIfNotExist();
            return path;
        }
        else
        {
            return AppDomain.CurrentDomain.BaseDirectory.CombinePath(".cache");
        }
    }
}

class TokenCache
{
    public string? Token { get; set; }
    public long Expires { get; set; }
}