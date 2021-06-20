namespace NuSave
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Core;
  using Microsoft.Extensions.CommandLineUtils;

  internal class Program
  {
    private const string Version = "3.0.0";

    private static void Main(string[] args)
    {
      CommandLineApplication app = new CommandLineApplication();

      CacheCommand(app);

      app.HelpOption("--help | -h");
      app.VersionOption("--version | -v", () => Version);

      try
      {
        app.Execute(args);
      }
      catch (Exception e)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(e.Message);
        Console.ResetColor();
      }
    }

    private static void CacheCommand(CommandLineApplication app)
    {
      app.Command("cache", target =>
      {
        SlnCommand(target);
        CsprojCommand(target);
        PackageCommand(target);
      });
    }

    private static void SlnCommand(CommandLineApplication target)
    {
      CommandArgument slnPath = null;
      CommandOption cacheDirectory = null;
      CommandOption sources = null;
      CommandOption targetFrameworks = null;
      CommandOption allowPreRelease = null;
      CommandOption allowUnlisted = null;
      CommandOption silent = null;

      target.Command("sln", sln =>
      {
        slnPath = sln.Argument("path", "Solution file (.sln)");
        sources = sln.Option("--source", "Additional sources, comma separated", CommandOptionType.SingleValue);
        targetFrameworks = sln.Option("--targetFrameworks",
          "Target Frameworks (comma separated), example: .NETStandard@1.3,.NETFramework@4.5",
          CommandOptionType.SingleValue);
        cacheDirectory = sln.Option("--cacheDir", "Cache directory", CommandOptionType.SingleValue);
        allowPreRelease = sln.Option("--allowPreRelease", "Allow pre-release packages", CommandOptionType.NoValue);
        allowUnlisted = sln.Option("--allowUnlisted", "Allow unlisted packages", CommandOptionType.NoValue);
        silent = sln.Option("--silent", "Don't write anything to stdout", CommandOptionType.NoValue);
      }).OnExecute(() =>
      {
        var cache = new Cache(cacheDirectory.Value());

        Log($"Using cache directory: {cache.Directory}", ConsoleColor.Cyan);

        var dependencyResolver = new DependencyResolver(new DependencyResolver.Options
        {
          Sources = sources.Value() != null ? sources.Value().Split(",").ToList() : new List<string>(),
          TargetFrameworks = targetFrameworks.Value() != null ? targetFrameworks.Value().Split(",").ToList() : new List<string>(),
          AllowPreRelease = allowPreRelease.HasValue(),
          AllowUnlisted = allowUnlisted.HasValue()
        }, cache);
        dependencyResolver.ResolveBySln(slnPath.Value);

        var downloader = new Downloader(new Downloader.Options
        {
          Silent = silent.HasValue(),
        }, dependencyResolver, cache);
        downloader.Download();

        return 0;
      });
    }

    private static void CsprojCommand(CommandLineApplication target)
    {
      CommandArgument csprojPath = null;
      CommandOption cacheDirectory = null;
      CommandOption sources = null;
      CommandOption targetFrameworks = null;
      CommandOption allowPreRelease = null;
      CommandOption allowUnlisted = null;

      target.Command("csproj", csproj =>
      {
        csprojPath = csproj.Argument("path", "Project file (.csproj)");
        sources = csproj.Option("--source", "Additional sources, comma separated", CommandOptionType.SingleValue);
        targetFrameworks = csproj.Option("--targetFrameworks",
          "Target Frameworks (comma separated), example: .NETStandard@1.3,.NETFramework@4.5",
          CommandOptionType.SingleValue);
        cacheDirectory = csproj.Option("--cacheDir", "Cache directory", CommandOptionType.SingleValue);
        allowPreRelease = csproj.Option("--allowPreRelease", "Allow pre-release packages", CommandOptionType.NoValue);
        allowUnlisted = csproj.Option("--allowUnlisted", "Allow unlisted packages", CommandOptionType.NoValue);
      }).OnExecute(() =>
      {
        var cache = new Cache(cacheDirectory.Value());

        Log($"Using cache directory: {cache.Directory}", ConsoleColor.Cyan);

        var dependencyResolver = new DependencyResolver(new DependencyResolver.Options
        {
          Sources = sources.Value() != null ? sources.Value().Split(",").ToList() : new List<string>(),
          TargetFrameworks = targetFrameworks.Value() != null ? targetFrameworks.Value().Split(",").ToList() : new List<string>(),
          AllowPreRelease = allowPreRelease.HasValue(),
          AllowUnlisted = allowUnlisted.HasValue()
        }, cache);
        dependencyResolver.ResolveByCsProj(csprojPath.Value);

        var downloader = new Downloader(new Downloader.Options
        {
        }, dependencyResolver, cache);
        downloader.Download();

        return 0;
      });
    }

    private static void PackageCommand(CommandLineApplication target)
    {
      CommandArgument idAndVersion = null;
      CommandOption cacheDirectory = null;
      CommandOption sources = null;
      CommandOption targetFrameworks = null;
      CommandOption allowPreRelease = null;
      CommandOption allowUnlisted = null;
      CommandOption silent = null;

      target.Command("package", package =>
      {
        idAndVersion = package.Argument("id@version", "Package ID and Version, example: System.Collections@4.3.0");
        sources = package.Option("--source", "Additional sources, comma separated", CommandOptionType.SingleValue);
        targetFrameworks = package.Option("--targetFrameworks",
          "Target Frameworks (comma separated), example: .NETStandard@1.3,.NETFramework@4.5",
          CommandOptionType.SingleValue);
        cacheDirectory = package.Option("--cacheDir", "Cache directory", CommandOptionType.SingleValue);
        allowPreRelease = package.Option("--allowPreRelease", "Allow pre-release packages", CommandOptionType.NoValue);
        allowUnlisted = package.Option("--allowUnlisted", "Allow unlisted packages", CommandOptionType.NoValue);
        silent = package.Option("--silent", "Don't write anything to stdout", CommandOptionType.NoValue);
      }).OnExecute(() =>
      {
        var cache = new Cache(cacheDirectory.Value());

        Log($"Using cache directory: {cache.Directory}", ConsoleColor.Cyan);

        var dependencyResolver = new DependencyResolver(new DependencyResolver.Options
        {
          Sources = sources.Value() != null ? sources.Value().Split(",").ToList() : new List<string>(),
          TargetFrameworks = targetFrameworks.Value() != null ? targetFrameworks.Value().Split(",").ToList() : new List<string>(),
          AllowPreRelease = allowPreRelease.HasValue(),
          AllowUnlisted = allowUnlisted.HasValue()
        }, cache);
        dependencyResolver.ResolveByIdAndVersion(idAndVersion.Value.Split("@")[0],
          idAndVersion.Value.Split("@")[1]);

        var downloader = new Downloader(new Downloader.Options
        {
          Silent = silent.HasValue(),
        }, dependencyResolver, cache);
        downloader.Download();

        return 0;
      });
    }

    private static void Log(string message, ConsoleColor consoleColor)
    {
      Console.ForegroundColor = consoleColor;
      Console.WriteLine(message);
      Console.ResetColor();
    }
  }
}