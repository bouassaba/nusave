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

      public List<string> TargetFrameworks { get; set; }

      public bool AllowPreRelease { get; set; }

      public bool AllowUnlisted { get; set; }
    }

    private readonly Options _options;
    private readonly Cache _cache;
    private List<Dependency> _dependencies = new();

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

      IPackageSearchMetadata package = FindPackage(
        id,
        SemanticVersion.Parse(version),
        _options.AllowPreRelease,
        _options.AllowUnlisted);

      if (package == null)
      {
        throw new Exception("Could not resolve package");
      }

      Append(package);
    }

    public void ResolveBySln(string path)
    {
      Log($"Resolving dependencies using {path} 🪄️", ConsoleColor.Yellow);

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
      Log($"Resolving dependencies using {path} 🪄️", ConsoleColor.Yellow);

      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.Load(path);
      var nodes = xmlDocument.SelectNodes("Project/ItemGroup/PackageReference");
      if (nodes == null)
      {
        return;
      }

      List<MsBuildPackageReference> references = new List<MsBuildPackageReference>();
      foreach (var node in nodes)
      {
        var json = JObject.Parse(node.ToJson());
        references.Add(new MsBuildPackageReference
        {
          Include = json["PackageReference"]?["@Include"]?.ToString(),
          Version = json["PackageReference"]?["@Version"]?.ToString()
        });
      }

      foreach (var reference in references)
      {
        IPackageSearchMetadata package = FindPackage(
          reference.Include,
          SemanticVersion.Parse(reference.Version),
          true,
          true);
        Append(package);
      }
    }

    private void Append(IPackageSearchMetadata package)
    {
      if (_cache.PackageExists(package.Identity.Id, package.Identity.Version))
      {
        return;
      }

      if (_dependencies.Any(e => e.Id == package.Identity.Id && e.Version == package.Identity.Version))
      {
        return;
      }

      _dependencies.Add(package.ToDependency());

      foreach (var set in package.DependencySets)
      {
        string setTargetFramework = set.TargetFramework.ToString();
        if (_options.TargetFrameworks.Any() &&
            !_options.TargetFrameworks.Any(e =>
              setTargetFramework.ToLowerInvariant().Equals(TranslateTargetFrameworkSyntax(e).ToLowerInvariant())))
        {
          continue;
        }

        foreach (var dependency in set.Packages)
        {
          if (_cache.PackageExists(dependency.Id, dependency.VersionRange.ToNuGetVersion()))
          {
            return;
          }

          if (_dependencies.Any(e => e.Id == dependency.Id && e.Version == dependency.VersionRange.ToNuGetVersion()))
          {
            return;
          }

          var found = FindPackage(
            dependency.Id,
            dependency.VersionRange.ToNuGetVersion(),
            _options.AllowPreRelease,
            _options.AllowUnlisted);
          if (found == null)
          {
            continue;
          }

          _dependencies.Add(found.ToDependency());

          Append(found);
        }
      }
    }

    private static string TranslateTargetFrameworkSyntax(string localSyntax)
    {
      var split = localSyntax.Split("@");
      string value = $"{split[0]},Version=v{split[1]}";
      return value;
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

    private static void Log(string message, ConsoleColor consoleColor)
    {
      Console.ForegroundColor = consoleColor;
      Console.WriteLine(message);
      Console.ResetColor();
    }
  }
}