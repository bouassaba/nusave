namespace NuSave.Core
{
  using System;
  using System.IO;
  using NuGet.Versioning;

  public class Cache
  {
    private readonly string _directory;

    public Cache(string directory)
    {
      _directory = directory;

      if (!string.IsNullOrWhiteSpace(_directory) && System.IO.Directory.Exists(_directory))
      {
        return;
      }

      if (string.IsNullOrWhiteSpace(_directory))
      {
        _directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nusave");
      }

      if (!System.IO.Directory.Exists(_directory))
      {
        System.IO.Directory.CreateDirectory(_directory);
      }
    }

    public string Directory
    {
      get => _directory;
    }

    public bool PackageExists(string id, NuGetVersion version)
    {
      return File.Exists(GetNuGetPackagePath(id, version)) || System.IO.Directory.Exists(GetNuGetHierarchicalPath(id, version));
    }

    public string GetNuGetPackagePath(string id, NuGetVersion version)
    {
      return Path.Combine(_directory, $"{id}.{version}.nupkg".ToLower());
    }

    private string GetNuGetHierarchicalPath(string id, NuGetVersion version)
    {
      return Path.Combine(_directory, id.ToLower(), version.ToString());
    }
  }
}