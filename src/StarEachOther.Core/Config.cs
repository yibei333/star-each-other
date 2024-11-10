namespace StarEachOther.Core;

public class Config
{
    public static readonly string ProxyUrl = "http://127.0.0.1:7890";
    public static readonly int Port = 1088;
    public static string Url => $"http://localhost:{Port}";
    public const string SigninCallbackEndpoint = "/SignInCallback";
    public const string GithubOAuthAppName = "StarGithub";
    public const string GithubSecretUrl = "https://raw.githubusercontent.com/yibei333/star-each-other/refs/heads/main/assets/secret.txt";
    public const string GithubShowSupportUrl = "https://raw.githubusercontent.com/yibei333/star-each-other/refs/heads/main/assets/show-support-author.txt";
    public const string SupportAutherUrl = "https://github.com/yibei333/star-each-other/blob/main/assets/buymeacoffee.md";
    public const string RepoListUrl = "https://raw.githubusercontent.com/yibei333/star-each-other/refs/heads/main/assets/list.txt";
    public const string NewIssueUrl = "https://github.com/yibei333/star-each-other/issues/new?title={0}&body={1}";
    public const string VersionUrl = "https://raw.githubusercontent.com/yibei333/star-each-other/refs/heads/main/assets/version.txt";
    public const string ReleaseUrl = "https://github.com/yibei333/star-each-other/releases";
}
