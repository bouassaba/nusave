namespace NuSave.Core
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Net;
  using System.Threading;
  using NuGet.Common;
  using NuGet.Protocol;
  using NuGet.Protocol.Core.Types;

  public class Downloader
  {
    private readonly Options _options;
    private readonly DependencyResolver _dependencyResolver;
    private readonly Cache _cache;

    public class Options
    {
      public string Source { get; set; }

      public bool Silent { get; set; }
    }

    public Downloader(Options options, DependencyResolver dependencyResolver, Cache cache)
    {
      _options = options;
      _dependencyResolver = dependencyResolver;
      _cache = cache;
    }

    private SourceRepository _sourceRepository;

    private SourceRepository SourceRepository => _sourceRepository ??= Repository.Factory.GetCoreV3(_options.Source);

    private SourceCacheContext _sourceCacheContext;

    private SourceCacheContext SourceCacheContext => _sourceCacheContext ??= new SourceCacheContext();

    public void Download()
    {
      Log("Downloading ⚡️", ConsoleColor.Yellow);

      foreach (var dependency in _dependencyResolver.Dependencies)
      {
        if (_cache.PackageExists(dependency.Id, dependency.Version))
        {
          continue;
        }

        Log($"{dependency.Id} {dependency.Version}");

        // We keep retrying forever until the user will press Ctrl-C
        // This lets the user decide when to stop retrying.
        // The reason for this is that building the dependencies list is expensive
        // on slow internet connection, when the CLI crashes because of a WebException
        // the user has to wait for the dependencies list to build rebuild again.
        while (true)
        {
          try
          {
            string nugetPackageOutputPath = _cache.GetNuGetPackagePath(dependency.Id, dependency.Version);
            using FileStream packageStream = File.Create(nugetPackageOutputPath);

            FindPackageByIdResource resource = SourceRepository.GetResource<FindPackageByIdResource>();
            resource.CopyNupkgToStreamAsync(
              dependency.Id,
              dependency.Version,
              packageStream,
              SourceCacheContext,
              NullLogger.Instance,
              CancellationToken.None).Wait();

            break;
          }
          catch (WebException e)
          {
            Log($"{e.Message}. Retrying in one second...", ConsoleColor.Red);
            Thread.Sleep(1000);
          }
        }
      }
      
      Log("Done 🎉", ConsoleColor.Yellow);
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