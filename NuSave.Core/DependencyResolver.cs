namespace NuSave.Core
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Threading;
  using System.Xml;
  using Microsoft.Build.Construction;
  using Newtonsoft.Json.Linq;
  using NuGet.Common;
  using NuGet.Protocol;
  using NuGet.Protocol.Core.Types;
  using NuGet.Versioning;

  public class DependencyResolver
  {
    public class Options
    {
      public string Source { get; set; }

      public string TargetFramework { get; set; }

      public bool AllowPreRelease { get; set; }

      public bool AllowUnlisted { get; set; }

      public bool Silent { get; set; }

      public bool NoCache { get; set; }
    }

    private readonly Options _options;
    private readonly Cache _cache;
    private List<Dependency> _dependencies;

    public DependencyResolver(Options options, Cache cache)
    {
      _options = options;
      _cache = cache;
    }

    public IEnumerable<Dependency> Dependencies => _dependencies;

    private SourceRepository _sourceRepository;

    private SourceRepository SourceRepository => _sourceRepository ??= Repository.Factory.GetCoreV3(_options.Source);

    private SourceCacheContext _sourceCacheContext;

    private SourceCacheContext SourceCacheContext => _sourceCacheContext ??= new SourceCacheContext();

    public void ResolveByIdAndVersion(string id, string version)
    {
      Log($"Resolving dependencies for {id}@{version} 🪄️", ConsoleColor.Yellow);

      _dependencies = new List<Dependency>();
      
      IPackageSearchMetadata package = FindPackage(id, SemanticVersion.Parse(version), _options.AllowPreRelease,
        _options.AllowUnlisted);

      if (package == null)
      {
        throw new Exception("Could not resolve package");
      }

      AppendFromPackageSearchMetadata(package);
    }

    public void ResolveBySln(string path)
    {
      Log($"Resolving dependencies using {path} ⚡️", ConsoleColor.Yellow);
      
      _dependencies = new List<Dependency>();

      var solutionFile = SolutionFile.Parse(path);
      foreach (var project in solutionFile.ProjectsInOrder)
      {
        if (!File.Exists(project.AbsolutePath))
        {
          continue;
        }

        ResolveByCsProj(project.AbsolutePath);
      }
    }

    public void ResolveByCsProj(string path)
    {
      Log($"Resolving dependencies using {path} ⚡️", ConsoleColor.Yellow);
      
      _dependencies = new List<Dependency>();

      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.Load(path);
      var nodes = xmlDocument.SelectNodes("Project/ItemGroup/PackageReference");
      if (nodes == null)
      {
        return;
      }

      List<PackageReference> references = new List<PackageReference>();
      foreach (var node in nodes)
      {
        var json = JObject.Parse(node.ToJson());
        references.Add(new PackageReference
        {
          Include = json["PackageReference"]?["@Include"]?.ToString(),
          Version = json["PackageReference"]?["@Version"]?.ToString()
        });
      }

      AppendFromPackageReferences(references);
    }

    private void AppendFromPackageSearchMetadata(IPackageSearchMetadata package)
    {
      if (!_options.NoCache && _cache.PackageExists(package.Identity.Id, package.Identity.Version))
      {
        return;
      }

      _dependencies.Add(package.ToDependency());
      
      Log($"{package.Identity.Id}@{package.Identity.Version}");

      foreach (var set in package.DependencySets)
      {
        string targetFramework = set.TargetFramework.ToString();
        if (!string.IsNullOrWhiteSpace(_options.TargetFramework) &&
            !targetFramework.ToLowerInvariant().Contains(_options.TargetFramework.ToLowerInvariant()))
        {
          continue;
        }

        foreach (var dependency in set.Packages)
        {
          if (!_options.NoCache && _cache.PackageExists(dependency.Id, dependency.VersionRange.ToNuGetVersion()))
          {
            continue;
          }

          if (_dependencies.Any(p => p.Id == dependency.Id && p.Version == dependency.VersionRange.ToNuGetVersion()))
          {
            continue;
          }

          var found = FindPackage(
            dependency.Id,
            dependency.VersionRange.ToNuGetVersion(),
            _options.AllowPreRelease,
            _options.AllowUnlisted);
          if (found == null)
          {
            Log($"Could not resolve dependency: {dependency.Id} {dependency.VersionRange.ToNuGetVersion().ToString()}", ConsoleColor.Red);
            continue;
          }

          _dependencies.Add(found.ToDependency());

          Log($"{found.Identity.Id}@{found.Identity.Version}");

          AppendFromPackageSearchMetadata(found);
        }
      }
    }

    private void AppendFromPackageReference(PackageReference packageReference)
    {
      IPackageSearchMetadata nugetPackage = FindPackage(packageReference.Include,
        SemanticVersion.Parse(packageReference.Version), true, true);
      AppendFromPackageSearchMetadata(nugetPackage);
    }

    private void AppendFromPackageReferences(IEnumerable<PackageReference> references)
    {
      foreach (var packageRef in references)
      {
        AppendFromPackageReference(packageRef);
      }
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

    private void Log(string message)
    {
      if (_options.Silent)
      {
        return;
      }

      Console.WriteLine(message);
    }

    private void Log(string message, ConsoleColor consoleColor)
    {
      if (_options.Silent)
      {
        return;
      }

      Console.ForegroundColor = consoleColor;
      Console.WriteLine(message);
      Console.ResetColor();
    }
  }
}