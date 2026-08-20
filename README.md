# ediFabric Native API for ASP .NET Core

## 1. Overview
This example hosts [ediFabric Native](https://www.edifabric.com/edifabric-native.html) behind an ASP .NET Core API. It translates X12 EDI to JSON and back, validates transaction sets, and generates acknowledgments using the C# bindings from [edifabric-csharp-bindings](https://github.com/EdiFabric/edifabric-csharp-bindings).

The native library is a self-contained shared library. No `EdiFabric.Api` NuGet package (aka EdiNation InHouse) is required on the target machine beyond this ASP .NET host.

## 2. Requirements
- [Visual Studio](https://visualstudio.microsoft.com/vs/) or the .NET 10 SDK.
- [Download Postman](https://www.postman.com/downloads/) - it's an application to consume/test your API.
- The native library for your platform:

| Platform | File |
| --- | --- |
| Windows | `edifabric-x12-tools.dll` |
| Linux | `edifabric-x12-tools.so` |
| macOS | `edifabric-x12-tools.dylib` |

[Download **ediFabric Native** Library](https://support.edifabric.com/hc/en-us/articles/37289848931869-Download)

Put the library in the repository root or in `EdiFabric.Api.ASPNET`, or set `LibraryPath` in `appsettings.json` / `EDIFABRIC_X12_LIB`. The project copies it next to the executable on build when it is found in those folders.

- X12 test file(s). If you don't have a test file, use one of ours - [X12 HIPAA](https://support.edifabric.com/hc/en-us/sections/360001487352-X12-HIPAA-Files-Templates), [X12](https://support.edifabric.com/hc/en-us/sections/360005274077-X12-Files-Templates).

## 3. License
Set `ApiKey` in `appsettings.json` to your serial. The free-plan serial is:

```
bd96a836feca45cb91c86ee65d281f52
```

The free plan authorizes with `set_serial` only. Tokens (`TokenFileCache`) are available for the Enterprise license.

## 4. Setup
Rebuild the solution. If there are any build errors, contact us at https://support.edifabric.com/hc/en-us/requests/new for assistance.

The C# bindings live in `EdiFabric.Api.ASPNET/Native` (`NativeMethods.cs` and `EdiFabricX12.cs`), copied from the [edifabric-csharp-bindings](https://github.com/EdiFabric/edifabric-csharp-bindings) repository.

By default the API uses the online spec service (`SetMap` with `"default": "<serial>"`). To use local JSON models instead, place a `map/map.json` next to the web project (see the bindings README for the map format).

## 5. Getting started
Run the project and open Swagger at `/swagger`. POST X12 EDI to:

| Endpoint | Native call |
| --- | --- |
| `POST /x12/read` | `EdiFabricX12.Parse` (JSON only) | Input is X12 file | Output is JSON |
| `POST /x12/write` | `EdiFabricX12.Build` | Input is JSON (the output from /read) | Output is X12 |
| `POST /x12/validate` | `EdiFabricX12.Parse` (JSON + validation report) | Input is JSON (the output from /read) or X12 | Output is JSON |
| `POST /x12/ack` | `EdiFabricX12.Parse` (JSON + validation + 999/997/TA1) | Input is JSON (the output from /read) or X12 | Output is JSON |

## 6. Warranty
*The source code in these example projects is strictly for demonstrational purposes and is provided "AS IS" without warranty of any kind, whether expressed or implied, including but not limited to the implied warranties of merchantability and/or fitness for a particular purpose.*

## 7. Additional information

[ediFabric Native documentation](https://support.edifabric.com/hc/en-us/articles/37276016388125-Introduction)

[C# bindings](https://github.com/EdiFabric/edifabric-csharp-bindings)

[Support](https://support.edifabric.com/hc/en-us/requests/new)
### 2026 © EdiFabric
