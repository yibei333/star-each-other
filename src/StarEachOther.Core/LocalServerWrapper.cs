using EmbedIO;
using EmbedIO.Actions;
using System.Threading.Tasks;

namespace StarEachOther.Core;

public class LocalServerWrapper
{
    readonly WebServer _webServer;

    public LocalServerWrapper(GithubClientWrapper githubClientWrapper)
    {
        _webServer = new WebServer(o => o.WithUrlPrefix(Config.Url).WithMode(HttpListenerMode.EmbedIO));
        _webServer.WithLocalSessionManager();
        _webServer.WithAction(Config.SigninCallbackEndpoint, HttpVerbs.Get, async (context) =>
        {
            var state = context.Request.QueryString["state"];
            var code = context.Request.QueryString["code"];
            var result = await githubClientWrapper.GetAccessToken(state, code);
            await context.SendStandardHtmlAsync(result ? 200 : 500, writer => writer.Write(ResourceExtension.GetText(result ? "Success.html" : "Fail.html")));
            if (SigninCallback is not null) await SigninCallback.Invoke(result);
        });
        _webServer.WithModule(new ActionModule("/", HttpVerbs.Any, ctx => ctx.SendDataAsync(new { Message = "未知错误" })));
    }

    public event AsyncEventHandler<bool>? SigninCallback;

    public void Start()
    {
        Task.Run(async () =>
        {
            await _webServer.RunAsync();
        });
    }

    public void Stop()
    {
        _webServer.Dispose();
    }
}
