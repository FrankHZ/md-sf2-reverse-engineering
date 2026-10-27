using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class BattleStoryReader
{
    internal static BattleStoryDefinition? Read(JsonElement root, IEnumerable<BattleDefinition> battles)
    {
        if (!root.TryGetProperty("battleStory", out var input))
        {
            Require(battles.All(b => b.Initialization?.RegionPrograms.Count is null or 0), "missing-battle-story", "battleStory");
            return null;
        }
        Object(input, "battleStory", "programs", "texts");
        var programs = new Dictionary<string, StoryProgram>();
        foreach (var row in Array(input, "programs"))
        {
            Object(row, "program", "id", "instructions");
            var instructions = Array(row, "instructions").Select(ExplorationContentReader.ReadInstruction).ToArray();
            Require(instructions.Length is > 0 and <= 4096 && instructions[^1] is EndProgram or ReturnProgram or JumpProgram or UnsupportedInstruction,
                "program-length-or-fallthrough", "battleStory.programs");
            string id = Id(row, "id");
            Require(programs.TryAdd(id, new(id, instructions, entitiesRunning: false)), "duplicate-program", "battleStory.programs");
        }
        var texts = new Dictionary<int, string>();
        foreach (var row in Array(input, "texts"))
        {
            Object(row, "text", "id", "text"); string text = Text(row, "text");
            Require(text.Length is > 0 and <= 8192, "text-length", "battleStory.texts");
            Require(texts.TryAdd(Number(row, "id", 0, 65535), text), "duplicate-text", "battleStory.texts");
        }
        void Target(ProgramLocation target) => Require(programs.TryGetValue(target.Program, out var program) &&
            target.Instruction >= 0 && target.Instruction < program.Instructions.Count, "unresolved-program-target", "battleStory.programs");
        foreach (var instruction in programs.Values.SelectMany(p => p.Instructions))
            switch (instruction)
            {
                case CallProgram call: Target(call.Target); break;
                case JumpProgram jump: Target(jump.Target); break;
                case BranchFlag branch: Target(branch.Target); break;
                case ShowText { TextId: { } text }: Require(texts.ContainsKey(text), "missing-dialogue-text", "battleStory.texts"); break;
            }
        foreach (var battle in battles)
            foreach (var row in battle.Initialization?.RegionPrograms ?? []) Target(new(row.Program, 0));
        return new(programs.Values, texts);
    }

    internal static (int[] Flags, BattleVisualState? Visuals, int[] Retained) Start(JsonElement start, BattleDefinition battle)
    {
        if (!start.TryGetProperty("story", out var input)) return ([], null, []);
        Object(input, "start.story", "flags", "aliases", "visuals", "retainedSourceSlots");
        int[] Numbers(string name, int maximum) => Array(input, name).Select(v =>
        {
            Require(v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out _) && v.GetInt32() >= 0 && v.GetInt32() <= maximum, "numeric-range", "start.story." + name);
            return v.GetInt32();
        }).ToArray();
        var aliases = new Dictionary<ActorRef, int>();
        foreach (var row in Array(input, "aliases"))
        {
            Object(row, "alias", "actor", "slot"); var actor = new ActorRef(Id(row, "actor"));
            Require(battle.Deployments.Any(d => d.Actor == actor), "missing-actor", "start.story.aliases");
            // Retained invalid bytes are admitted as supplied state and fail only when used.
            Require(aliases.TryAdd(actor, Number(row, "slot", 0, 255)), "duplicate-alias", "start.story.aliases");
        }
        var visuals = new Dictionary<int, BattleVisual>();
        foreach (var row in Array(input, "visuals"))
        {
            Object(row, "visual", "slot", "appearance", "x", "y", "facing");
            int slot = Number(row, "slot", 0, 48); var actor = new ActorRef(Id(row, "appearance"));
            Require(battle.Deployments.Any(d => d.Actor == actor), "missing-actor", "start.story.visuals");
            Require(visuals.TryAdd(slot, new(slot, actor, new(Number(row, "x", 0, 47), Number(row, "y", 0, 47)),
                (byte)Number(row, "facing", 0, 3))), "duplicate-visual", "start.story.visuals");
        }
        Require(visuals.Values.Select(v => v.Appearance).Distinct().Count() == visuals.Count,
            "duplicate-visual-appearance", "start.story.visuals", true);
        return (Numbers("flags", 65535), new(aliases, visuals.Values), Numbers("retainedSourceSlots", 159));
    }
}
