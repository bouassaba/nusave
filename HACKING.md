### Generate executable
macOS:
```shell
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true -p:PublishReadyToRun=true --self-contained false
dotnet publish -c Release -r osx-x64 -p:PublishSingleFile=true -p:PublishReadyToRun=true --self-contained false
dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=true -p:PublishReadyToRun=true --self-contained false
```