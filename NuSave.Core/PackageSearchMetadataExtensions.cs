namespace NuSave.Core
{
  using NuGet.Protocol.Core.Types;

  public static class PackageSearchMetadataExtensions
  {
    public static Dependency ToDependency(this IPackageSearchMetadata packageDependency)
    {
      return new Dependency
      {
        Id = packageDependency.Identity.Id,
        Version = packageDependency.Identity.Version,
        Authors = string.Join(" ", packageDependency.Authors)
      };
    }
  }
}