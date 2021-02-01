# nusave 💾⚡️

## Usage

nusave gives you the ability to download and save a NuGet package from nuget.org or any other source, with its dependency tree to your computer for offline use. Here is an example:

```shell
nusave -id "Newtonsoft.Json" -version "12.0.3" -outputDirectory "C:\MyLocalFeed"
```

The command above will bring packages that Newtonsoft.Json depend on, if there are any duplicates, they will be ignored. `nusave` checks for existing `.nupkg` files and for hierarchical package folders.

The combination of `nusave` and `NuGet.Server` gives you the ability to download all packages needed on your laptop or workstation for offline use.

## Installation

Check the releases page for binaries, or build `nusave.sln` .

Don't forget to add the location of `nusave.exe` or `nusave` to the `$PATH`.

.NET 5 is needed to build and run nusave.

## Download nuget packages from a `.csproj` file

```shell
nusave -msbuildProject "/path/to/project.csproj" -outputDirectory "/path/to/output/dir"
```

## JSON output

`nusave` is able to output the dependency list as JSON without downloading it:
```shell
./nusave -id "System.Collections" -version "4.3.0" -noDownload -json
```
Result:
```json
[
  {
    "id": "System.Collections",
    "version": "4.3.0",
    "authors": "Microsoft"
  },
  {
    "id": "Microsoft.NETCore.Platforms",
    "version": "1.1.0",
    "authors": "Microsoft"
  },
  {
    "id": "Microsoft.NETCore.Platforms",
    "version": "1.1.0",
    "authors": "Microsoft"
  },
  {
    "id": "Microsoft.NETCore.Targets",
    "version": "1.1.0",
    "authors": "Microsoft"
  },
  {
    "id": "Microsoft.NETCore.Targets",
    "version": "1.1.0",
    "authors": "Microsoft"
  },
  {
    "id": "System.Runtime",
    "version": "4.3.0",
    "authors": "Microsoft"
  },
  {
    "id": "System.Runtime",
    "version": "4.3.0",
    "authors": "Microsoft"
  }
]
```

Check `nusave -help` for more command line options.



