# nusave 💾⚡️

## Usage

nusave gives you the ability to download and save a NuGet package from nuget.org or any other source, with its dependency tree to your computer for offline use. Here is an example:

```powershell
nusave -id "Newtonsoft.Json" -version "12.0.3" -outputDirectory "C:\MyLocalFeed"
```

The command above will bring packages that Newtonsoft.Json depend on, if there are any duplicates, they will be ignored. `nusave` checks for existing `.nupkg` files and for hierarchical package folders.

The combination of `nusave` and `NuGet.Server` gives you the ability to download all packages needed on your laptop or workstation for offline use.

## Installation

Check the releases page for binaries, or build `nusave.sln` .

Don't forget to add the location of `nusave.exe` or `nusave` to the `$PATH`.

.NET 5 is needed to build and run nusave.

## More

### Download nuget packages from a .csproj MSBuild project

```powershell
nusave -msbuildProject "/path/to/project.csproj" -outputDirectory "/path/to/output/dir"
```

### Pipe the JSON result to PowerShell's `Out-GridView`

`nusave` is able to output the dependency list without downloading it, and formatting the output as JSON, that way you can pipe the content to another program that will use this information to do other tasks, this can be the case for build scripts. The following command will pipe the content to PowerShell's `Out-GridView` :

```powershell
nusave -id "Newtonsoft.Json" -version "12.0.3" -noDownload -json | ConvertFrom-Json | Out-GridView
```

The result:

![outgridview](https://raw.githubusercontent.com/anass-b/nusave/master/readme/outgridview.png)

Check `nusave -help` for more command line options.



