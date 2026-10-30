using System.Text.RegularExpressions;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Content.Persistence;

namespace Sf2.Remake.GodotAdapter.Persistence;

internal sealed record SlotRead(OpeningSaveRecord? Primary, OpeningSaveRecord? Backup, string? Error, string? BackupError);
internal sealed record SlotWrite(bool Success, string? Error, OpeningSaveRecord? Record = null);

// Two local slots, no manifest/database. Every operation uses a single named slot's writer lock.
internal sealed class LocalSaveStore
{
    private readonly string _directory;
    private readonly Func<OpeningSaveData, string?> _validate;

    internal LocalSaveStore(string root, string space, string worktree, Func<OpeningSaveData, string?> validate)
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

    internal SlotWrite Write(int slot, OpeningSaveData data)
    {
        string primary = PathFor(slot, "json"), backup = PathFor(slot, "bak");
        string? temporary = null;
        bool publicationAttempted = false;
        try
        {
            Directory.CreateDirectory(_directory);
            RequireNoLinks();
            using var writer = new FileStream(PathFor(slot, "lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            string? invalid = _validate(data);
            if (invalid is not null) return new(false, invalid);
            var old = Read(slot);
            // An incompatible or unreadable primary is not destroyed by a new save; recovery is explicit.
            if (File.Exists(primary) && old.Primary is null) return new(false, "save-primary-invalid-use-recovery");
            long sequence = checked(Math.Max(old.Primary?.Sequence ?? 0, old.Backup?.Sequence ?? 0) + 1);
            var record = new OpeningSaveRecord(1, slot, sequence, DateTimeOffset.UtcNow, data);
            byte[] bytes = OpeningSaveJson.Write(record);
            temporary = Path.Combine(_directory, $"slot{slot}.{Guid.NewGuid():N}.tmp");
            using (var output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                4096, FileOptions.WriteThrough))
            { output.Write(bytes); output.Flush(flushToDisk: true); }
            var reread = ReadFile(temporary, slot);
            if (reread.Record is null) return new(false, reread.Error);
            publicationAttempted = true;
            if (old.Primary is not null) File.Replace(temporary, primary, backup);
            else File.Move(temporary, primary);
            temporary = null;
            var committed = ReadFile(primary, slot);
            if (committed.Record?.Sequence != sequence) return new(false, "save-publication-uncertain");
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
            if (read.Primary is not null || read.Backup is null || read.Error is "save-incompatible" or "save-version")
                return new(false, "save-recovery-unavailable");
            byte[] bytes = File.ReadAllBytes(PathFor(slot, "bak"));
            temporary = Path.Combine(_directory, $"slot{slot}.{Guid.NewGuid():N}.tmp");
            using (var output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
            { output.Write(bytes); output.Flush(true); }
            if (ReadFile(temporary, slot).Record is null) return new(false, "save-recovery-invalid");
            attempted = true;
            if (File.Exists(PathFor(slot, "json"))) File.Replace(temporary, PathFor(slot, "json"), null);
            else File.Move(temporary, PathFor(slot, "json"));
            temporary = null;
            var restored = ReadFile(PathFor(slot, "json"), slot);
            return restored.Record is null ? new(false, "save-publication-uncertain") : new(true, null, restored.Record);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or NotSupportedException)
        { return new(false, attempted ? "save-publication-uncertain" : "save-storage-unavailable"); }
        finally { if (temporary is not null) try { File.Delete(temporary); } catch (IOException) { } catch (UnauthorizedAccessException) { } }
    }

    private (OpeningSaveRecord? Record, string? Error) ReadFile(string path, int slot)
    {
        try
        {
            RequireNoLinks();
            if (!File.Exists(path)) return (null, null);
            // Bound the allocation even if another process changes the file after its length is read.
            using var input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (input.Length is <= 0 or > OpeningSaveJson.MaximumBytes) return (null, "save-size");
            byte[] bytes = new byte[(int)input.Length]; input.ReadExactly(bytes);
            if (input.ReadByte() != -1) return (null, "save-size");
            var record = OpeningSaveJson.Read(bytes, slot);
            string? error = _validate(record.Data);
            return error is null ? (record, null) : (null, error);
        }
        catch (SaveFormatException error) { return (null, error.Code); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or NotSupportedException)
        { return (null, "save-storage-unavailable"); }
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
