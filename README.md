# textile

.NET DLL / EXE decompiler with a clean WPF GUI.

Built on [ICSharpCode.Decompiler](https://github.com/icsharpcode/ILSpy) and [ICSharpCode.BamlDecompiler](https://github.com/icsharpcode/ILSpy).

## Features

- Decompile .NET assemblies (`.dll` / `.exe`) to C# source.
- Extract and decompile WPF `.baml` resources to `.xaml`.
- Drag & drop support.
- Light / dark theme.
- Internal console with colored log output.

## Build

Requires .NET 10 SDK.

```
dotnet build textile.csproj -c Release -p:Platform=x64
```

## License

GNU General Public License v3.0. See [LICENSE](LICENSE).