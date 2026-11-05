using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Content.Scenarios;

// Bounded services-world admission reproduction, not a general package copier.
var watch = Stopwatch.StartNew();
int exitCode = 1;
long scratchBytes = 0;
try
{
    if (args is ["--help"])
    {
        Console.WriteLine("--repository <checkout> --package <services-package> --scratch <new-local-directory>");
        Console.WriteLine("Package and scratch paths are relative to repository; repository is relative to cwd.");
        exitCode = 0;
        return 0;
    }
    if (args.Length != 6 || args[0] != "--repository" || args[2] != "--package" || args[4] != "--scratch")
        throw new ArgumentException("usage: --repository <checkout> --package <services-package> --scratch <new-local-directory>");
    string repository = Path.GetFullPath(args[1]);
    if (!Path.Exists(Path.Combine(repository, ".git")) ||
        !File.Exists(Path.Combine(repository, "remake/tools/Sf2.Remake.PrivateAdmission/Sf2.Remake.PrivateAdmission.csproj")))
        throw new ArgumentException("repository must be a checkout containing this maintained tool");
    string package = Path.GetFullPath(args[3], repository);
    string scratch = Path.GetFullPath(args[5], repository);
    string local = Path.Combine(repository, "local");
    if (!Within(scratch, local) || Within(scratch, package) || Within(package, scratch) || scratch == package)
        throw new ArgumentException("scratch must be beneath repository/local and separate from the input package");
    // A local-looking junction must not redirect writes into shared inputs or another checkout.
    for (var ancestor = new DirectoryInfo(scratch); ancestor is not null; ancestor = ancestor.Parent)
        if (ancestor.Exists && (ancestor.Attributes & FileAttributes.ReparsePoint) != 0)
            throw new ArgumentException("scratch ancestors must not be links or junctions");
    if (Path.Exists(scratch)) throw new ArgumentException("scratch must be a fresh, nonexistent directory");

    string world = Path.Combine(package, "world.json");
    var party = new PrivateBattleScenarioReader(Input("BATTLE01_DATA"), Input("BATTLE01_SCENE"),
        Input("BATTLE01_TERRAIN"), Input("STATIC_DATA"), Input("ENEMY_DATA"), Input("ENEMY_GOLD"), Input("CONTROLLED_START"));
    // These are exactly the eight dependencies of the supported, already prepared package.
    // Preserve bytes and world-relative names; never rewrite dependency selectors or copy a tree.
    Dictionary<string, string> dependencies = new()
    {
        ["placement"] = "battle02/placement.json", ["scene"] = "battle02/scene.json",
        ["terrain"] = "battle02/terrain.bin", ["start"] = "battle02-party.json",
        ["static"] = "accepted-inputs/SF2_PRIVATE_STATIC_DATA.json",
        ["enemies"] = "accepted-inputs/SF2_PRIVATE_ENEMY_DATA.json",
        ["gold"] = "accepted-inputs/SF2_PRIVATE_ENEMY_GOLD.json",
        ["battleScenes"] = "scene02-field-01/battle-scenes.json",
    };
    CheckLayout(world, dependencies);
    foreach (string relative in dependencies.Values)
        if (!File.Exists(Path.Combine(package, relative)))
            throw new ArgumentException("missing package dependency: " + relative);
    Directory.CreateDirectory(scratch);
    foreach (string relative in dependencies.Values)
    {
        string destination = Path.Combine(scratch, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        File.Copy(Path.Combine(package, relative), destination, overwrite: false);
        scratchBytes += new FileInfo(destination).Length;
    }
    long dependencyBytes = scratchBytes;
    Console.WriteLine($"inputBytes={new FileInfo(world).Length}; dependencyBytes={dependencyBytes}");
    foreach (string expected in new[] { "accepted", "world-rom-identity", "raster-identity", "missing-sprite-visual", "document-size" })
    {
        Probe(expected, world, scratch, repository, party, dependencyBytes);
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
    }
    exitCode = 0;
}
catch (Exception error)
{
    // No raw exception paths/content: selected inputs and scratch are private.
    Console.Error.WriteLine($"failed={error.GetType().Name}; detail={(error is ArgumentException ? error.Message : "input/layout/read or admission mismatch; see case output")}");
}
finally
{
    Console.WriteLine($"exit={exitCode}; seconds={watch.Elapsed.TotalSeconds:F2}; peakWorkingSetBytes={Process.GetCurrentProcess().PeakWorkingSet64}; scratchBytes={scratchBytes}");
}
return exitCode;

static bool Within(string path, string directory) => path.StartsWith(
    Path.TrimEndingDirectorySeparator(directory) + Path.DirectorySeparatorChar,
    OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

static string Input(string key)
{
    string name = "SF2_PRIVATE_" + key;
    string? value = Environment.GetEnvironmentVariable(name);
    if (string.IsNullOrWhiteSpace(value) || !Path.IsPathFullyQualified(value) || !File.Exists(value))
        throw new ArgumentException(name + " must select an existing absolute read-only input");
    return value;
}

static void CheckLayout(string world, Dictionary<string, string> dependencies)
{
    using var stream = File.OpenRead(world);
    using var document = JsonDocument.Parse(stream, new JsonDocumentOptions { MaxDepth = 32 });
    var root = document.RootElement;
    var encounters = root.GetProperty("encounters");
    if (encounters.GetArrayLength() != 1 || encounters[0].EnumerateObject().Count() != dependencies.Count ||
        dependencies.Any(pair => encounters[0].GetProperty(pair.Key).GetString() != pair.Value))
        throw new ArgumentException("unsupported package: expected the services-world encounter layout");
    if (root.GetProperty("world").GetProperty("presentation").TryGetProperty("audio", out var audio) &&
        audio.EnumerateArray().Any(row => row.TryGetProperty("runtimeWav", out _)))
        throw new ArgumentException("unsupported package: external runtime WAV inputs are outside this entry");
}

void Probe(string expected, string world, string scratch, string repository,
    PrivateBattleScenarioReader party, long dependencyBytes)
{
    var node = JsonNode.Parse(File.ReadAllText(world))!;
    if (expected == "world-rom-identity") node["provenance"]!["romSha256"] = new string('0', 64);
    if (expected == "raster-identity") node["world"]!["presentation"]!["maps"]![0]!["atlas"]!["data"] = "AAAA";
    if (expected == "missing-sprite-visual")
    {
        var sprites = node["world"]!["presentation"]!["sprites"]!.AsArray();
        sprites.Remove(sprites.Single(sprite => sprite!["sprite"]!.GetValue<int>() == 209));
    }
    // JSON-file output only; retain default escaping solely for the real oversized control.
    var options = expected == "document-size" ? null : new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
    string mutation = Path.Combine(scratch, "mutation.json");
    File.WriteAllText(mutation, node.ToJsonString(options));
    long bytes = new FileInfo(mutation).Length;
    scratchBytes = dependencyBytes + bytes;
    string start = expected == "world-rom-identity" ? "map3-sarah-start.json" : "map3-opening-start.json";
    var result = GameSession.Start(new PrivateExplorationReader(mutation,
        Path.Combine(repository, "remake/reference/inputs", start), party));
    string code = result is SessionStartFailed failed ? failed.Failure.Code : result is SessionStarted ? "accepted" : "unexpected-result";
    string kind = result is SessionStartFailed rejection ? rejection.Failure.Kind.ToString() : "none";
    Console.WriteLine($"expected={expected}; actual={code}; kind={kind}; bytes={bytes}");
    if (code != expected || result is SessionStartFailed failure && failure.Failure.Kind != SessionFailureKind.ContentError)
        throw new InvalidOperationException("Admission control failed");
}
