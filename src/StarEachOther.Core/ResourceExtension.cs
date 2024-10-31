using System.IO;
using System.Reflection;
using System.Text;

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
}
