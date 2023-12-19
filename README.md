# NuSave

NuSave gives you the ability to cache NuGet packages from nuget.org or
any other source with full dependency tree to your computer for offline use.

Supported platforms: Windows, macOS and Linux.

![](./docs/img/recording.gif)

## Usage

Check `nusave --help` for usage options.

### Cache a single NuGet package with dependencies

```shell
nusave cache package "Newtonsoft.Json@12.0.3" --targetFrameworks ".NETStandard@1.0,.NETStandard@1.3" --cacheDir "C:\path\to\my-local-feed"
```

The command above will bring all packages that Newtonsoft.Json depend on, and if there are
any duplicates, they will be ignored. NuSave checks for existing `.nupkg` files and
hierarchical package folders.

The combination of NuSave and NuGet.Server gives you the ability to download all
packages needed on your laptop or workstation for offline use.

### Cache multiple NuGet packages from a `.csproj` file

```shell
nusave cache csproj "C:\path\to\project.csproj" --cacheDir "C:\path\to\my-local-feed"
```

### Cache multiple NuGet packages from an `.sln` file

```shell
nusave cache sln "C:\path\to\solution.sln" --cacheDir "C:\path\to\my-local-feed"
```

### Restore and build a solution using NuSave offline cache

```shell
cd C:\path\to\my-solution
dotnet restore --packages C:\path\to\my-local-feed
dotnet build --no-restore
```

## Install

Download the latest release for your platform: https://github.com/eli-ba/nusave/releases

Add the binary to the path. Enjoy! 🎉

## Build

Build `nusave.sln` .

Don't forget to add the location of `nusave.exe` or `nusave` to the `$PATH`.

.NET 5 is needed to build and run NuSave.

## Licensing

NuSave is released under the [MIT License](LICENSE).
