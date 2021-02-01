namespace NuSave.Core
{
  using System;
  using System.IO;
  using NuGet.Versioning;

  public class Cache
  {
    private readonly Options _options;

    public class Options
    {
      public string Directory { get; set; }
    }

    public Cache(Options options)
    {
      _options = options;
    }

    public bool PackageExists(string id, NuGetVersion version)
    {
      return File.Exists(GetNuGetPackagePath(id, version)) || Directory.Exists(GetNuGetHierarchicalPath(id, version));
    }

    public string GetNuGetPackagePath(string id, NuGetVersion version)
    {
      return Path.Combine(_options.Directory, $"{id}.{version}.nupkg".ToLower());
    }

    private string GetNuGetHierarchicalPath(string id, NuGetVersion version)
    {
      return Path.Combine(_options.Directory, id.ToLower(), version.ToString());
    }

    public string EnsureDirectory()
    {
      string directory = _options.Directory;

      if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
      {
        return directory;
      }

      if (string.IsNullOrWhiteSpace(directory))
      {
        directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nusave");
      }

      if (!Directory.Exists(directory))
      {
        Directory.CreateDirectory(directory);
      }

      return directory;
    }
  }
}