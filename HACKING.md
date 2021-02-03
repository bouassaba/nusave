## Publish

### Tag
```shell
git tag -d 3.0.0 && git push --delete origin 3.0.0
git tag 3.0.0 && git push origin --tags
```

### Create release assets

Windows:
```shell
cd ./nusave && \
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained true && \
mkdir -p ../dist && \
cd ./bin/Release/net5.0/win-x64/publish/ && \
zip -r ../../../../../../dist/nusave-3.0.0-win-x64.zip . && \
cd ../../../../../../
```

macOS:
```shell
cd ./nusave && \
dotnet publish -c Release -r osx-x64 -p:PublishSingleFile=true --self-contained true && \
mkdir -p ../dist && \
cd ./bin/Release/net5.0/osx-x64/publish/ && \
zip -r ../../../../../../dist/nusave-3.0.0-dawrin-x64.zip . && \
cd ../../../../../../
```

Linux:
```shell
cd ./nusave && \
dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=true --self-contained true && \
mkdir -p ../dist && \
cd ./bin/Release/net5.0/linux-x64/publish/ && \
zip -r ../../../../../../dist/nusave-3.0.0-linux-x64.zip . && \
cd ../../../../../../
```