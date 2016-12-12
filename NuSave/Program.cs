using Microsoft.Extensions.CommandLineUtils;
using NuSave.Core;
using System;

namespace NuSave.CLI
{
    class Program
    {
        const string Version = "1.0.0-preview.2";

        static void Main(string[] args)
        {
            CommandLineApplication app = new CommandLineApplication();

            var source = app.Option("-source", "Package source", CommandOptionType.SingleValue);
            var packageId = app.Option("-id", "Package ID", CommandOptionType.SingleValue);
            var outputDirectory = app.Option("-outputDirectory", "Output directory", CommandOptionType.SingleValue);
            var packageVersion = app.Option("-version", "Package version", CommandOptionType.SingleValue);
            var allowPreRelease = app.Option("-allowPreRelease", "Allow pre-release packages", CommandOptionType.NoValue);
            var allowUnlisted = app.Option("-allowUnlisted", "Allow unlisted packages", CommandOptionType.NoValue);
            var silent = app.Option("-silent", "Don't write anything to stdout", CommandOptionType.NoValue);
            var noDownload = app.Option("-noDownload", "Don't download packages", CommandOptionType.NoValue);
            var json = app.Option("-json", "Dependencies list will be printed in json format", CommandOptionType.NoValue);

            app.Command("version", (target) => { })
            .OnExecute(() =>
            {
                string revision = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Revision.ToString();
                Console.WriteLine($"{Version}+{revision}");
                return 0;
            });

            app.HelpOption("-? | --help | -help");

            app.OnExecute(() =>
            {
                string outputDirectoryStr = noDownload.HasValue() ? null : outputDirectory.Value();

                var downloader = new Downloader(
                    source: source.Value(),
                    outputDirectory: outputDirectoryStr,
                    id: packageId.Value(),
                    version: packageVersion.Value(),
                    allowPreRelease: allowPreRelease.HasValue(),
                    allowUnlisted: allowUnlisted.HasValue(),
                    silent: silent.HasValue(),
                    json: json.HasValue());

                downloader.ResolveDependencies();
                if (!noDownload.HasValue())
                {
                    downloader.Download();
                }

                return 0;
            });

            app.Execute(args);
        }
    }
}