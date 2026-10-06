using System.Numerics;
using static H4Comparison.Operands;
using static H4Comparison.SceneOperands;

namespace H4Comparison;

internal sealed class SceneObservations(CountedChecks checks, object? scene, int digitLimit)
{
    private void Check(string name, object? value) => checks.Check("scene", name, value, BigInteger.One);
    private void Evaluated(string name, Action action) => checks.Evaluated("scene", name, action);

    public void Accept(object? row) => Evaluated("scene occurrence", () =>
    {
        var state = At(row, "scene");
        var healing = Get(state, "healing");
        var fairy = Get(Truth(healing) ? healing : Dict(), "Fairy");
        if (Truth(Get(state, "visible")) && Truth(fairy) && Truth(Get(fairy, "Control")))
        {
            var needed = Dict();
            var index = 0;
            foreach (var instance in Iterate(At(fairy, "Fairies")))
            {
                var i = index++;
                Evaluated("fairy instance", () =>
                {
                    if (!Truth(At(instance, "Active"))) return;
                    needed["FairyBody" + i] = Index(At(At(scene, "healing"), "bodies"), Int(At(instance, "BodyFrame"), digitLimit));
                    needed["FairyWings" + i] = Index(At(At(scene, "healing"), "wings"), Int(At(instance, "WingFrame"), digitLimit));
                });
            }
            index = 0;
            foreach (var dust in Iterate(At(fairy, "Dust")))
            {
                var i = index++;
                Evaluated("fairy dust", () =>
                {
                    if (Truth(At(dust, "Age")))
                        needed["FairyDust" + i] = Index(At(At(scene, "healing"), "dust"), Int(At(dust, "Frame"), digitLimit));
                });
            }
            var mounted = Mounted(Default(state, "fairySprites", new List<object?>()), "name");
            foreach (var (name, resource) in needed)
            {
                var (mount, found) = Lookup(mounted, name);
                var node = Get(found ? mount : Dict(), "binding");
                object? value = null;
                if (node is not null)
                {
                    value = Equal(Get(node, "resource"), resource);
                    if (Truth(value)) value = Get(node, "texturePresent");
                    if (Truth(value)) value = Get(node, "visible");
                }
                Check("required fairy mounted texture " + name, value);
            }
        }
        var death = Get(state, "fieldDeath");
        if (!Truth(death)) return;
        var actors = Get(row, "fieldActors");
        Check("actual field-death consumer channel", actors is not null ? true : null);
        var phase = At(state, "phase");
        if (Equal(phase, "FieldSpin") || Equal(phase, "FieldExit"))
        {
            var mounted = Mounted(Truth(actors) ? actors : new List<object?>(), "id", "sprite");
            foreach (var dead in Iterate(At(death, "actors")))
            {
                var (node, _) = Lookup(mounted, dead);
                object? value = null;
                if (node is not null)
                {
                    value = Get(node, "visible");
                    if (Truth(value)) value = Get(node, "texturePresent");
                }
                Check("required dead actor remains projected during its source effect", value);
            }
        }
        foreach (var actor in Iterate(Truth(actors) ? actors : new List<object?>()))
            Evaluated("field actor", () =>
            {
                var sprite = Get(actor, "sprite");
                if (!Truth(sprite) || !Truth(Get(sprite, "visible"))) return;
                var selector = Get(sprite, "resourceSelector");
                var facing = At(sprite, "facing");
                var direction = Equal(facing, BigInteger.One) ? 0 : Equal(facing, new BigInteger(3)) ? 2 : 1;
                object? ally = null;
                foreach (var entry in Iterate(At(At(scene, "fieldDeath"), "allies")))
                    if (Equal(At(actor, "id"), "ally-" + Str(At(entry, "character"))))
                    { ally = At(entry, "sprite"); break; }
                var original = ally ?? At(Index(At(At(scene, "fieldDeath"), "enemies"), BigInteger.Zero), "sprite");
                var actorId = At(actor, "id");
                var expected = Contains(At(death, "actors"), actorId) && Equal(At(state, "phase"), "FieldExit")
                    ? new BigInteger(63) : original;
                object? outcome = null;
                if (selector is not null)
                {
                    outcome = Equal(selector, Dict(("sprite", expected), ("direction", new BigInteger(direction)),
                        ("frame", At(sprite, "walkingFrame")),
                        ("raster", Equal(expected, new BigInteger(63))
                            ? Index(At(At(scene, "fieldDeath"), "exitFrames"), new BigInteger(direction)) : null)));
                    if (Truth(outcome)) outcome = Get(sprite, "texturePresent");
                    if (Truth(outcome)) outcome = Get(sprite, "visibleInTree");
                }
                Check("actual field-death assigned texture", outcome);
            });
    });
}
