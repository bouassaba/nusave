# NuSave

## Usage

NuSave gives you the ability to download and save a nuget package from nuget.org or any other source, with all it's dependency tree to your computer for offline use. Here is an example:

```bash
NuSave -id "Newtonsoft.Json" -version "9.0.1" -outputDirectory "C:\MyLocalFeed"
```

The command above will bring packages that Newtonsoft.Json depends on, if there are duplicates, they will be ignored. `NuSave` checks for existing `.nupkg` files and for hierarchical package folders.

The combination of `NuSave` and `NuGet.Server` gives you the ability to download all packages needed on your laptop or workstation for offline access.

## Installation

Check the releases page for binaries, or build `NuSave.sln` .

Don't forget to add the location of `NuSave.exe` to the `$PATH`.

.NET Framework 4.6.2 is need to build and run `NuSave`.

## Commands

`NuSave` is able to output the dependency list without downloading it, and formatting the output as JSON, that way you can pipe the content to another program that will use this information to do other tasks, this can be the case for build scripts. The following command will pipe the content to PowerShell's `Out-GridView` :

```shell
NuSave -outputDirectory "C:\MyLocalFeed" -id "Newtonsoft.Json" -version "9.0.1" -noDownload -json | ConvertFrom-Json) | Out-GridView
```

The result:

![outgridview](https://raw.githubusercontent.com/anass-b/NuSave/master/readme/outgridview.png)

#### -?

Help.

### -outputDirectory

The directory where to save the downloaded packages.

### -version

Specifies the package version that needs to be downloaded.

### -allowPreRelease

Enabled of pre-release packages.

### -allowUnlisted

Enabled unlisted packages.

### -silent

No console output.

### -json

Get a clean output with no messages "except on errors", which is JSON formatted.

### -noDownload

Don't download the packages, just get a list of packages that will be downloaded if we omit this option.



