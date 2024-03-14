using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace BangumiSubReNamer.Models;

public static class ExtensionTools
{
    public static string GetExtensionName(this string str, string regex)
    {
        var extension1 = Path.GetExtension(str);
        var name1 = Path.GetFileNameWithoutExtension(str);

        var extension2 = Path.GetExtension(name1);

        if (string.IsNullOrEmpty(extension2)
            || Regex.IsMatch(extension2, regex))
        {
            return extension1;
        }
        else
        {
            return extension2 + extension1;
        }
    }

    public static void AddUnique<T>(this ObservableCollection<T> list, T addObj)
    {
        bool isContain = false;
        foreach (var t in list)
        {
            if (addObj.Equals(t))
            {
                isContain = true;
            }
        }

        if (!isContain) list.Add(addObj);
    }

    public static void AddUnique<T>(this List<T> list, T addObj)
    {
        bool isContain = false;
        foreach (var t in list)
        {
            if (addObj.Equals(t))
            {
                isContain = true;
            }
        }

        if (!isContain) list.Add(addObj);
    }

    public static ObservableCollection<string> ConvertToObservableCollection(this string str, string convertPart = "|")
    {
        var strArray = str.Split(convertPart);

        var list = new ObservableCollection<string>();

        foreach (var s in strArray)
        {
            list.Add(s);
        }

        return list;
    }

    public static List<string> ConvertToList(this string str, string convertPart = "|")
    {
        var strArray = str.Split(convertPart);

        var list = new List<string>();

        foreach (var s in strArray)
        {
            list.Add(s);
        }

        return list;
    }

    public static bool EndsWithList(this string str, List<string> list)
    {
        bool isEndsWith = false;

        foreach (var s in list)
        {
            if (str.EndsWith(s))
            {
                isEndsWith = true;
                break;
            }
        }

        return isEndsWith;
    }

    public static string RemoveInvalidFileNameChar(this string str)
    {
        // var invalidFileNameChar = Path.GetInvalidFileNameChars();
        // return invalidFileNameChar.Aggregate(str, (current, c) => current.Replace(c.ToString(), string.Empty));

        var invalidFileNameStr = @"[\\\/\^*×―$%~!@#$…&%￥+=<>《》!！??？:：•'`·、。，；,;""‘’“”]";
        return Regex.Replace(str, invalidFileNameStr, @" ");
    }

    public static string RemoveInvalidPathNameChar(this string str)
    {
        // var invalidPathChars = Path.GetInvalidPathChars();
        // return invalidPathChars.Aggregate(str, (current, c) => current.Replace(c.ToString(), string.Empty));

        var rootpath = Path.GetPathRoot(str);
        if (!string.IsNullOrEmpty(rootpath))
        {
            str = str.Replace(rootpath, "");
        }

        var invalidPathNameStr = @"[\^*×―$%~!@#$…&%￥+=<>《》!！??？:：•'`·、。，；,;""‘’“”]";
        var newStr = Regex.Replace(str, invalidPathNameStr, @" ");

        return rootpath + newStr;
    }

    [DllImport("Kernel32", CharSet = CharSet.Unicode)]
    public static extern bool CreateHardLink(string linkName, string sourceName, IntPtr attribute);
    
    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
    public static extern int StrCmpLogicalW(string psz1, string psz2);
}
