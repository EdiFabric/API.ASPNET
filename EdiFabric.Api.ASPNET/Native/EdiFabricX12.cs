using System.Text;

namespace EdiFabric.Native.X12;

/// <summary>mode argument for parse and start_split.</summary>
public enum ParseMode
{
    /// <summary>Transaction-set JSON only.</summary>
    Json = 1,

    /// <summary>JSON plus a validation report.</summary>
    JsonValidate = 2,

    /// <summary>JSON plus validation and a 999/997/TA1 acknowledgment.</summary>
    JsonValidateAck = 3,
}

/// <summary>min_level argument for init_logger.</summary>
public enum LogLevel
{
    Trace = 0,
    Debug = 1,
    Information = 2,
    Warning = 3,
    Error = 4,
}

/// <summary>Library-level status codes. Validation codes come from the engine.</summary>
public enum EdiFabricErrorCode
{
    Success = 0,
    InsufficientCapacity = 1,
    IncorrectInput = 611,
    LoggerInitialization = 612,
    MapDeserialization = 613,
    IncorrectCapacity = 614,
    MapNotSet = 615,
    IncorrectMode = 616,
    NoJson = 617,
    ValidationUnavailable = 618,
    ValidationSerialization = 619,
    IncorrectToken = 620,
    ConfigDeserialization = 621,
    SplitSegmentIdMissing = 622,
    SplitNotStarted = 623,
    NoResult = 624,
    ResultSizeMismatch = 625,
    MergeNotStarted = 626,
    IncorrectOutputPointer = 627,
    IncorrectSerial = 628,
    LicenseNotInstalled = 629,
    AppVersionExceeded = 630,
    TokenExpired = 631,
    TokenMissing = 632,
    MaxLicensesExceeded = 633,
    LicenseSnapshotMissing = 634,
    LicenseNotSet = 635,
}

/// <summary>Thrown when a native call returns a non-zero status code.</summary>
public sealed class EdiFabricException : Exception
{
    public EdiFabricException(int code, string message, string api)
        : base(string.IsNullOrEmpty(api) ? $"error {code}: {message}" : $"{api}: error {code}: {message}")
    {
        Code = code;
        NativeMessage = message;
        Api = api;
    }

    /// <summary>The native status code.</summary>
    public int Code { get; }

    /// <summary>The message reported by get_error, without the wrapper prefix.</summary>
    public string NativeMessage { get; }

    /// <summary>The C entry point that failed.</summary>
    public string Api { get; }
}

/// <summary>Output of a parse call.</summary>
/// <param name="Output">The full UTF-8 payload returned by the native library.</param>
/// <param name="Offset">Where the validation and acknowledgment section starts; 0 in mode 1.</param>
public readonly record struct ParseResult(string Output, int Offset)
{
    /// <summary>The transaction-set JSON portion of <see cref="Output"/>.</summary>
    public string Transactions => Offset > 0 ? Output[..Offset] : Output;

    /// <summary>The validation and acknowledgment JSON, or an empty string in mode 1.</summary>
    public string Report => Offset > 0 ? Output[Offset..] : string.Empty;
}

/// <summary>One step of a split stream.</summary>
/// <param name="Size">Bytes available from get_result; 0 when this step produced nothing.</param>
/// <param name="Offset">Offset of the validation section inside the result.</param>
/// <param name="IsLast">True when this is the final step.</param>
public readonly record struct SplitStep(int Size, int Offset, bool IsLast);

/// <summary>A payload produced by a split stream.</summary>
/// <param name="Payload">The UTF-8 result bytes.</param>
/// <param name="Offset">Offset of the validation section inside the payload.</param>
/// <param name="IsLast">True when this is the final payload.</param>
public readonly record struct SplitPart(byte[] Payload, int Offset, bool IsLast);

/// <summary>
/// Managed wrapper over the ediFabric Native X12 C ABI.
/// Grow-and-retry is handled internally; failures surface as <see cref="EdiFabricException"/>.
/// </summary>
public static class EdiFabricX12
{
    private const int Success = 0;
    private const int InsufficientCapacity = 1;

    private static bool _freeErrorMissing;

    /// <summary>
    /// Explicit path to the native library, either the file itself or its folder.
    /// Set this before the first call. EDIFABRIC_X12_LIB is used when it is null.
    /// </summary>
    public static string? LibraryPath { get; set; }

    /// <summary>The library file that was actually loaded, once resolution has happened.</summary>
    public static string? ResolvedLibraryPath { get; internal set; }

    /// <summary>
    /// Loads the native library and returns its application version.
    /// Optional: any other call loads it on demand.
    /// </summary>
    public static int Load(string? libraryPath = null)
    {
        if (libraryPath is not null)
            LibraryPath = libraryPath;

        NativeMethods.EnsureResolverRegistered();
        return GetAppVersion();
    }

    /* ------------------------------------------------------------------ */
    /* Lifecycle and logging                                               */
    /* ------------------------------------------------------------------ */

    /// <summary>Writes a log file at <paramref name="path"/>.</summary>
    public static unsafe void InitLogger(string path, LogLevel minLevel = LogLevel.Information)
    {
        var bytes = Encoding.UTF8.GetBytes(path);
        fixed (byte* pointer = bytes)
        {
            Check(NativeMethods.InitLogger(pointer, bytes.Length, (int)minLevel), "init_logger");
        }
    }

    /// <summary>Flushes and stops the logger.</summary>
    public static void ShutdownLogger() => Check(NativeMethods.ShutdownLogger(), "shutdown_logger");

    /// <summary>Resets the model map, stream state, last result, license, and logger.</summary>
    public static void ClearCache() => Check(NativeMethods.ClearCache(), "clear_cache");

    /* ------------------------------------------------------------------ */
    /* Licensing                                                           */
    /* ------------------------------------------------------------------ */

    /// <summary>Registers this machine once. Requires internet access.</summary>
    public static unsafe void InstallLicense(string serial)
    {
        var bytes = Encoding.UTF8.GetBytes(serial);
        fixed (byte* pointer = bytes)
        {
            Check(NativeMethods.InstallLicense(pointer, bytes.Length), "install_license");
        }
    }

    /// <summary>Returns the library application version.</summary>
    public static unsafe int GetAppVersion()
    {
        int version;
        Check(NativeMethods.GetAppVersion(&version), "get_app_version");
        return version;
    }

    /// <summary>Fetches a signed license token for the serial. Requires internet access.</summary>
    public static unsafe string GetToken(string serial, int capacity = 4096)
    {
        var serialBytes = Encoding.UTF8.GetBytes(serial);
        capacity = Math.Max(1, capacity);

        while (true)
        {
            var output = new byte[capacity];
            int length;
            int rc;

            fixed (byte* serialPointer = serialBytes)
            fixed (byte* outputPointer = output)
            {
                rc = NativeMethods.GetToken(serialPointer, serialBytes.Length, outputPointer, capacity, &length);
            }

            if (rc == InsufficientCapacity)
            {
                capacity = length > capacity ? length : capacity * 2;
                continue;
            }

            Check(rc, "get_token");
            return Encoding.UTF8.GetString(output, 0, length);
        }
    }

    /// <summary>Validates a token without caching it.</summary>
    public static unsafe void ValidateToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        fixed (byte* pointer = bytes)
        {
            Check(NativeMethods.ValidateToken(pointer, bytes.Length), "validate_token");
        }
    }

    /// <summary>Caches a token for this process.</summary>
    public static unsafe void SetToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        fixed (byte* pointer = bytes)
        {
            Check(NativeMethods.SetToken(pointer, bytes.Length), "set_token");
        }
    }

    /// <summary>Returns the token expiry as .NET UTC ticks, or 0 when no token is set.</summary>
    public static unsafe long GetTokenExpirationTicks()
    {
        long ticks;
        Check(NativeMethods.GetTokenExpiration(&ticks), "get_token_expiration");
        return ticks;
    }

    /// <summary>Returns the token expiry, or null when no token is set.</summary>
    public static DateTime? GetTokenExpiration()
    {
        var ticks = GetTokenExpirationTicks();
        return ticks == 0 ? null : new DateTime(ticks, DateTimeKind.Utc);
    }

    /// <summary>Caches a serial for runtime authorization against the license server.</summary>
    public static unsafe void SetSerial(string serial)
    {
        var bytes = Encoding.UTF8.GetBytes(serial);
        fixed (byte* pointer = bytes)
        {
            Check(NativeMethods.SetSerial(pointer, bytes.Length), "set_serial");
        }
    }

    /* ------------------------------------------------------------------ */
    /* Model map                                                           */
    /* ------------------------------------------------------------------ */

    /// <summary>Loads the template map JSON. Call once before parsing or splitting.</summary>
    public static unsafe void SetMap(string mapJson)
    {
        var bytes = Encoding.UTF8.GetBytes(mapJson);
        fixed (byte* pointer = bytes)
        {
            Check(NativeMethods.SetMap(pointer, bytes.Length), "set_map");
        }
    }

    public static unsafe void SetMap(byte[] mapJson)
    {
        fixed (byte* pointer = mapJson)
        {
            Check(NativeMethods.SetMap(pointer, mapJson.Length), "set_map");
        }
    }

    /* ------------------------------------------------------------------ */
    /* Parse, split, build, merge                                          */
    /* ------------------------------------------------------------------ */

    /// <summary>Parses a whole interchange.</summary>
    public static ParseResult Parse(string edi, ParseMode mode = ParseMode.Json, string? config = null, int capacity = 0)
        => Parse(Encoding.UTF8.GetBytes(edi), mode, config, capacity);

    /// <summary>Parses a whole interchange.</summary>
    public static unsafe ParseResult Parse(ReadOnlySpan<byte> edi, ParseMode mode = ParseMode.Json, string? config = null, int capacity = 0)
    {
        var configBytes = config is null ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(config);
        var currentCapacity = capacity > 0 ? capacity : GrowthEstimate(edi.Length);

        while (true)
        {
            var output = new byte[currentCapacity];
            int length;
            int offset;
            int rc;

            fixed (byte* inputPointer = edi)
            fixed (byte* configPointer = configBytes)
            fixed (byte* outputPointer = output)
            {
                rc = NativeMethods.Parse(
                    inputPointer, edi.Length,
                    (int)mode,
                    configPointer, configBytes.Length,
                    outputPointer, currentCapacity,
                    &length, &offset);
            }

            if (rc == InsufficientCapacity)
            {
                currentCapacity = length > currentCapacity ? length : currentCapacity * 2;
                continue;
            }

            Check(rc, "parse");
            return new ParseResult(Encoding.UTF8.GetString(output, 0, length), offset);
        }
    }

    /// <summary>Begins a streaming split. The config must contain a split section with segment_id.</summary>
    public static void StartSplit(string edi, ParseMode mode = ParseMode.Json, string? config = null)
        => StartSplit(Encoding.UTF8.GetBytes(edi), mode, config);

    /// <summary>Begins a streaming split. The config must contain a split section with segment_id.</summary>
    public static unsafe void StartSplit(ReadOnlySpan<byte> edi, ParseMode mode = ParseMode.Json, string? config = null)
    {
        var configBytes = config is null ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(config);

        fixed (byte* inputPointer = edi)
        fixed (byte* configPointer = configBytes)
        {
            Check(
                NativeMethods.StartSplit(inputPointer, edi.Length, (int)mode, configPointer, configBytes.Length),
                "start_split");
        }
    }

    /// <summary>
    /// Advances the split stream. Fetch the payload with <see cref="GetResult"/>
    /// whenever <see cref="SplitStep.Size"/> is greater than zero.
    /// </summary>
    public static unsafe SplitStep Split()
    {
        int size;
        int offset;
        byte last;

        Check(NativeMethods.Split(&size, &offset, &last), "split");
        return new SplitStep(size, offset, last != 0);
    }

    /// <summary>
    /// Builds X12 EDI from transaction-set JSON. <paramref name="postfix"/> is appended
    /// after each segment terminator; pass null for compact output.
    /// </summary>
    public static unsafe string Build(string json, string? postfix = null, int capacity = 0)
    {
        var inputBytes = Encoding.UTF8.GetBytes(json);
        var postfixBytes = postfix is null ? null : Encoding.UTF8.GetBytes(postfix + "\0");
        var currentCapacity = capacity > 0 ? capacity : Math.Max(4096, inputBytes.Length);

        while (true)
        {
            var output = new byte[currentCapacity];
            int length;
            int rc;

            fixed (byte* inputPointer = inputBytes)
            fixed (byte* postfixPointer = postfixBytes)
            fixed (byte* outputPointer = output)
            {
                rc = NativeMethods.Build(
                    inputPointer, inputBytes.Length,
                    postfixPointer,
                    outputPointer, currentCapacity,
                    &length);
            }

            if (rc == InsufficientCapacity)
            {
                currentCapacity = length > currentCapacity ? length : currentCapacity * 2;
                continue;
            }

            Check(rc, "build");
            return Encoding.UTF8.GetString(output, 0, length);
        }
    }

    /// <summary>Begins a streaming merge from a full interchange JSON document.</summary>
    public static unsafe void StartMerge(string json)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        fixed (byte* pointer = bytes)
        {
            Check(NativeMethods.StartMerge(pointer, bytes.Length), "start_merge");
        }
    }

    /// <summary>Returns the size of the next segment, or 0 at the end of the stream.</summary>
    public static unsafe int Merge()
    {
        int size;
        Check(NativeMethods.Merge(&size), "merge");
        return size;
    }

    /// <summary>Copies the last split or merge result. The size must match the reported size.</summary>
    public static unsafe byte[] GetResult(int size)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(size);

        var buffer = new byte[size];
        fixed (byte* pointer = buffer)
        {
            Check(NativeMethods.GetResult(pointer, size), "get_result");
        }

        return buffer;
    }

    /* ------------------------------------------------------------------ */
    /* Convenience wrappers                                                */
    /* ------------------------------------------------------------------ */

    /// <summary>Streams every payload produced by a split, one transaction set or loop at a time.</summary>
    public static IEnumerable<SplitPart> EnumerateSplit(string edi, ParseMode mode = ParseMode.Json, string? config = null)
        => EnumerateSplit(Encoding.UTF8.GetBytes(edi), mode, config);

    /// <summary>Streams every payload produced by a split, one transaction set or loop at a time.</summary>
    public static IEnumerable<SplitPart> EnumerateSplit(byte[] edi, ParseMode mode = ParseMode.Json, string? config = null)
    {
        StartSplit(edi, mode, config);

        while (true)
        {
            var step = Split();
            if (step.Size > 0)
                yield return new SplitPart(GetResult(step.Size), step.Offset, step.IsLast);

            if (step.IsLast)
                yield break;
        }
    }

    /// <summary>Streams each X12 segment produced by a merge.</summary>
    public static IEnumerable<byte[]> EnumerateMerge(string json)
    {
        StartMerge(json);

        while (true)
        {
            var size = Merge();
            if (size == 0)
                yield break;

            yield return GetResult(size);
        }
    }

    /* ------------------------------------------------------------------ */
    /* Errors                                                              */
    /* ------------------------------------------------------------------ */

    /// <summary>Returns the message for a status code, releasing the native string.</summary>
    public static string GetError(int errorCode)
    {
        var pointer = NativeMethods.GetError(errorCode);
        if (pointer == IntPtr.Zero)
            return $"Unknown error {errorCode}";

        try
        {
            return System.Runtime.InteropServices.Marshal.PtrToStringUTF8(pointer) ?? $"Unknown error {errorCode}";
        }
        finally
        {
            FreeError(pointer);
        }
    }

    /// <summary>Returns the message for a status code, releasing the native string.</summary>
    public static string GetError(EdiFabricErrorCode errorCode) => GetError((int)errorCode);

    /// <summary>
    /// Releases a string returned by the raw get_error export.
    /// <see cref="GetError(int)"/> already does this for you.
    /// </summary>
    public static void FreeError(IntPtr pointer)
    {
        if (pointer == IntPtr.Zero)
            return;

        if (!_freeErrorMissing)
        {
            try
            {
                NativeMethods.FreeError(pointer);
                return;
            }
            catch (EntryPointNotFoundException)
            {
                // Older builds do not export free_error.
                _freeErrorMissing = true;
            }
        }

        System.Runtime.InteropServices.Marshal.FreeHGlobal(pointer);
    }

    /// <summary>Throws <see cref="EdiFabricException"/> unless the status code is success.</summary>
    public static void Check(int rc, string api = "")
    {
        if (rc == Success)
            return;

        throw new EdiFabricException(rc, GetError(rc), api);
    }

    /// <summary>
    /// Direct access to the raw exports, for callers that want to own the
    /// unmanaged memory themselves.
    /// </summary>
    public static class Raw
    {
        /// <summary>
        /// Calls get_error and returns the unmanaged pointer. Release it with
        /// <see cref="EdiFabricX12.FreeError"/>.
        /// </summary>
        public static IntPtr GetError(int errorCode) => NativeMethods.GetError(errorCode);

        /// <summary>The library file name for the current platform.</summary>
        public static string PlatformFileName => NativeMethods.PlatformFileName;
    }

    private static int GrowthEstimate(int inputLength)
    {
        var estimate = (long)inputLength * 12;
        return (int)Math.Clamp(estimate, 4096, int.MaxValue);
    }
}
