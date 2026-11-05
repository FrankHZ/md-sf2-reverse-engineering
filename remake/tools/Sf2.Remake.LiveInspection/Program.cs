using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.Diagnostics.Runtime;

// Read a Windows PSS snapshot, never execute managed methods or write to the game.
// This is a field receipt, not another implementation of QuerySave/RequireStable.
try
{
    if (!OperatingSystem.IsWindows() || args.Length != 3)
        throw new ArgumentException("Windows only: <pid> <process-start-UTC-O> <new-output-under-local>");
    int pid = int.Parse(args[0], CultureInfo.InvariantCulture);
    var expectedStart = DateTimeOffset.ParseExact(args[1], "O", CultureInfo.InvariantCulture);
    string output = Path.GetFullPath(args[2]);
    string repo = FindRepo(AppContext.BaseDirectory);
    ValidateOutput(repo, output);
    using var process = Process.GetProcessById(pid);
    if (process.StartTime.ToUniversalTime() != expectedStart.UtcDateTime)
        throw new ArgumentException("process-start-mismatch");
    string configured = Environment.GetEnvironmentVariable("GODOT_BIN") ?? "";
    if (!Path.IsPathFullyQualified(configured)) throw new ArgumentException("absolute-GODOT_BIN-required");
    string native = Path.GetFullPath(configured).Replace("_console.exe", ".exe", StringComparison.OrdinalIgnoreCase);
    if (!string.Equals(process.MainModule?.FileName, native, StringComparison.OrdinalIgnoreCase))
        throw new ArgumentException("pid-is-not-configured-native-Godot");
    if (process.PrivateMemorySize64 > 8L * 1024 * 1024 * 1024)
        throw new ArgumentException("target-over-8GiB-replan-snapshot-budget");
    var before = Memory();
    if (before.Limit - before.Total < 2UL * 1024 * 1024 * 1024)
        throw new ArgumentException("less-than-2GiB-system-commit-headroom");
    string dac = process.Modules.Cast<ProcessModule>().Single(m => m.ModuleName == "coreclr.dll").FileName;
    dac = Path.Combine(Path.GetDirectoryName(dac)!, "mscordaccore.dll");
    var clock = Stopwatch.StartNew();
    object state;
    object[] modules;
    long snapshotMilliseconds;
    (ulong Total, ulong Limit) during;
    using (var target = DataTarget.CreateSnapshotAndAttach(pid))
    {
        snapshotMilliseconds = clock.ElapsedMilliseconds;
        during = Memory();
        using var runtime = target.ClrVersions.Single().CreateRuntime(dac);
        var roots = new List<ClrObject>();
        long objects = 0;
        foreach (var obj in runtime.Heap.EnumerateObjects())
        {
            if (++objects > 5_000_000 || clock.Elapsed > TimeSpan.FromSeconds(30))
                throw new InvalidOperationException("heap-walk-budget-exceeded");
            if (obj.Type?.Name == "Sf2.Remake.GodotAdapter.GameRoot" && !obj.ReadField<bool>("_disposed") &&
                obj.ReadField<ulong>("NativePtr") != 0) roots.Add(obj);
        }
        if (roots.Count != 1) throw new InvalidOperationException("expected-one-live-GameRoot");
        var root = roots[0];
        var session = root.ReadObjectField("_session");
        var current = session.ReadObjectField("_current");
        var story = Object(current, "Story");
        var active = Object(current, "Active");
        if (active.Type?.Name != "Sf2.Remake.Application.Runtime.ActiveExploration")
            throw new InvalidOperationException("current-mode-is-not-exploration");
        var world = Object(active, "World");
        var definition = Object(world, "Definition");
        var entities = Object(world, "AllEntities").ReadObjectField("list").AsArray();
        if (entities.Length > 128) throw new InvalidOperationException("entity-budget-exceeded");
        var view = Object(story, "LogicalView");
        var display = Object(story, "Display");
        var entityRows = new List<object>();
        for (int i = 0; i < entities.Length; i++)
        {
            var entity = entities.GetObjectValue(i);
            var actions = Object(entity, "Actions");
            object? actionTypes = null;
            if (!actions.IsNull)
            {
                var array = Object(actions, "Actions").ReadObjectField("list").AsArray();
                if (array.Length > 1024) throw new InvalidOperationException("action-budget-exceeded");
                actionTypes = Enumerable.Range(0, array.Length).Select(n => array.GetObjectValue(n).Type?.Name).ToArray();
            }
            entityRows.Add(new { fields = Fields(entity), motion = Fields(Object(entity, "Motion")),
                follower = Fields(Object(entity, "Follower")), actionTypes });
        }
        state = new
        {
            root = root.Address.ToString("x", CultureInfo.InvariantCulture), current = Fields(current), story = Fields(story),
            callers = Object(story, "Callers").ReadObjectField("list").AsArray().Length,
            logicalText = Fields(Object(story, "LogicalText")), view = Fields(view),
            area = Fields(Object(view, "Area")), axes = new[] { "AX", "AY", "BX", "BY" }.ToDictionary(n => n, n => Fields(Object(view, n))),
            display = Fields(display), paletteBase = Fields(Object(display, "Base")), paletteCurrent = Fields(Object(display, "Current")),
            world = Fields(world), map = Fields(Object(definition, "Map")), entities = entityRows,
            savePanel = Fields(root.ReadObjectField("_slots")), inspectedObjects = objects,
        };
        modules = runtime.EnumerateModules().Where(m => Path.GetFileName(m.Name ?? "").StartsWith("Sf2.Remake.", StringComparison.Ordinal)
            && (m.Name ?? "").EndsWith(".dll", StringComparison.OrdinalIgnoreCase)).Select(m => Module(target, m, repo)).ToArray();
    }
    var after = Memory();
    using var self = Process.GetCurrentProcess();
    process.Refresh();
    var receipt = new
    {
        formatVersion = 1, pid, processStartedUtc = expectedStart, capturedUtc = DateTimeOffset.UtcNow,
        method = "Windows PSS snapshot; managed fields only; no target method evaluation", modules, state,
        measurements = new { snapshotMilliseconds, elapsedMilliseconds = clock.ElapsedMilliseconds,
            // Snapshot-call duration is an upper bound, NOT a measurement of target suspension duration.
            systemCommitBefore = before.Total, systemCommitDuring = during.Total, systemCommitAfter = after.Total,
            systemCommitLimit = before.Limit, inspectorPeakWorkingSetBytes = self.PeakWorkingSet64,
            inspectorPrivateBytes = self.PrivateMemorySize64, targetPrivateBytes = process.PrivateMemorySize64,
            targetExited = process.HasExited },
    };
    byte[] json = JsonSerializer.SerializeToUtf8Bytes(receipt, new JsonSerializerOptions { WriteIndented = true });
    if (json.Length > 2 * 1024 * 1024) throw new InvalidOperationException("report-over-2MiB");
    using var stream = new FileStream(output, FileMode.CreateNew, FileAccess.Write, FileShare.None);
    stream.Write(json);
    Console.WriteLine($"captured pid={pid}; bytes={json.Length}; elapsedMs={clock.ElapsedMilliseconds}; targetExited={process.HasExited}");
    return 0;
}
catch (Exception error)
{
    Console.Error.WriteLine($"inspection failed: {error.GetType().Name}: {error.Message}");
    return 1;
}

static ClrObject Object(ClrObject obj, string property) => obj.ReadObjectField($"<{property}>k__BackingField");

static object? Fields(ClrObject obj) => obj.IsNull ? null : new
{
    type = obj.Type?.Name, address = obj.Address.ToString("x", CultureInfo.InvariantCulture),
    fields = ReadFields(obj.Type ?? throw new InvalidOperationException("unknown-type"), obj.Address, false, 0),
};

static Dictionary<string, object?> ReadFields(ClrType type, ulong address, bool interior, int depth)
{
    if (depth > 4 || type.Fields.Length > 128) throw new InvalidOperationException("field-budget-exceeded");
    var result = new Dictionary<string, object?>();
    foreach (var f in type.Fields)
    {
        string name = f.Name ?? throw new InvalidOperationException("unnamed-field");
        object? value;
        if (f.IsObjectReference)
        {
            var child = f.ReadObject(address, interior);
            value = child.IsNull ? null : child.Type?.IsString == true ? child.AsString(128)
                : new { type = child.Type?.Name, address = child.Address.ToString("x", CultureInfo.InvariantCulture) };
        }
        else value = f.ElementType switch
        {
            ClrElementType.Boolean => f.Read<bool>(address, interior),
            ClrElementType.Int8 => f.Read<sbyte>(address, interior),
            ClrElementType.UInt8 => f.Read<byte>(address, interior),
            ClrElementType.Int16 => f.Read<short>(address, interior),
            ClrElementType.UInt16 => f.Read<ushort>(address, interior),
            ClrElementType.Int32 => f.Read<int>(address, interior),
            ClrElementType.UInt32 => f.Read<uint>(address, interior),
            ClrElementType.Int64 => f.Read<long>(address, interior),
            ClrElementType.UInt64 => f.Read<ulong>(address, interior),
            ClrElementType.Float => f.Read<float>(address, interior),
            ClrElementType.Double => f.Read<double>(address, interior),
            ClrElementType.Struct => Value(f.ReadStruct(address, interior), depth + 1),
            _ => $"unread:{f.ElementType}",
        };
        result.Add(name, value);
    }
    return result;
}

static object Value(ClrValueType value, int depth) => ReadFields(value.Type!, value.Address, true, depth);

static object Module(DataTarget target, ClrModule module, string repo)
{
    if (module.MetadataLength > 32 * 1024 * 1024) throw new InvalidOperationException("module-metadata-budget-exceeded");
    var bytes = new byte[checked((int)module.MetadataLength)];
    if (target.DataReader.Read(module.MetadataAddress, bytes) != bytes.Length)
        throw new InvalidOperationException("module-metadata-read-failed");
    using var provider = MetadataReaderProvider.FromMetadataImage(ImmutableArray.Create(bytes));
    var reader = provider.GetMetadataReader();
    Guid loaded = reader.GetGuid(reader.GetModuleDefinition().Mvid);
    // Godot's assembly loader can retain only the module basename in CLR metadata.
    // Compare with this checkout's Debug artifacts, never infer its HEAD was loaded.
    string diskPath = Path.Combine(repo, "remake", "game", ".godot", "mono", "temp", "bin", "Debug", Path.GetFileName(module.Name!));
    using var stream = File.OpenRead(diskPath);
    using var pe = new PEReader(stream);
    var diskReader = pe.GetMetadataReader();
    Guid disk = diskReader.GetGuid(diskReader.GetModuleDefinition().Mvid);
    return new { name = Path.GetFileName(module.Name), loadedMvid = loaded, diskMvid = disk, diskMatchesLoaded = loaded == disk };
}

static string FindRepo(string path)
{
    for (var dir = new DirectoryInfo(path); dir is not null; dir = dir.Parent)
        if (File.Exists(Path.Combine(dir.FullName, "remake", "global.json")) && Directory.Exists(Path.Combine(dir.FullName, "src", "sf2tool"))) return dir.FullName;
    throw new ArgumentException("run-from-built-repository-tool");
}

static void ValidateOutput(string repo, string output)
{
    string local = Path.Combine(repo, "local") + Path.DirectorySeparatorChar;
    if (!output.StartsWith(local, StringComparison.OrdinalIgnoreCase) || File.Exists(output))
        throw new ArgumentException("output-must-be-new-under-owning-local");
    var parent = new DirectoryInfo(Path.GetDirectoryName(output)!);
    if (!parent.Exists) throw new ArgumentException("create-output-directory-before-inspection");
    for (var dir = parent; dir is not null; dir = dir.Parent)
        if ((dir.Attributes & FileAttributes.ReparsePoint) != 0) throw new ArgumentException("output-reparse-point");
}

static (ulong Total, ulong Limit) Memory()
{
    var info = new PerformanceInfo { Size = (uint)Marshal.SizeOf<PerformanceInfo>() };
    if (!GetPerformanceInfo(ref info, info.Size)) throw new InvalidOperationException("cannot-read-system-commit");
    return ((ulong)info.CommitTotal * (ulong)info.PageSize, (ulong)info.CommitLimit * (ulong)info.PageSize);
}

[DllImport("psapi.dll", SetLastError = true)]
[return: MarshalAs(UnmanagedType.Bool)]
static extern bool GetPerformanceInfo(ref PerformanceInfo info, uint size);

[StructLayout(LayoutKind.Sequential)]
struct PerformanceInfo
{
    public uint Size;
    public nuint CommitTotal, CommitLimit, CommitPeak, PhysicalTotal, PhysicalAvailable, SystemCache,
        KernelTotal, KernelPaged, KernelNonpaged, PageSize;
    public uint HandleCount, ProcessCount, ThreadCount;
}
