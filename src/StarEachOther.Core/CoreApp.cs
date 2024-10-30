using EmbedIO;
using EmbedIO.Actions;
using Octokit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StarEachOther.Core;

public class CoreApp
{


    public CoreApp(Func<Exception, Task> onException, Func<AuthorizationException, Task> onLogin, Action<bool> loginResult)
    {
        ApiWrapper = new GithubApiWrapper(new GitHubClient(new ProductHeaderValue(Config.GithubOAuthAppName)), Config.Url, loginResult);

        WebServer = new WebServer(o => o.WithUrlPrefix(Config.Url).WithMode(HttpListenerMode.EmbedIO));
        WebServer.WithLocalSessionManager();
        WebServer.WithAction(Config.CloseWindowEndpoint, HttpVerbs.Get, ApiWrapper.CloseWindow);
        WebServer.WithAction(Config.SigninEndpoint, HttpVerbs.Get, ApiWrapper.SignIn);
        WebServer.WithAction(Config.SigninCallbackEndpoint, HttpVerbs.Get, ApiWrapper.SignInCallback);
        WebServer.WithModule(new ActionModule("/", HttpVerbs.Any, ctx => ctx.SendDataAsync(new { Message = "未知错误" })));
        OnException = onException;
        OnLogin = onLogin;
    }

    public WebServer WebServer { get; }

    public GithubApiWrapper ApiWrapper { get; }

    public Func<Exception, Task> OnException { get; }

    public Func<AuthorizationException, Task> OnLogin { get; }

    public void Start()
    {
        Task.Run(async () =>
        {
            await WebServer.RunAsync();
        });
    }

    public void Stop()
    {
        WebServer.Dispose();
    }

    async void Request(Func<Task> request)
    {
        try
        {
            await request();
        }
        catch (AuthorizationException ex)
        {
            await OnLogin(ex);
        }
        catch (Exception ex)
        {
            await OnException(ex);
        }
    }

    public void GetAuthUrl(Action<Uri> callback)
    {
        Request(async () =>
        {
            await Task.Yield();
            var authUri = ApiWrapper.GetAuthUrl();
            var fullUrl = $"{Config.Url}{Config.SigninEndpoint}?redirect_url={authUri}";
            callback(new Uri(fullUrl));
        });
    }

    public void GetRepositories(Action<IReadOnlyCollection<Repository>> callback)
    {
        Request(async () =>
        {
            var repositories = await ApiWrapper.GithubClient.Repository.GetAllForCurrent();
            callback(repositories);
        });
    }
}
