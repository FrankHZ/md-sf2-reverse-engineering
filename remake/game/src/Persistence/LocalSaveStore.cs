using System.Text.RegularExpressions;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Content.Persistence;

namespace Sf2.Remake.GodotAdapter.Persistence;

internal sealed record SlotRead(SaveRecord? Primary, SaveRecord? Backup, string? Error, string? BackupError)
{
    // Only absent or structurally corrupt primary files permit explicit backup repair.
    // Semantic/content rejection, future versions and unavailable readers preserve primary bytes.
    internal bool CanRecover => Primary is null && Backup is not null && Error is null or
        "save-size" or "save-malformed" or "save-empty" or "save-checksum" or "save-header" or
        "save-null-value" or "save-duplicate-key" or "save-presence" or "save-battle-custody";
}
internal sealed record SlotWrite(bool Success, string? Error, SaveRecord? Record = null);

// Two local slots, no manifest/database. Publications and repairs hold the named slot's writer lock.
internal sealed class LocalSaveStore
{
    private readonly string _directory;
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

    internal SlotRead Read(int slot)
    {
        string primary = PathFor(slot, "json"), backup = PathFor(slot, "bak");
        var p = ReadFile(primary, slot); var b = ReadFile(backup, slot);
        return new(p.Record, b.Record, p.Error, b.Error);
    }

    internal SlotWrite Write(int slot, OpeningSaveData data) => Write(new OpeningSaveRecord(1, slot, 1, DateTimeOffset.UtcNow, data));
    internal SlotWrite Write(int slot, BattleSaveData data) => Write(new BattleSaveRecord(slot, 1, DateTimeOffset.UtcNow, data));

    private SlotWrite Write(SaveRecord candidate)
    {
        int slot = candidate.Slot;
        string primary = PathFor(slot, "json"), backup = PathFor(slot, "bak");
        string? temporary = null;
        bool publicationAttempted = false;
        try
        {
            Directory.CreateDirectory(_directory);
            RequireNoLinks();
            using var writer = new FileStream(PathFor(slot, "lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            string? invalid = _validate(candidate);
            if (invalid is not null) return new(false, invalid);
            var old = Read(slot);
            // An incompatible or unreadable primary is not destroyed by a new save; recovery is explicit.
            if (File.Exists(primary) && old.Primary is null) return new(false, "save-primary-invalid-use-recovery");
            long sequence = checked(Math.Max(old.Primary?.Sequence ?? 0, old.Backup?.Sequence ?? 0) + 1);
            var record = candidate with { Sequence = sequence };
            byte[] bytes = OpeningSaveJson.WriteRecord(record);
            temporary = Path.Combine(_directory, $"slot{slot}.{Guid.NewGuid():N}.tmp");
            using (var output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                4096, FileOptions.WriteThrough))
            { output.Write(bytes); output.Flush(flushToDisk: true); }
            var reread = ReadFile(temporary, slot);
            if (reread.Record is null || !bytes.SequenceEqual(reread.Bytes!)) return new(false, reread.Error ?? "save-readback");
            publicationAttempted = true;
            if (old.Primary is not null) File.Replace(temporary, primary, backup);
            else File.Move(temporary, primary);
            temporary = null;
            var committed = ReadFile(primary, slot);
            if (committed.Record?.Sequence != sequence || !bytes.SequenceEqual(committed.Bytes!)) return new(false, "save-publication-uncertain");
            return new(true, null, committed.Record);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or NotSupportedException or SaveFormatException or OverflowException)
        { return new(false, publicationAttempted ? "save-publication-uncertain" : error is SaveFormatException format ? format.Code : "save-storage-unavailable"); }
        finally
        {
            // Only this operation's unpublished temporary is disposable. Never delete a primary/backup.
            if (temporary is not null)
                try { File.Delete(temporary); } catch (Exception error) when (error is IOException or UnauthorizedAccessException) { }
        }
    }

    // Recovery loads the backup explicitly. Preserve its bytes when repairing a corrupt primary;
    // replacing it with the corrupt primary would destroy the last valid progress.
    internal SlotWrite Recover(int slot)
    {
        string? temporary = null;
        bool attempted = false;
        try
        {
            RequireNoLinks();
            using var writer = new FileStream(PathFor(slot, "lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            var read = Read(slot);
            if (!read.CanRecover)
                return new(false, "save-recovery-unavailable");
            // Keep the second read bounded too; a noncooperating writer may change the backup.
            var source = ReadFile(PathFor(slot, "bak"), slot);
            if (source.Record is null) return new(false, "save-recovery-invalid");
            byte[] bytes = source.Bytes!;
            temporary = Path.Combine(_directory, $"slot{slot}.{Guid.NewGuid():N}.tmp");
            using (var output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
            { output.Write(bytes); output.Flush(true); }
            var reread = ReadFile(temporary, slot);
            if (reread.Record is null || !bytes.SequenceEqual(reread.Bytes!)) return new(false, "save-recovery-invalid");
            attempted = true;
            if (File.Exists(PathFor(slot, "json"))) File.Replace(temporary, PathFor(slot, "json"), null);
            else File.Move(temporary, PathFor(slot, "json"));
            temporary = null;
            var restored = ReadFile(PathFor(slot, "json"), slot);
            return restored.Record is null || !bytes.SequenceEqual(restored.Bytes!) ? new(false, "save-publication-uncertain") : new(true, null, restored.Record);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or NotSupportedException)
        { return new(false, attempted ? "save-publication-uncertain" : "save-storage-unavailable"); }
        finally { if (temporary is not null) try { File.Delete(temporary); } catch (IOException) { } catch (UnauthorizedAccessException) { } }
    }

    private (SaveRecord? Record, string? Error, byte[]? Bytes) ReadFile(string path, int slot)
    {
        try
        {
            RequireNoLinks();
            if (!File.Exists(path)) return (null, null, null);
            // Bound the allocation even if another process changes the file after its length is read.
            using var input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (input.Length is <= 0 or > OpeningSaveJson.MaximumBytes) return (null, "save-size", null);
            byte[] bytes = new byte[(int)input.Length]; input.ReadExactly(bytes);
            if (input.ReadByte() != -1) return (null, "save-size", null);
            var record = OpeningSaveJson.ReadRecord(bytes, slot);
            string? error = _validate(record);
            return error is null ? (record, null, bytes) : (null, error, null);
        }
        catch (SaveFormatException error) { return (null, error.Code, null); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or NotSupportedException)
        { return (null, "save-storage-unavailable", null); }
    }

    private string PathFor(int slot, string extension) => slot is 1 or 2
        ? Path.Combine(_directory, $"slot{slot}.{extension}") : throw new ArgumentOutOfRangeException(nameof(slot));
    private void RequireNoLinks()
    {
        for (var directory = new DirectoryInfo(_directory); directory is not null; directory = directory.Parent)
            if (directory.Exists && (directory.Attributes & FileAttributes.ReparsePoint) != 0) throw new IOException("save-directory-link");
        if (Directory.Exists(_directory) && Directory.EnumerateFileSystemEntries(_directory)
                .Any(path => (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)) throw new IOException("save-file-link");
    }
}
