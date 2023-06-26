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
      public List<string> Sources { get; set; }

      public List<string> TargetFrameworks { get; set; }

      public bool AllowPreRelease { get; set; }

      public bool AllowUnlisted { get; set; }
    }

    private readonly Options _options;
    private readonly Cache _cache;
    private readonly List<Dependency> _dependencies = new();
    private readonly HashSet<string> _sources = new() {"https://api.nuget.org/v3/index.json"};
    private readonly List<SourceRepository> _sourceRepositories = new();
    private readonly SourceCacheContext _sourceCacheContext = new();

    public DependencyResolver(Options options, Cache cache)
    {
      _options = options;
      _cache = cache;

      foreach (var source in options.Sources)
      {
        _sources.Add(source);
      }

      foreach (var source in _sources)
      {
        _sourceRepositories.Add(Repository.Factory.GetCoreV3(source));
      }
    }

    public IEnumerable<Dependency> Dependencies => _dependencies;

    public void ResolveByIdAndVersion(string id, string version)
    {
      Log($"Resolving dependencies for {id}@{version} 🪄️", ConsoleColor.Yellow);

      (IPackageSearchMetadata package, SourceRepository sourceRepository) = FindPackage(
        id,
        NuGetVersion.Parse(version),
        _options.AllowPreRelease,
        _options.AllowUnlisted);
      Append(package, sourceRepository);
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

      var additionalSources = new HashSet<string>();

      var restoreAdditionalProjectSourcesXmlNode = xmlDocument.SelectSingleNode("Project/PropertyGroup/RestoreAdditionalProjectSources");
      if (restoreAdditionalProjectSourcesXmlNode?.InnerText != null)
      {
        var sources = restoreAdditionalProjectSourcesXmlNode.InnerText.Split(";").ToList();
        sources = sources.Select(e => e.Trim()).Where(e => e.Length > 0).ToList();
        foreach (var source in sources)
        {
          additionalSources.Add(source);
        }
      }

      var restoreSourcesXmlNode = xmlDocument.SelectSingleNode("Project/PropertyGroup/RestoreSources");
      if (restoreSourcesXmlNode?.InnerText != null)
      {
        var sources = restoreSourcesXmlNode.InnerText.Split(";").ToList();
        sources = sources.Select(e => e.Trim()).Where(e => e.Length > 0).ToList();
        foreach (var source in sources)
        {
          additionalSources.Add(source);
        }
      }

      foreach (var source in additionalSources)
      {
        if (_sources.Any(e => e == source)) continue;
        _sources.Add(source);
        _sourceRepositories.Add(Repository.Factory.GetCoreV3(source));
      }

      var packageReferenceXmlNodes = xmlDocument.SelectNodes("Project/ItemGroup/PackageReference");
      if (packageReferenceXmlNodes == null)
      {
        return;
      }

      List<MsBuildPackageReference> references = new List<MsBuildPackageReference>();
      foreach (var node in packageReferenceXmlNodes)
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
        (IPackageSearchMetadata package, SourceRepository sourceRepository) = FindPackage(
          reference.Include,
          NuGetVersion.Parse(reference.Version),
          true,
          true);
        Append(package, sourceRepository);
      }
    }

    private void Append(IPackageSearchMetadata package, SourceRepository sourceRepository)
    {
      if (!_dependencies.Any(e => e.Id == package.Identity.Id && e.Version == package.Identity.Version))
      {
        _dependencies.Add(package.ToDependency(sourceRepository));
      }

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
          var minVersion = dependency.VersionRange.MinVersion;
          (IPackageSearchMetadata dependencyPackage, SourceRepository dependencySourceRepository) = FindPackage(
            dependency.Id,
            minVersion,
            _options.AllowPreRelease,
            _options.AllowUnlisted);
          Append(dependencyPackage, dependencySourceRepository);
        }
      }
    }

    private static string TranslateTargetFrameworkSyntax(string localSyntax)
    {
      var split = localSyntax.Split("@");
      string value = $"{split[0]},Version=v{split[1]}";
      return value;
    }

    private (IPackageSearchMetadata, SourceRepository) FindPackage(string id, NuGetVersion version, bool includePrerelease, bool includeUnlisted)
    {
      foreach (var sourceRepository in _sourceRepositories)
      {
        try
        {
          PackageMetadataResource resource = sourceRepository.GetResource<PackageMetadataResource>();
          List<IPackageSearchMetadata> packages = resource.GetMetadataAsync(
            id,
            includePrerelease,
            includeUnlisted,
            _sourceCacheContext,
            NullLogger.Instance,
            CancellationToken.None).Result.ToList();
          foreach (var package in packages)
          {
            if (package.Identity.Version == version)
            {
              return (package, sourceRepository);
            }
          }
        }
        catch
        {
          // ignored
        }
      }

      throw new Exception($"Could not find package {id}@{version.OriginalVersion}");
    }

    private static void Log(string message, ConsoleColor consoleColor)
    {
      Console.ForegroundColor = consoleColor;
      Console.WriteLine(message);
      Console.ResetColor();
    }
  }
}