# Publishing dafny2js as a standalone binary

dafny2js can be published as a self-contained single-file binary that does not require the .NET SDK or runtime on the target machine.

## Quick build

```bash
dotnet publish -c Release -r osx-arm64 --self-contained /p:PublishSingleFile=true
```

The binary will be at:

```
bin/Release/net8.0/osx-arm64/publish/dafny2js
```

## Platform targets

Replace `osx-arm64` with the appropriate runtime identifier:

| Platform              | RID            |
| --------------------- | -------------- |
| macOS Apple Silicon   | `osx-arm64`    |
| macOS Intel           | `osx-x64`      |
| Linux x64             | `linux-x64`    |
| Linux ARM64           | `linux-arm64`  |
| Windows x64           | `win-x64`      |

## Notes

- The binary is ~90 MB because it bundles the entire .NET 8 runtime.
- A few `IL3000` warnings about `Assembly.Location` come from Dafny's backend code and are harmless — dafny2js only uses the parser/AST, not the compilation backends.
- The `../dafny` source tree must be present at build time (project references to DafnyCore and DafnyDriver), but is **not** needed at runtime.
