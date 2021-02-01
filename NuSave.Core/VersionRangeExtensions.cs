namespace NuSave.Core
{
  using NuGet.Versioning;

  public static class VersionRangeExtensions
  {
    public static NuGetVersion ToNuGetVersion(this VersionRange versionRange)
    {
      return versionRange.MaxVersion != null ? versionRange.MaxVersion : versionRange.MinVersion;
    }
  }
}