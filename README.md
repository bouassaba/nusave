# NuSave

## Usage

NuSave gives you the ability to download and save a nuget package from nuget.org or any other source, with all it's dependency tree to your computer for offline use. Here is an example:

```powershell
NuSave -id "Newtonsoft.Json" -version "9.0.1" -outputDirectory "C:\MyLocalFeed"
```

The command above will bring packages that Newtonsoft.Json depend on, if there are duplicates, they will be ignored. `NuSave` checks for existing `.nupkg` files and for hierarchical package folders.

The combination of `NuSave` and `NuGet.Server` gives you the ability to download all packages needed on your laptop or workstation for offline use.

## Installation

Check the releases page for binaries, or build `NuSave.sln` .

Don't forget to add the location of `NuSave.exe` to the `$PATH`.

.NET Framework 4.6.2 is needed to build and run `NuSave`.

## More

### Download nuget packages from a .csproj MSBuild project

```powershell
NuSave -msbuildProject "/path/to/project.csproj" -outputDirectory "/path/to/output/dir"
```

### Use a default proxy configuration
```powershell
NuSave -id "Newtonsoft.Json" -version "9.0.1" -outputDirectory "C:\MyLocalFeed" -useDefaultProxyConfig
```

### Pipe the JSON result to PowerShell's `Out-GridView`

`NuSave` is able to output the dependency list without downloading it, and formatting the output as JSON, that way you can pipe the content to another program that will use this information to do other tasks, this can be the case for build scripts. The following command will pipe the content to PowerShell's `Out-GridView` :

```powershell
NuSave -id "Newtonsoft.Json" -version "9.0.1" -noDownload -json | ConvertFrom-Json | Out-GridView
```

The result:

![outgridview](https://raw.githubusercontent.com/anass-b/NuSave/master/readme/outgridview.png)

Check `NuSave -help` for more command line options.



