namespace NuSave.Core
{
  using NuGet.Protocol.Core.Types;

  public static class PackageSearchMetadataExtensions
  {
    public static Dependency ToDependency(this IPackageSearchMetadata packageSearchMetadata)
    {
      return new()
      {
        Id = packageSearchMetadata.Identity.Id,
        Version = packageSearchMetadata.Identity.Version
      };
    }
  }
}