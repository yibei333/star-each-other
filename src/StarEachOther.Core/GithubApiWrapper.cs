using EmbedIO;
using Octokit;
using SharpDevLib;
using System;
using System.Threading.Tasks;

namespace StarEachOther.Core;

public class GithubApiWrapper
{
    //https://github.com/settings/developers -> OAuth Apps
    string _clientId = "your client id";
    string _clientSecret = "your client secret";
    string SigninCallbackEndpointPath => Url.CombinePath(Config.SigninCallbackEndpoint);
    string? _githubToken;
    string? _csrf;

    public GithubApiWrapper(GitHubClient gitHubClient, string url, Action<bool> loginResult)
    {
        GithubClient = gitHubClient;
        Url = url;
        LoginResult = loginResult;
    }

    public GitHubClient GithubClient { get; }

    public string Url { get; }

    public Action<bool> LoginResult { get; }

    public async Task Initialize()
    {
        var key = await ResourceExtension.GetConfig();
        _clientId = key.Item1;
        _clientSecret = key.Item2;
    }

    public Uri GetAuthUrl()
    {
        _csrf = DateTime.Now.ToUtcTimestamp().ToString();
        var request = new OauthLoginRequest(_clientId)
        {
            Scopes = { "user", "notifications" },
            State = _csrf,
            RedirectUri = new Uri(SigninCallbackEndpointPath)
        };
        var oauthLoginUrl = GithubClient.Oauth.GetGitHubLoginUrl(request);
        return oauthLoginUrl;
    }

    public async Task SignInCallback(IHttpContext context)
    {
        var state = context.Request.QueryString["state"];
        var code = context.Request.QueryString["code"];
        if (state.NotNullOrWhiteSpace() && code.NotNullOrWhiteSpace())
        {
            if (state != _csrf)
            {
                await context.SendStandardHtmlAsync(401, writer => writer.Write(ResourceExtension.GetText("Fail.html")));
                return;
            }

            var request = new OauthTokenRequest(_clientId, _clientSecret, code);
            var token = await GithubClient.Oauth.CreateAccessToken(request);
            _githubToken = token.AccessToken;
            GithubClient.Credentials = new Credentials(_githubToken);
            await context.SendStandardHtmlAsync(200, writer => writer.Write(ResourceExtension.GetText("Success.html")));
            LoginResult(true);
        }
        else
        {
            await context.SendStandardHtmlAsync(500, writer => writer.Write(ResourceExtension.GetText("Fail.html")));
            LoginResult(false);
        }
    }

    public string? GetToken() => _githubToken;
}
