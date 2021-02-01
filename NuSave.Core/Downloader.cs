namespace NuSave.Core
{
  using Newtonsoft.Json;
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Net;
  using System.Threading;
  using System.Xml.Linq;
  using NuGet.Common;
  using NuGet.Protocol;
  using NuGet.Protocol.Core.Types;
  using NuGet.Versioning;

  public class Downloader
  {
    private const string DefaultSource = "https://api.nuget.org/v3/index.json";
    private readonly string _source;
    private readonly string _outputDirectory;
    private readonly string _id;
    private readonly string _version;
    private readonly bool _allowPreRelease;
    private readonly bool _allowUnlisted;
    private readonly bool _silent;
    private readonly bool _json;
    private List<IPackageSearchMetadata> _toDownload = new();

    public Downloader(
      string source,
      string outputDirectory,
      string id,
      string version,
      bool allowPreRelease = false,
      bool allowUnlisted = false,
      bool silent = false,
      bool json = false)
    {
      _source = source;
      _outputDirectory = outputDirectory;
      _id = id;
      _version = version;
      _allowPreRelease = allowPreRelease;
      _allowUnlisted = allowUnlisted;
      _json = json;
      _silent = _json || silent;

      _outputDirectory = EnsureOutputDirectory(outputDirectory);
    }

    public void Download()
    {
      if (!_silent)
      {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Downloading");
        Console.ResetColor();
      }

      foreach (var package in _toDownload)
      {
        if (PackageExists(package.Identity.Id, package.Identity.Version.ToString()))
        {
          continue;
        }

        if (!_silent)
        {
          Console.WriteLine($"{package.Identity.Id} {package.Identity.Version}");
        }

        // We keep retrying forever until the user will press Ctrl-C
        // This lets the user decide when to stop retrying.
        // The reason for this is that building the dependencies list is expensive
        // on slow internet connection, when the CLI crashes because of a WebException
        // the user has to wait for the dependencies list to build rebuild again.
        while (true)
        {
          try
          {
            string nugetPackageOutputPath = GetNuGetPackagePath(package.Identity.Id, package.Identity.Version.ToString());
            using FileStream packageStream = File.Create(nugetPackageOutputPath);

            FindPackageByIdResource resource = SourceRepository.GetResource<FindPackageByIdResource>();
            resource.CopyNupkgToStreamAsync(
              package.Identity.Id,
              package.Identity.Version,
              packageStream,
              SourceCacheContext,
              NullLogger.Instance,
              CancellationToken.None).Wait();

            break;
          }
          catch (WebException e)
          {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{e.Message}. Retrying in one second...");
            Console.ResetColor();
            Thread.Sleep(1000);
          }
        }
      }
    }

    public void ResolveDependencies(string csprojPath = null)
    {
      if (_toDownload != null && _toDownload.Count > 1)
      {
        _toDownload = new List<IPackageSearchMetadata>();
      }

      if (!_silent)
      {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Resolving dependencies");
        Console.ResetColor();
      }

      if (csprojPath == null)
      {
        IPackageSearchMetadata package = FindPackage(_id, SemanticVersion.Parse(_version), _allowPreRelease, _allowUnlisted);

        if (package == null)
        {
          throw new Exception("Could not resolve package");
        }

        ResolveDependencies(package);
      }
      else
      {
        XNamespace @namespace = "http://schemas.microsoft.com/developer/msbuild/2003";
        XDocument csprojDoc = XDocument.Load(csprojPath);

        IEnumerable<MsBuildPackageRef> references = csprojDoc
          .Element(@namespace + "Project")
          ?.Elements(@namespace + "ItemGroup")
          .Elements(@namespace + "PackageReference")
          .Select(e => new MsBuildPackageRef()
          {
            Include = e.Attribute("Include")?.Value,
            Version = e.Element(@namespace + "Version")?.Value
          });
        ResolveDependencies(references);

        IEnumerable<MsBuildPackageRef> dotnetCliToolReferences = csprojDoc
          .Element(@namespace + "Project")
          ?.Elements(@namespace + "ItemGroup")
          .Elements(@namespace + "DotNetCliToolReference")
          .Select(e => new MsBuildPackageRef()
          {
            Include = e.Attribute("Include")?.Value,
            Version = e.Element(@namespace + "Version")?.Value
          });
        ResolveDependencies(dotnetCliToolReferences);
      }

      if (_json)
      {
        Console.WriteLine(JsonConvert.SerializeObject(GetDependencies(), Formatting.Indented));
      }
    }

    /// <summary>
    /// Convenience method that can be used in powershell in combination with Out-GridView
    /// </summary>
    /// <returns></returns>
    public List<NuGetPackage> GetDependencies()
    {
      var list = new List<NuGetPackage>();
      foreach (var p in _toDownload)
      {
        list.Add(new NuGetPackage
        {
          Id = p.Identity.Id,
          Version = p.Identity.Version.ToString(),
          Authors = string.Join(" ", p.Authors)
        });
      }

      return list;
    }

    private string EnsureOutputDirectory(string value)
    {
      string outputDirectory = value;

      if (!string.IsNullOrWhiteSpace(outputDirectory) && Directory.Exists(outputDirectory))
      {
        return outputDirectory;
      }

      if (string.IsNullOrWhiteSpace(outputDirectory))
      {
        outputDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nusave");
      }

      Directory.CreateDirectory(outputDirectory);

      if (!_silent)
      {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Using output directory: {outputDirectory}");
        Console.ResetColor();
      }

      return outputDirectory;
    }

    private void ResolveDependencies(IEnumerable<MsBuildPackageRef> references)
    {
      foreach (var packageRef in references)
      {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"{packageRef.Include} {packageRef.Version}");
        Console.ResetColor();

        ResolveDependencies(packageRef);
      }
    }

    private string GetNuGetPackagePath(string id, string version)
    {
      return Path.Combine(_outputDirectory, $"{id}.{version}.nupkg".ToLower());
    }

    private string GetNuGetHierarchicalDirPath(string id, string version)
    {
      return Path.Combine(_outputDirectory, id.ToLower(), version);
    }

    private bool PackageExists(string id, string version)
    {
      string nugetPackagePath = GetNuGetPackagePath(id, version);
      if (File.Exists(nugetPackagePath))
      {
        return true;
      }

      string nuGetHierarchicalDirPath = GetNuGetHierarchicalDirPath(id, version);
      if (Directory.Exists(nuGetHierarchicalDirPath))
      {
        return true;
      }

      return false;
    }

    private void ResolveDependencies(MsBuildPackageRef msBuildPackageRef)
    {
      IPackageSearchMetadata nugetPackage = FindPackage(msBuildPackageRef.Include,
        SemanticVersion.Parse(msBuildPackageRef.Version), true, true);
      ResolveDependencies(nugetPackage);
    }

    private void ResolveDependencies(IPackageSearchMetadata package)
    {
      if (PackageExists(package.Identity.Id, package.Identity.Version.ToString()))
      {
        return;
      }

      _toDownload.Add(package);

      foreach (var set in package.DependencySets)
      {
        foreach (var dependency in set.Packages)
        {
          if (PackageExists(dependency.Id, VersionRangeToVersion(dependency.VersionRange).ToString()))
          {
            continue;
          }

          var found = FindPackage(
            dependency.Id,
            VersionRangeToVersion(dependency.VersionRange),
            _allowPreRelease,
            _allowUnlisted);

          if (found == null)
          {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Could not resolve dependency: {dependency.Id} {VersionRangeToVersion(dependency.VersionRange).ToString()}");
            Console.ResetColor();
          }
          else
          {
            if (_toDownload.Any(p => p.Title == found.Title && p.Identity.Version == found.Identity.Version))
            {
              continue;
            }

            _toDownload.Add(found);

            if (!_silent)
            {
              Console.WriteLine($"{found.Identity.Id} {found.Identity.Version}");
            }

            ResolveDependencies(found);
          }
        }
      }
    }

    private NuGetVersion VersionRangeToVersion(VersionRange versionRange)
    {
      return versionRange.MaxVersion != null ? versionRange.MaxVersion : versionRange.MinVersion;
    }

    private string GetSource()
    {
      return _source ?? DefaultSource;
    }

    private IPackageSearchMetadata FindPackage(string id, SemanticVersion version, bool includePrerelease, bool includeUnlisted)
    {
      PackageMetadataResource resource = SourceRepository.GetResource<PackageMetadataResource>();
      List<IPackageSearchMetadata> packages = resource.GetMetadataAsync(
        id,
        includePrerelease: includePrerelease,
        includeUnlisted: includeUnlisted,
        SourceCacheContext,
        NullLogger.Instance,
        CancellationToken.None).Result.ToList();
      foreach (var package in packages)
      {
        if (package.Identity.Version == version)
        {
          return package;
        }
      }

      return null;
    }

    private SourceRepository _sourceRepository;

    private SourceRepository SourceRepository => _sourceRepository ??= Repository.Factory.GetCoreV3(GetSource());

    private SourceCacheContext _sourceCacheContext;

    private SourceCacheContext SourceCacheContext => _sourceCacheContext ??= new SourceCacheContext();
  }
}