using System.IO;
using System.Reflection;

namespace TwitchTools.Utils;

internal static class PathUtils
{
    public static readonly string AssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
}