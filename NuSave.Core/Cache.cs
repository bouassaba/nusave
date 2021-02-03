namespace NuSave.Core
{
  using System;
  using System.IO;
  using NuGet.Versioning;

  public class Cache
  {
    public Cache(string directory)
    {
      Directory = directory;

      if (!string.IsNullOrWhiteSpace(Directory) && System.IO.Directory.Exists(Directory))
      {
        return;
      }

      if (string.IsNullOrWhiteSpace(Directory))
      {
        Directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nusave");
      }

      if (!System.IO.Directory.Exists(Directory))
      {
        System.IO.Directory.CreateDirectory(Directory);
      }
    }

    public string Directory { get; }

    public bool PackageExists(string id, NuGetVersion version)
    {
      return File.Exists(GetNuGetPackagePath(id, version)) || System.IO.Directory.Exists(GetNuGetHierarchicalPath(id, version));
    }

    public string GetNuGetPackagePath(string id, NuGetVersion version)
    {
      return Path.Combine(Directory, $"{id}.{version}.nupkg".ToLower());
    }

    private string GetNuGetHierarchicalPath(string id, NuGetVersion version)
    {
      return Path.Combine(Directory, id.ToLower(), version.ToString());
    }
  }
}