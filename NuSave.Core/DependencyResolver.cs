namespace NuSave.Core
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading;
  using System.Xml.Linq;
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

      public string MsBuildProject { get; set; }

      public string Id { get; set; }

      public string Version { get; set; }

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

    private SourceRepository _sourceRepository;

    private SourceRepository SourceRepository => _sourceRepository ??= Repository.Factory.GetCoreV3(_options.Source);

    private SourceCacheContext _sourceCacheContext;

    private SourceCacheContext SourceCacheContext => _sourceCacheContext ??= new SourceCacheContext();

    public IEnumerable<Dependency> Resolve()
    {
      _dependencies = new List<Dependency>();

      Log("Resolving dependencies", ConsoleColor.Yellow);

      if (_options.MsBuildProject == null)
      {
        IPackageSearchMetadata package = FindPackage(_options.Id, SemanticVersion.Parse(_options.Version), _options.AllowPreRelease,
          _options.AllowUnlisted);

        if (package == null)
        {
          throw new Exception("Could not resolve package");
        }

        Resolve(package);
      }
      else
      {
        XNamespace @namespace = "http://schemas.microsoft.com/developer/msbuild/2003";
        XDocument csprojDoc = XDocument.Load(_options.MsBuildProject);

        IEnumerable<MsBuildPackageReference> references = csprojDoc
          .Element(@namespace + "Project")
          ?.Elements(@namespace + "ItemGroup")
          .Elements(@namespace + "PackageReference")
          .Select(e => new MsBuildPackageReference()
          {
            Include = e.Attribute("Include")?.Value,
            Version = e.Element(@namespace + "Version")?.Value
          });
        Resolve(references);

        IEnumerable<MsBuildPackageReference> dotnetCliToolReferences = csprojDoc
          .Element(@namespace + "Project")
          ?.Elements(@namespace + "ItemGroup")
          .Elements(@namespace + "DotNetCliToolReference")
          .Select(e => new MsBuildPackageReference()
          {
            Include = e.Attribute("Include")?.Value,
            Version = e.Element(@namespace + "Version")?.Value
          });
        Resolve(dotnetCliToolReferences);
      }

      return _dependencies;
    }

    private void Resolve(IPackageSearchMetadata package)
    {
      if (!_options.NoCache && _cache.PackageExists(package.Identity.Id, package.Identity.Version))
      {
        return;
      }

      _dependencies.Add(package.ToDependency());

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

          Log($"{found.Identity.Id} {found.Identity.Version}");

          Resolve(found);
        }
      }
    }

    private void Resolve(IEnumerable<MsBuildPackageReference> references)
    {
      foreach (var packageRef in references)
      {
        Log($"{packageRef.Include} {packageRef.Version}", ConsoleColor.Green);
        Resolve(packageRef);
      }
    }

    private void Resolve(MsBuildPackageReference msBuildPackageReference)
    {
      IPackageSearchMetadata nugetPackage = FindPackage(msBuildPackageReference.Include,
        SemanticVersion.Parse(msBuildPackageReference.Version), true, true);
      Resolve(nugetPackage);
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