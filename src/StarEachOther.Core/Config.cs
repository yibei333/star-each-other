namespace StarEachOther.Core;

public class Config
{
    public static readonly int Port = 1088;
    public static string Url => $"http://localhost:{Port}";
    public const string SigninCallbackEndpoint = "/SignInCallback";
    public const string GithubOAuthAppName = "StarEachOther";
    public const string GithubSecretUrl = "https://raw.githubusercontent.com/yibei333/star-each-other/refs/heads/main/assets/secret.txt";
    public const string SupportAutherUrl = "https://github.com/yibei333/star-each-other";
    public const string RepoListUrl = "https://raw.githubusercontent.com/yibei333/star-each-other/refs/heads/main/assets/list.txt";
}
