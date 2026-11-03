using System.Text.Json;
using System.Text.RegularExpressions;

namespace Sf2.Remake.GodotAdapter.Input;

// One preferences file, separate from game progress; no OS path fallback.
internal sealed class SettingsStore
{
    private readonly string _directory;
    private string FilePath => Path.Combine(_directory, "input.json");
    private byte[]? _loaded;
    private bool _readFailed;
    internal bool NeedsReset { get; private set; }
    internal string? Error { get; private set; }

    internal SettingsStore(string root, string space, string worktree)
    {
        if (string.IsNullOrWhiteSpace(root) || !Path.IsPathFullyQualified(root) ||
            !Regex.IsMatch(space, "^[A-Za-z0-9_-]{1,64}$", RegexOptions.CultureInvariant))
            throw new ArgumentException("Preferences need a configured absolute persistent root and namespace.");
        root = Path.GetFullPath(root);
        string checkout = Path.TrimEndingDirectorySeparator(Path.GetFullPath(worktree));
        if (root.Equals(checkout, StringComparison.OrdinalIgnoreCase) || root.StartsWith(checkout + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Preferences must live outside the game checkout.");
        for (var parent = new DirectoryInfo(root); parent is not null; parent = parent.Parent)
            if (parent.Exists && (File.Exists(Path.Combine(parent.FullName, ".git")) || Directory.Exists(Path.Combine(parent.FullName, ".git"))))
                throw new ArgumentException("Preferences must live outside Git checkouts.");
        _directory = Path.Combine(root, space, "sf2-preferences");
    }

    internal InputSettings Load()
    {
        try
        {
            RequireNoLinks();
            _loaded = ReadBytes();
            if (_loaded is null) return InputSettings.Load(null);
            var settings = InputSettings.Parse(_loaded);
            _ = GameInput.Prepare(settings);
            return settings;
        }
        catch (Exception error) when (error is JsonException or ArgumentException)
        {
            NeedsReset = true;
            Error = "Saved preferences are invalid. Your file is retained. Restore defaults, then Apply to recover; Cancel keeps it.";
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            _readFailed = true;
            Error = "Saved preferences could not be read. Current defaults are in use. Your file is retained; check storage and relaunch.";
        }
        return InputSettings.Load(null);
    }

    internal string? Write(InputSettings candidate, bool reset)
    {
        if (_readFailed) return Error;
        if (NeedsReset && !reset) return "Restore defaults first, then Apply to explicitly recover the retained invalid file.";
        string? temporary = null;
        try
        {
            RequireNoLinks();
            Directory.CreateDirectory(_directory);
            RequireNoLinks();
            using var writer = new FileStream(Path.Combine(_directory, "input.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            var current = ReadBytes();
            if ((current is null) != (_loaded is null) || current is not null && !current.AsSpan().SequenceEqual(_loaded))
                return "Preferences changed outside this game. Your file and current settings are retained; relaunch before applying.";
            byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(candidate, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true });
            temporary = Path.Combine(_directory, $"input.{Guid.NewGuid():N}.tmp");
            using (var output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
            { output.Write(bytes); output.Flush(true); }
            if (current is null) File.Move(temporary, FilePath);
            else File.Replace(temporary, FilePath, NeedsReset ? Path.Combine(_directory, $"input.{Guid.NewGuid():N}.invalid.json") : null);
            temporary = null; _loaded = bytes; NeedsReset = false; Error = null;
            return null;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or NotSupportedException)
        { return "Preferences could not be saved. Your previous file and current settings are retained. Check storage and try again."; }
        finally
        {
            if (temporary is not null)
                try { File.Delete(temporary); } catch (IOException) { } catch (UnauthorizedAccessException) { }
        }
    }

    private byte[]? ReadBytes()
    {
        try
        {
            using var stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (stream.Length > 65536) throw new IOException("Preferences exceed 64 KiB.");
            byte[] bytes = new byte[(int)stream.Length]; stream.ReadExactly(bytes);
            if (stream.ReadByte() != -1) throw new IOException("Preferences changed during read.");
            return bytes;
        }
        catch (FileNotFoundException) { return null; }
        catch (DirectoryNotFoundException) { return null; }
    }

    private void RequireNoLinks()
    {
        for (var parent = new DirectoryInfo(_directory); parent is not null; parent = parent.Parent)
            if (parent.Exists && (parent.Attributes & FileAttributes.ReparsePoint) != 0) throw new IOException("Preferences directory link.");
        if (Directory.Exists(_directory) && Directory.EnumerateFileSystemEntries(_directory)
            .Any(path => (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)) throw new IOException("Preferences file link.");
    }
}
