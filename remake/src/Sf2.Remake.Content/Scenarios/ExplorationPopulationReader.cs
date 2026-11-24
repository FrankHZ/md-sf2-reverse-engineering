using System.Collections.ObjectModel;
using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class ExplorationPopulationReader
{
    internal static ExplorationPopulation Read(JsonElement row)
    {
        ObjectOptional(row, "population", "allySprites", "allyCount", "nonAllyStart", "playerSprite", "followers");
        var followers = Array(row, "followers").Select(follower =>
        {
            Object(follower, "follower", "flag", "character", "sprite");
            return new ExplorationFollowerDefinition(Number(follower, "flag", 0, 65535),
                Number(follower, "character", 0, 255), Number(follower, "sprite", 0, 255));
        }).ToArray();
        var allies = row.TryGetProperty("allySprites", out _)
            ? System.Array.AsReadOnly(Array(row, "allySprites").Select(Ally).ToArray()) : null;
        return new(Number(row, "allyCount", 1, 128), Number(row, "nonAllyStart", 128, 255),
            Number(row, "playerSprite", 0, 255), System.Array.AsReadOnly(followers), allies);
    }

    private static ExplorationAllySprite Ally(JsonElement appearance)
    {
        ObjectOptional(appearance, "allySprite", ["character", "sprite", "joinedFlag", "unjoinedSprite"],
            "promotionClass", "promotionSprite", "promotionSprites");
        int? joined = NullableNumber("joinedFlag", 65535), unjoined = NullableNumber("unjoinedSprite", 255);
        Require((joined is null) == (unjoined is null), "ally-sprite-override", "population.allySprites");
        bool scalar = appearance.TryGetProperty("promotionClass", out _);
        bool variants = appearance.TryGetProperty("promotionSprites", out _);
        Require(scalar == appearance.TryGetProperty("promotionSprite", out _) && !(scalar && variants),
            "ally-promotion-sprite", "population.allySprites");
        Dictionary<byte, int> sprites = [];
        if (scalar) sprites.Add((byte)Number(appearance, "promotionClass", 0, 31), Number(appearance, "promotionSprite", 0, 255));
        if (variants)
            foreach (var variant in Array(appearance, "promotionSprites"))
            {
                Object(variant, "promotionSprite", "classId", "sprite");
                Require(sprites.TryAdd((byte)Number(variant, "classId", 0, 31), Number(variant, "sprite", 0, 255)),
                    "ally-promotion-sprite", "population.allySprites");
            }
        return new(Number(appearance, "character", 0, 255), Number(appearance, "sprite", 0, 255), joined, unjoined,
            sprites.Count == 0 ? null : new ReadOnlyDictionary<byte, int>(sprites));

        int? NullableNumber(string key, int maximum) =>
            appearance.GetProperty(key).ValueKind == JsonValueKind.Null ? null : Number(appearance, key, 0, maximum);
    }
}
