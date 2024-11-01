﻿using Octokit;
using SharpDevLib;
using SharpDevLib.Cryptography;
using System;
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
        }
        catch (Exception ex)
        {
            throw new Exception($"获取配置失败:{ex.Message},可能你需要打开代理并重启应用", ex);
        }
    }

    public Uri GetAuthUrl()
    {
        _csrf = DateTime.Now.ToUtcTimestamp().ToString();
        var request = new OauthLoginRequest(_clientId)
        {
            Scopes = { "user", "notifications" },
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
                    return true;
                }
            }
        }
        catch { }
        return false;
    }
}
