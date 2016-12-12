using NuGet;
using System.IO;

namespace NuSave.Core
{
    public static class IPackageExtensions
    {
        public static string GetFileName(this IPackage package)
        {
            return $"{package.Id}.{package.Version}.nupkg";
        }

        public static string GetHierarchialDirPath(this IPackage package, string baseDir)
        {
            return Path.Combine(baseDir, package.Id, package.Version.ToString());
        }
    }
}
