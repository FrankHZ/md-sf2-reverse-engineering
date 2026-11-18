using System.Globalization;
using System.Text.RegularExpressions;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Content.Persistence;

namespace Sf2.Remake.GodotAdapter.Persistence;

internal sealed record SaveFile(SaveRecord? Record, string? Error, byte[]? Bytes = null, long Length = 0, long Modified = 0)
{
    internal bool Matches(SaveFile other) => Error == other.Error && Length == other.Length && Modified == other.Modified &&
        (Bytes is null ? other.Bytes is null : other.Bytes is not null && Bytes.AsSpan().SequenceEqual(other.Bytes));
}

internal sealed record SlotRead(SaveFile Main, SaveFile Previous)
{
    internal SaveRecord? Primary => Main.Record;
    internal SaveRecord? Backup => Previous.Record;
    internal string? Error => Main.Error;
    internal string? BackupError => Previous.Error;
    internal bool CanRecover => Primary is null && Backup is not null && Error is null or
        "save-size" or "save-malformed" or "save-empty" or "save-checksum" or "save-header" or
        "save-null-value" or "save-duplicate-key" or "save-presence" or "save-battle-custody";
    internal bool Matches(SlotRead other) => Main.Matches(other.Main) && Previous.Matches(other.Previous);
}

internal sealed record SlotWrite(bool Success, string? Error, SaveRecord? Record = null);
internal sealed record SavePage(int Index, int Count, (int Slot, SlotRead Read)[] Rows, int? NewSlot);

internal sealed class LocalSaveStore
{
    private readonly string _directory;
    private readonly string? _output;
    private readonly Func<SaveRecord, string?> _validate;

    internal LocalSaveStore(string root, string space, string worktree, Func<SaveRecord, string?> validate)
    {
        if (string.IsNullOrWhiteSpace(root) || !Path.IsPathFullyQualified(root) ||
            !Regex.IsMatch(space, "^[A-Za-z0-9_-]{1,64}$", RegexOptions.CultureInvariant))
            throw new ArgumentException("save-root-configuration");
        root = Path.GetFullPath(root);
        string checkout = Path.TrimEndingDirectorySeparator(Path.GetFullPath(worktree));
        if (root.Equals(checkout, StringComparison.OrdinalIgnoreCase) || root.StartsWith(checkout + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("save-root-inside-worktree");
        for (var parent = new DirectoryInfo(root); parent is not null; parent = parent.Parent)
            if (parent.Exists && ((parent.Attributes & FileAttributes.ReparsePoint) != 0 ||
                File.Exists(Path.Combine(parent.FullName, ".git")) || Directory.Exists(Path.Combine(parent.FullName, ".git"))))
                throw new ArgumentException("save-root-inside-worktree-or-link");
        _directory = Path.Combine(root, space, "sf2-opening");
        _validate = validate;
    }

    // Explicit output has its own sidecars. Its directory can contain unrelated developer inputs.
    internal LocalSaveStore(string output, Func<SaveRecord, string?> validate)
    {
        _output = output;
        _directory = Path.GetDirectoryName(output)!;
        _validate = validate;
    }

    internal SavePage ReadPage(int page)
    {
        if (_output is not null) return new(0, 1, [(1, Read(1))], null);
        const int pageSize = 6;
        var slots = new SortedSet<int> { 1, 2 };
        try
        {
            var comparison = OperatingSystem.IsWindows() ? RegexOptions.IgnoreCase : RegexOptions.None;
            InspectDirectory(path =>
            {
                var name = Path.GetFileName(path);
                var match = Regex.Match(name, @"^slot([1-9][0-9]*)\.(json|bak)$", comparison | RegexOptions.CultureInvariant);
                if (match.Success && int.TryParse(match.Groups[1].Value, CultureInfo.InvariantCulture, out int slot))
                    slots.Add(slot);
            });
            int pages = (slots.Count - 1) / pageSize + 1;
            page = Math.Clamp(page, 0, pages - 1);
            int next = slots.Max == int.MaxValue ? slots.First(slot => slot < int.MaxValue && !slots.Contains(slot + 1)) + 1 : slots.Max + 1;
            return new(page, pages, slots.Skip(page * pageSize).Take(pageSize).Select(slot => (slot, Read(slot))).ToArray(), next);
        }
        catch (Exception error) when (StorageFailure(error)) { throw new ArgumentException("save-storage-unavailable", error); }
    }

    internal SlotRead Read(int slot) => new(ReadFile(PathFor(slot, "json"), _output is null ? slot : null, _validate),
        ReadFile(PathFor(slot, "bak"), _output is null ? slot : null, _validate));

    internal static SaveFile ReadSeed(string path) => ReadFile(path, null, null);

    internal SlotWrite Write(int slot, SlotRead seen, OpeningSaveData data) =>
        Publish(slot, seen, new OpeningSaveRecord(1, slot, 1, DateTimeOffset.UtcNow, data));
    internal SlotWrite Write(int slot, SlotRead seen, BattleSaveData data) =>
        Publish(slot, seen, new BattleSaveRecord(slot, 1, DateTimeOffset.UtcNow, data));
    internal SlotWrite Recover(int slot, SlotRead seen) => Publish(slot, seen, null);

    private SlotWrite Publish(int slot, SlotRead seen, SaveRecord? candidate)
    {
        string primary = PathFor(slot, "json"), backup = PathFor(slot, "bak"), lockPath = PathFor(slot, "lock");
        string? temporary = null;
        bool attempted = false;
        try
        {
            InspectDirectory();
            Directory.CreateDirectory(_directory);
            CheckPath(lockPath);
            using var writer = new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            var current = Read(slot);
            if (!seen.Matches(current)) return new(false, "save-record-changed");
            byte[] bytes;
            if (candidate is null)
            {
                if (!current.CanRecover) return new(false, "save-recovery-unavailable");
                bytes = current.Previous.Bytes!;
            }
            else
            {
                if (current.Error is not null) return new(false, "save-primary-invalid-use-recovery");
                if (current.BackupError is not null) return new(false, "save-backup-unreadable");
                if (_validate(candidate) is { } invalid) return new(false, invalid);
                long sequence = checked(Math.Max(current.Primary?.Sequence ?? 0, current.Backup?.Sequence ?? 0) + 1);
                bytes = OpeningSaveJson.WriteRecord(candidate with { Sequence = sequence });
            }
            temporary = Path.Combine(_directory, $".save-{Guid.NewGuid():N}.tmp");
            CheckPath(temporary);
            using (var output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
            { output.Write(bytes); output.Flush(true); }
            int? expectedSlot = _output is null ? slot : null;
            var reread = ReadFile(temporary, expectedSlot, _validate);
            if (reread.Record is null || !bytes.AsSpan().SequenceEqual(reread.Bytes)) return new(false, reread.Error ?? "save-readback");
            // The slot lock serializes cooperating writers. Recheck the actual publication operands
            // after validation; another application can still replace a path between OS calls.
            CheckPath(_directory);
            RejectLink(primary); RejectLink(backup); RejectLink(temporary);
            attempted = true;
            if (current.Primary is not null || current.Error is not null)
                File.Replace(temporary, primary, candidate is null ? null : backup);
            else File.Move(temporary, primary);
            temporary = null;
            var committed = ReadFile(primary, expectedSlot, _validate);
            return committed.Record is not null && bytes.AsSpan().SequenceEqual(committed.Bytes)
                ? new(true, null, committed.Record) : new(false, "save-publication-uncertain");
        }
        catch (Exception error) when (StorageFailure(error) || error is SaveFormatException or OverflowException)
        { return new(false, attempted ? "save-publication-uncertain" : error is SaveFormatException format ? format.Code : "save-storage-unavailable"); }
        finally
        {
            if (temporary is not null)
                try { CheckPath(temporary); File.Delete(temporary); } catch (Exception error) when (StorageFailure(error)) { }
        }
    }

    private static SaveFile ReadFile(string path, int? slot, Func<SaveRecord, string?>? validate)
    {
        byte[]? bytes = null;
        try
        {
            CheckPath(path);
            using var input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (input.Length > OpeningSaveJson.MaximumBytes)
                return new(null, "save-size", Length: input.Length, Modified: File.GetLastWriteTimeUtc(path).Ticks);
            bytes = new byte[(int)input.Length]; input.ReadExactly(bytes);
            if (input.ReadByte() != -1) return new(null, "save-storage-unavailable");
            var record = OpeningSaveJson.ReadRecord(bytes, slot);
            string? error = validate?.Invoke(record);
            return new(error is null ? record : null, error, bytes);
        }
        catch (FileNotFoundException) { return new(null, null); }
        catch (DirectoryNotFoundException) { return new(null, null); }
        catch (SaveFormatException error) { return new(null, error.Code, bytes); }
        catch (Exception error) when (StorageFailure(error)) { return new(null, "save-storage-unavailable"); }
    }

    private string PathFor(int slot, string extension) => _output is not null
        ? extension == "json" ? _output : _output + "." + extension
        : Path.Combine(_directory, $"slot{slot.ToString(CultureInfo.InvariantCulture)}.{extension}");

    private void InspectDirectory(Action<string>? visit = null)
    {
        CheckPath(_directory);
        if (_output is not null) return;
        try
        {
            foreach (string path in Directory.EnumerateFileSystemEntries(_directory))
            {
                RejectLink(path);
                visit?.Invoke(path);
            }
        }
        catch (DirectoryNotFoundException) when (!Directory.Exists(_directory)) { }
    }

    private static bool StorageFailure(Exception error) => error is IOException or UnauthorizedAccessException or NotSupportedException;
    private static void CheckPath(string path)
    {
        var parts = new Stack<string>();
        for (string? part = path; part is not null; part = Path.GetDirectoryName(part)) parts.Push(part);
        foreach (var part in parts) RejectLink(part);
    }
    private static void RejectLink(string path)
    {
        try
        {
            if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0) throw new IOException("save-link");
        }
        catch (FileNotFoundException) { }
        catch (DirectoryNotFoundException) { }
    }
}
