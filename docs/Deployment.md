## Publish

### Tag
```shell
git tag -d v1.2.3 && git push --delete origin v1.2.3
git tag v1.2.3 && git push origin --tags
```

### Build release artifacts

Windows:
```shell
./scripts/deploy.sh win-x64 
```

macOS:
```shell
./scripts/deploy.sh osx-x64 
```

Linux:
```shell
./scripts/deploy.sh linux-x64
```