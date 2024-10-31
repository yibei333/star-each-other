﻿namespace StarEachOther.Core;

internal class Config
{
    public static readonly int Port = 1088;
    public static string Url => $"http://localhost:{Port}";
    public const string SigninCallbackEndpoint = "/SignInCallback";
    public const string GithubOAuthAppName = "StarEachOther";
    public const string GithubSecretUrl = "https://raw.githubusercontent.com/yibei333/star-each-other/refs/heads/main/assets/secret.txt";
}