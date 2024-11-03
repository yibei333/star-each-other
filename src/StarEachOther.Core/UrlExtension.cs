using SharpDevLib;
using System;

namespace StarEachOther.Core;

public static class UrlExtension
{
    public static Tuple<bool, string, string, string> GetUserAndRepoNameByUrl(this string url)
    {
        try
        {
            var array = url.TrimStart("https://github.com/").SplitToList('/');
            return new Tuple<bool, string, string, string>(true, string.Empty, array[0], array[1]);
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string, string, string>(false, ex.Message, string.Empty, string.Empty);
        }
    }
}