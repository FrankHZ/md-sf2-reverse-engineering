using System.Reflection;
using System.Text.Json;
using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.GodotAdapter.Battles;
using Sf2.Remake.GodotAdapter.Input;

namespace Sf2.Remake.GodotAdapter.Probes;

public partial class BattleRasterObservation : Node
{
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

    public override void _Ready()
    {
        try
        {
            Observe();
            GD.Print("BATTLE-RASTER-OBSERVATION PASS");
            GetTree().Quit();
        }
        catch (Exception error) { GD.PushError(error.ToString()); GetTree().Quit(1); }
    }

    private void Observe()
    {
        var path = ProjectSettings.GlobalizePath("res://../content/authored/early-ranged.json");
        var authored = (ScenarioReadAccepted)new AuthoredScenarioPackageReader(path).Read();
        var originals = new Dictionary<string, ExplorationRaster>();
        var variants = new Dictionary<string, ExplorationRaster>();
        void Add(string key, int width, int height, int density = 0)
        {
            originals.Add(key, Raster(width, height, Colors.Red));
            if (density > 0)
                variants.Add(key, Raster(width * density, height * density, Colors.Blue, density));
        }

        Add("body/idle", 96, 96, 4);
        Add("body/strike", 192, 192, 2);
        Add("foe/idle", 192, 192, 2);
        Add("foe/strike", 192, 192);
        Add("weapon/idle", 64, 64, 4);
        Add("weapon/strike", 128, 128, 2);
        Add("backdrop", 512, 128, 2);
        Add("ground", 240, 48, 2);
        string[] exits = ["exit/down", "exit/side", "exit/up"];
        foreach (string exit in exits) Add(exit, 48, 24, 4);

        var idle = new BattleSceneWeaponFrame(0, 1, 4, 8);
        var strike = new BattleSceneWeaponFrame(49, 2, 7, 12);
        var sequences = new Dictionary<string, BattleSceneAnimation>
        {
            ["idle"] = new(0, 0, 0, false, idle, [new(0, 6, 0, 0, idle)]),
            ["attack"] = new(1, 0, 0, false, null, [new(0, 6, 0, 0, idle), new(1, 12, 3, 0, strike)]),
            ["dodge"] = new(2, 0, 0, false, null, [new(0, 6, 0, 0, null)]),
        };
        var lance = new BattleSceneActorVisual(0, 0, 72, ["body/idle", "body/strike"], ["weapon/idle", "weapon/strike"], sequences);
        var sentry = new BattleSceneActorVisual(1, 1, null, ["foe/idle", "foe/strike"], [],
            sequences.ToDictionary(row => row.Key, row => row.Value with
            { IdleWeapon = null, Frames = row.Value.Frames.Select(frame => frame with { Weapon = null }).ToArray() }));
        var content = new BattleSceneDefinition(authored.Start.Encounter, "backdrop", "ground", originals,
            new Dictionary<BattleClassRule, BattleSceneActorVisual> { [BattleClassRule.UnpromotedKnight] = lance },
            new Dictionary<ActorRef, BattleSceneActorVisual> { [new("sentinel")] = sentry },
            new Dictionary<int, string>(), [],
            FieldDeath: new(new Dictionary<int, int>(), new Dictionary<ActorRef, int>(), exits),
            HdRasters: variants);
        var definition = (ScenarioDefinition)Activator.CreateInstance(typeof(ScenarioDefinition), Private, null,
            ["battle-raster-observation", authored.Definition.Encounters.Values, null, null, content, null, null, null,
                BattleStartOrigin.Controlled], null)!;
        var session = ((SessionStarted)GameSession.Start(definition, authored.Start)).Session;
        var settings = new InputSettings { FormatVersion = 1, TextMode = "instant", ReducedFlash = true };
        var input = new GameInput(settings);
        var owner = new Control();
        AddChild(owner);
        var view = new BattleSceneView { PresentationRoot = owner };
        owner.AddChild(view);
        view.Configure(session, input, null);
        view.Present();
        ObserveEmptyPreparation(view, content, originals, settings, session);

        Submit(session, new Confirm());
        Submit(session, new ChooseAction(SessionAction.PhysicalAttack));
        Submit(session, new SelectTarget(new("sentinel")));
        Submit(session, new Confirm());
        view.Present();
        Check(view.Error is null, view.Error ?? "");
        var ally = view.GetNode<Sprite2D>("SceneCanvas/Ally");
        var foe = view.GetNode<Sprite2D>("SceneCanvas/Enemy");
        var weapon = view.GetNode<Sprite2D>("SceneCanvas/Weapon");
        var background = view.GetNode<Sprite2D>("SceneCanvas/Background");
        var wrap = view.GetNode<Sprite2D>("SceneCanvas/BackgroundWrap");
        var ground = view.GetNode<Sprite2D>("SceneCanvas/Ground");
        var originalTexture = ally.Texture;
        var action = session.Current.BattleScene;
        Check(action is not null && weapon.Visible && weapon.ZIndex < ally.ZIndex, "ordinary input starts rear weapon scene");

        foreach (string mode in new[] { "original", "hd", "original" })
        {
            view.PrepareGraphics(mode, content);
            settings.GraphicsMode = mode;
            view.PublishGraphics();
            bool hd = mode == "hd";
            Expect(ally, 96, 96, hd ? 4 : 1, "native-size ally footprint");
            Expect(foe, 96, 96, 2, "incomplete enemy group falls back as a whole");
            Expect(weapon, 64, 64, hd ? 4 : 1, "native-size weapon footprint");
            Expect(background, 256, 64, hd ? 4 : 2, "scenery footprint");
            Expect(ground, 120, 24, hd ? 4 : 2, "ground footprint");
            Check(background.Texture == wrap.Texture && wrap.Position.X - background.Position.X == 256, "scenery wrap");
            Check(ReferenceEquals(session.Current.BattleScene, action), "mode publish leaves the wait token and state");
        }
        owner.RemoveChild(view);
        owner.AddChild(view);
        Check(ally.Texture == originalTexture && originalTexture.GetWidth() == 96, "borrow survives mode switch and reparent");

        using var temporary = Image.CreateEmpty(384, 384, false, Image.Format.Rgba8);
        variants["foe/strike"] = Encoded(384, 384, "png", temporary.SavePngToBuffer()[..33]);
        try
        {
            view.PrepareGraphics("hd", content);
            throw new Exception("declared broken image was accepted");
        }
        catch (InvalidOperationException error) when (error.Message == "raster-decode")
        {
            Check(settings.GraphicsMode == "original" && ally.Texture == originalTexture, "failed prepare keeps current binding");
        }
        variants.Remove("foe/strike");

        for (int step = 0; session.Current.BattleScene is { } state; step++)
        {
            Check(step < 40, "physical close-up completes");
            if (state.Phase == BattleScenePhase.ActionAnimation)
            {
                view.PrepareGraphics("hd", content);
                settings.GraphicsMode = "hd";
                view.PublishGraphics();
                _ = view.Consume(0.06);
                Expect(ally, 96, 96, 4, "double-density ally strike footprint");
                Expect(weapon, 64, 64, 4, "double-density weapon strike footprint");
                Check(weapon.FlipH && weapon.FlipV && weapon.ZIndex > ally.ZIndex, "front weapon and both flip bits");
                settings.GraphicsMode = "original";
                view.PublishGraphics();
                Expect(weapon, 64, 64, 2, "original double-density weapon footprint");
            }
            Submit(session, state.RequiresAcknowledgement ? new Acknowledge(state.Token) :
                new CompletePresentation(state.Token, state.CompletionKind));
            view.Present();
            Check(view.Error is null, view.Error ?? "");
        }
        Check(ally.Texture is null && weapon.Texture is null, "return retires close-up borrowers");

        var map = new BattleMapViewport();
        owner.AddChild(map);
        typeof(BattleMapViewport).GetField("_sceneContent", Private)!.SetValue(map, content);
        var assign = typeof(BattleMapViewport).GetMethod("SetSpriteTexture", Private)!;
        var sprite = new Sprite2D();
        foreach (bool hd in new[] { false, true, false })
        {
            map.SelectTerrain(hd ? "hd" : "original");
            assign.Invoke(map, [sprite, 63, 1, 1]);
            int size = hd ? 96 : 24;
            Check(sprite.Texture.GetWidth() == size && sprite.Texture.GetHeight() == size, "exit direction crops one half");
            Check(sprite.Texture.GetWidth() * sprite.Scale.X == Maps.WorldViewportLayout.CellSize, "exit logical footprint");
            using var image = sprite.Texture.GetImage();
            int density = hd ? 4 : 1;
            Check(image.GetPixel(0, 0) == Colors.Transparent && image.GetPixel(density, density) == Colors.Black &&
                image.GetPixel(size - 1, size - 1) == (hd ? Colors.Blue : Colors.Red), "exit crop keeps alpha and opaque black");
        }
        sprite.Free();
        owner.Free();
        ObserveEffectRegions();
    }

    private static void ObserveEffectRegions()
    {
        using var originalImage = Image.CreateEmpty(24, 16, false, Image.Format.Rgba8);
        using var hdImage = Image.CreateEmpty(96, 64, false, Image.Format.Rgba8);
        originalImage.Fill(Colors.Green); hdImage.Fill(Colors.Yellow);
        using var originalTexture = ImageTexture.CreateFromImage(originalImage);
        using var hdTexture = ImageTexture.CreateFromImage(hdImage);
        bool hd = false;
        BattleSceneRaster Frame(int _) => new(hd ? hdTexture : originalTexture, new(24, 16), hd);
        var fire = new BlazeMeteorEffect(Frame, false, true, 2);
        var wind = new BlastEffect(Frame, true, true);
        foreach (Func<double, object> observe in new Func<double, object>[] { fire.Observe, wind.Observe })
        {
            hd = false;
            var baseline = JsonSerializer.SerializeToElement(observe(0.24));
            hd = true;
            var upgraded = JsonSerializer.SerializeToElement(observe(0.24));
            var left = baseline.GetProperty("sprites").EnumerateArray().ToArray();
            var right = upgraded.GetProperty("sprites").EnumerateArray().ToArray();
            Check(left.Length > 0 && left.Length == right.Length &&
                baseline.GetProperty("duration").GetDouble() == upgraded.GetProperty("duration").GetDouble(),
                "effect count and duration across densities");
            for (int index = 0; index < left.Length; index++)
            {
                Check(right[index].GetProperty("textureWidth").GetInt32() == 96 &&
                    right[index].GetProperty("textureHeight").GetInt32() == 64, "effect uses dense texture");
                foreach (var property in left[index].EnumerateObject())
                    if (property.Name is not ("textureWidth" or "textureHeight" or "hd"))
                        Check(property.Value.GetRawText() == right[index].GetProperty(property.Name).GetRawText(),
                            "effect logical geometry, frame, flip and opacity across densities");
            }
        }
    }

    private static void Submit(GameSession session, SessionCommand command)
    {
        var before = session.Current;
        var result = session.Submit(new(before.SessionId, before.Revision, before.Selection?.Actor, command));
        Check(result.Failure is null, result.Failure?.Code ?? "");
    }

    private static void ObserveEmptyPreparation(BattleSceneView view, BattleSceneDefinition content,
        Dictionary<string, ExplorationRaster> originals, InputSettings settings, GameSession session)
    {
        using var image = Image.CreateEmpty(192, 192, false, Image.Format.Rgba8);
        var broken = Encoded(192, 192, "png", image.SavePngToBuffer()[..33]);
        var before = session.Current;
        var sprite = view.GetNode<Sprite2D>("SceneCanvas/Ally");
        Check(sprite.Texture is null, "movement preparation has no close-up bindings");
        void Reject(string mode)
        {
            string previous = settings.GraphicsMode;
            try
            {
                view.PrepareGraphics(mode, content);
                throw new Exception("empty-view preparation accepted broken selected Original");
            }
            catch (InvalidOperationException error) when (error.Message == "raster-decode")
            {
                Check(settings.GraphicsMode == previous && sprite.Texture is null &&
                    ReferenceEquals(session.Current, before), "rejected prepare retains mode, display and state");
            }
        }
        var strike = originals["body/strike"];
        originals["body/strike"] = broken;
        settings.GraphicsMode = "hd";
        view.PrepareGraphics("hd", content);
        Reject("original");
        originals["body/strike"] = strike;
        var foe = originals["foe/strike"];
        originals["foe/strike"] = broken;
        settings.GraphicsMode = "original";
        Reject("hd");
        Reject("original");
        originals["foe/strike"] = foe;
        view.PrepareGraphics("original", content);
    }

    private static void Expect(Sprite2D node, int width, int height, int density, string label)
    {
        Check(node.Texture.GetSize() == new Vector2(width, height) * density &&
            node.GetRect().Size * node.Scale == new Vector2(width, height), label);
    }

    private static ExplorationRaster Raster(int width, int height, Color color, int density = 1)
    {
        using var image = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
        image.Fill(color);
        image.FillRect(new(width / 2, 0, density, density), Colors.Transparent);
        image.FillRect(new(width / 2 + density, density, density, density), Colors.Black);
        return Encoded(width, height, "rgba8", image.GetData());
    }

    private static ExplorationRaster Encoded(int width, int height, string format, byte[] bytes) =>
        (ExplorationRaster)Activator.CreateInstance(typeof(ExplorationRaster), Private, null, [width, height, format, bytes], null)!;

    private static void Check(bool value, string label)
    {
        if (!value) throw new InvalidOperationException(label);
    }
}
