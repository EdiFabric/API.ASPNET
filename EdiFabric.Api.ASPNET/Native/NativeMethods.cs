using System.Runtime.InteropServices;

namespace EdiFabric.Native.X12;

/// <summary>
/// One-to-one P/Invoke declarations for c-abi-edifabric_x12_tools.h.
/// Use <see cref="EdiFabricX12"/> instead of calling these directly.
/// </summary>
internal static unsafe class NativeMethods
{
    internal const string LibraryName = "edifabric-x12-tools";

    static NativeMethods()
    {
        NativeLibrary.SetDllImportResolver(typeof(NativeMethods).Assembly, Resolve);
    }

    /// <summary>Forces the static constructor to run before the first P/Invoke.</summary>
    internal static void EnsureResolverRegistered()
    {
    }

    internal static string PlatformFileName =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "edifabric-x12-tools.dll"
        : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "edifabric-x12-tools.dylib"
        : "edifabric-x12-tools.so";

    private static IntPtr Resolve(string libraryName, System.Reflection.Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName != LibraryName)
            return IntPtr.Zero;

        foreach (var candidate in CandidatePaths())
        {
            if (!File.Exists(candidate) || !NativeLibrary.TryLoad(candidate, out var handle))
                continue;

            EdiFabricX12.ResolvedLibraryPath = Path.GetFullPath(candidate);
            return handle;
        }

        // Fall back to the default probing logic.
        return IntPtr.Zero;
    }

    private static IEnumerable<string> CandidatePaths()
    {
        var fileName = PlatformFileName;

        foreach (var hint in new[] { EdiFabricX12.LibraryPath, Environment.GetEnvironmentVariable("EDIFABRIC_X12_LIB") })
        {
            if (string.IsNullOrWhiteSpace(hint))
                continue;

            yield return Directory.Exists(hint) ? Path.Combine(hint, fileName) : hint;
        }

        var baseDirectory = AppContext.BaseDirectory;
        yield return Path.Combine(baseDirectory, fileName);

        // Walk up a few levels so the example also runs straight from bin/Debug/net10.0.
        var directory = new DirectoryInfo(baseDirectory);
        for (var level = 0; level < 6 && directory?.Parent is not null; level++)
        {
            directory = directory.Parent;
            yield return Path.Combine(directory.FullName, fileName);
        }
    }

    /* Lifecycle / logging */

    [DllImport(LibraryName, EntryPoint = "init_logger", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int InitLogger(byte* pathUtf8, int pathLength, int minLevel);

    [DllImport(LibraryName, EntryPoint = "shutdown_logger", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int ShutdownLogger();

    [DllImport(LibraryName, EntryPoint = "clear_cache", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int ClearCache();

    /* Licensing */

    [DllImport(LibraryName, EntryPoint = "install_license", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int InstallLicense(byte* serial, int serialLength);

    [DllImport(LibraryName, EntryPoint = "get_app_version", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int GetAppVersion(int* appVersion);

    [DllImport(LibraryName, EntryPoint = "get_token", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int GetToken(byte* serial, int serialLength, byte* output, int outputCapacity, int* outputLength);

    [DllImport(LibraryName, EntryPoint = "validate_token", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int ValidateToken(byte* token, int tokenLength);

    [DllImport(LibraryName, EntryPoint = "set_token", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int SetToken(byte* token, int tokenLength);

    [DllImport(LibraryName, EntryPoint = "get_token_expiration", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int GetTokenExpiration(long* expirationUtc);

    [DllImport(LibraryName, EntryPoint = "set_serial", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int SetSerial(byte* serial, int serialLength);

    /* Model map */

    [DllImport(LibraryName, EntryPoint = "set_map", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int SetMap(byte* map, int mapLength);

    /* Parse / split / build / merge */

    [DllImport(LibraryName, EntryPoint = "parse", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int Parse(
        byte* input, int inputLength,
        int mode,
        byte* config, int configLength,
        byte* output, int outputCapacity,
        int* outputLength, int* outputOffset);

    [DllImport(LibraryName, EntryPoint = "start_split", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int StartSplit(byte* input, int inputLength, int mode, byte* config, int configLength);

    [DllImport(LibraryName, EntryPoint = "split", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int Split(int* resultSize, int* resultOffset, byte* last);

    [DllImport(LibraryName, EntryPoint = "build", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int Build(byte* input, int inputLength, byte* postfix, byte* output, int outputCapacity, int* outputLength);

    [DllImport(LibraryName, EntryPoint = "start_merge", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int StartMerge(byte* input, int inputLength);

    [DllImport(LibraryName, EntryPoint = "merge", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int Merge(int* resultSize);

    [DllImport(LibraryName, EntryPoint = "get_result", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int GetResult(byte* buffer, int bufferSize);

    /* Error messages */

    [DllImport(LibraryName, EntryPoint = "get_error", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr GetError(int errorCode);

    [DllImport(LibraryName, EntryPoint = "free_error", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void FreeError(IntPtr pointer);
}
