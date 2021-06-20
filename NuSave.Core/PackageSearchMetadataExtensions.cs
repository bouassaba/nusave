namespace NuSave.Core
{
  using NuGet.Protocol.Core.Types;

  public static class PackageSearchMetadataExtensions
  {
    public static Dependency ToDependency(this IPackageSearchMetadata packageSearchMetadata, SourceRepository sourceRepository)
    {
      return new()
      {
        Id = packageSearchMetadata.Identity.Id,
        Version = packageSearchMetadata.Identity.Version,
        SourceRepository = sourceRepository,
      };
    }
  }
}