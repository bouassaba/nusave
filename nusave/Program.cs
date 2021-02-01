namespace NuSave
{
  using System;
  using Core;
  using Microsoft.Extensions.CommandLineUtils;
  using Newtonsoft.Json;

  internal class Program
  {
    private const string Version = "3.0.0";
    private const string DefaultSource = "https://api.nuget.org/v3/index.json";

    private static void Main(string[] args)
    {
      CommandLineApplication app = new CommandLineApplication();

      Cache(app);
      Dependencies(app);

      app.HelpOption("--help | -h");
      app.VersionOption("--version | -v", () => Version);

      app.Execute(args);
    }

    private static void Dependencies(CommandLineApplication app)
    {
      CommandOption msbuildProject = null;
      CommandOption source = null;
      CommandOption targetFramework = null;
      CommandOption id = null;
      CommandOption version = null;
      CommandOption allowPreRelease = null;
      CommandOption allowUnlisted = null;
      CommandOption silent = null;

      app.Command("dependencies", target =>
        {
          msbuildProject = target.Option("--msbuildProject", "MSBuild Project file", CommandOptionType.SingleValue);
          source = target.Option("--source", "Package source", CommandOptionType.SingleValue);
          targetFramework = target.Option("--targetFramework",
            "Target Framework, example: .NETStandard,Version=1.3, .NETFramework,Version=4.5",
            CommandOptionType.SingleValue);
          id = target.Option("--id", "Package ID", CommandOptionType.SingleValue);
          version = target.Option("--version", "Package version", CommandOptionType.SingleValue);
          allowPreRelease = target.Option("--allowPreRelease", "Allow pre-release packages", CommandOptionType.NoValue);
          allowUnlisted = target.Option("--allowUnlisted", "Allow unlisted packages", CommandOptionType.NoValue);
          silent = target.Option("--silent", "Don't write anything to stdout", CommandOptionType.NoValue);
        })
        .OnExecute(() =>
        {
          var dependencyResolver = new DependencyResolver(new DependencyResolver.Options
          {
            Source = source.Value() ?? DefaultSource,
            TargetFramework = targetFramework.Value(),
            MsBuildProject = msbuildProject.Value(),
            Id = id.Value(),
            Version = version.Value(),
            AllowPreRelease = allowPreRelease.HasValue(),
            AllowUnlisted = allowUnlisted.HasValue(),
            Silent = true,
            NoCache = true
          }, null);
          Console.WriteLine(JsonConvert.SerializeObject(dependencyResolver.Resolve(), Formatting.Indented));

          return 0;
        });
    }

    private static void Cache(CommandLineApplication app)
    {
      CommandOption cacheDirectory = null;
      CommandOption msbuildProject = null;
      CommandOption source = null;
      CommandOption targetFramework = null;
      CommandOption id = null;
      CommandOption version = null;
      CommandOption allowPreRelease = null;
      CommandOption allowUnlisted = null;
      CommandOption silent = null;

      app.Command("cache", target =>
        {
          msbuildProject = target.Option("--msbuildProject", "MSBuild Project file", CommandOptionType.SingleValue);
          source = target.Option("--source", "Package source", CommandOptionType.SingleValue);
          targetFramework = target.Option("--targetFramework",
            "Target Framework, example: .NETStandard,Version=1.3, .NETFramework,Version=4.5",
            CommandOptionType.SingleValue);
          id = target.Option("--id", "Package ID", CommandOptionType.SingleValue);
          cacheDirectory = target.Option("--cacheDir", "Cache directory", CommandOptionType.SingleValue);
          version = target.Option("--version", "Package version", CommandOptionType.SingleValue);
          allowPreRelease = target.Option("--allowPreRelease", "Allow pre-release packages", CommandOptionType.NoValue);
          allowUnlisted = target.Option("--allowUnlisted", "Allow unlisted packages", CommandOptionType.NoValue);
          silent = target.Option("--silent", "Don't write anything to stdout", CommandOptionType.NoValue);
        })
        .OnExecute(() =>
        {
          var cache = new Cache(new Cache.Options
          {
            Directory = cacheDirectory.Value()
          });

          Log($"Using cache directory: {cache.EnsureDirectory()}", ConsoleColor.Cyan);

          var dependencyResolver = new DependencyResolver(new DependencyResolver.Options
          {
            Source = source.Value() ?? DefaultSource,
            TargetFramework = targetFramework.Value(),
            MsBuildProject = msbuildProject.Value(),
            Id = id.Value(),
            Version = version.Value(),
            AllowPreRelease = allowPreRelease.HasValue(),
            AllowUnlisted = allowUnlisted.HasValue(),
            Silent = silent.HasValue()
          }, cache);
          var downloader = new Downloader(new Downloader.Options
          {
            Silent = silent.HasValue(),
            Source = source.Value() ?? DefaultSource
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