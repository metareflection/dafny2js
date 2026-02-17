# Publishing dafny2js

Self-contained binaries that don't require the .NET runtime.

## Releasing

Push a version tag to trigger CI:

```bash
git tag v0.2.0
git push origin v0.2.0
```

GitHub Actions builds for `osx-arm64`, `osx-x64`, `linux-x64`, and `linux-arm64`, then attaches the tarballs to a GitHub release.

## Local build

```bash
dotnet publish -c Release -r osx-arm64 --self-contained /p:PublishSingleFile=true
```

Output: `bin/Release/net8.0/osx-arm64/publish/dafny2js`

RIDs: `osx-arm64`, `osx-x64`, `linux-x64`, `linux-arm64`, `win-x64`.

## Notes

- ~90 MB binary (bundles the .NET 8 runtime).
- `../dafny` source tree required at build time, not at runtime.
