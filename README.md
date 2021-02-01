# nusave 💾⚡️

## Usage

### Download NuGet package
nusave gives you the ability to download and save a NuGet package from nuget.org or any other source, with its dependency tree to your computer for offline use. Here is an example:

```shell
nusave cache package "Newtonsoft.Json@12.0.3" --targetFramework ".NETStandard,Version=v1.0" --cacheDir "C:\MyLocalFeed"
```

The command above will bring packages that Newtonsoft.Json depend on, if there are any duplicates, they will be ignored. `nusave` checks for existing `.nupkg` files and for hierarchical package folders.

The combination of `nusave` and `NuGet.Server` gives you the ability to download all packages needed on your laptop or workstation for offline use.

### Download nuget packages from a `.csproj` file

```shell
nusave cache csproj "C:\path\to\project.csproj" --cacheDir "C:\MyLocalFeed"
```

### Download nuget packages from a `.sln` file

```shell
nusave cache csproj "C:\path\to\solution.sln" --cacheDir "C:\MyLocalFeed"
```

## Installation

Check the releases page for binaries, or build `nusave.sln` .

Don't forget to add the location of `nusave.exe` or `nusave` to the `$PATH`.

.NET 5 is needed to build and run nusave.

Check `nusave -help` for more command line options.



