using NuGet;
using System.IO;

namespace NuSave.Core
{
    public static class IPackageExtensions
    {
        public static string GetFileName(this IPackage package)
        {
            return $"{package.Id}.{package.Version}.nupkg".ToLower();
        }
    }
}
