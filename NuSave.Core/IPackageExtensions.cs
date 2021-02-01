namespace NuSave.Core
{
  using NuGet.Protocol.Core.Types;

  public static class IPackageExtensions
  {
    public static string GetFileName(this IPackageSearchMetadata package)
    {
      return $"{package.Identity.Id}.{package.Identity.Version}.nupkg".ToLower();
    }
  }
}