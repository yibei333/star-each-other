using SharpDevLib;
using SharpDevLib.Cryptography;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StarEachOther.Core;

internal class ResourceExtension
{
    static readonly Assembly _assembly = Assembly.GetAssembly(typeof(ResourceExtension));

    public static string GetText(string name)
    {
        using var stream = _assembly.GetManifestResourceStream($"{_assembly.GetName().Name}.Assets.{name}");
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        var bytes = memoryStream.ToArray();
        return Encoding.UTF8.GetString(bytes);
    }

    public static async Task<Tuple<string, string>> GetConfig()
    {
        try
        {
            var secret = GetText("Secret.txt").Trim();
            var remote = (await GetRemoteSecret()).Trim();

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
            return new Tuple<string, string>(array[0], array[1]);
        }
        catch (Exception ex)
        {
            throw new Exception($"获取配置失败:{ex.Message}", ex);
        }
    }

    static async Task<string> GetRemoteSecret()
    {
        using var client = new HttpClient();
        return await client.GetStringAsync(Config.GithubSecretUrl);
    }
}
