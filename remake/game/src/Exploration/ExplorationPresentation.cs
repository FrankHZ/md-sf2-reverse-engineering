using Godot;
using Sf2.Remake.GodotAdapter.Audio;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter.Exploration;

internal sealed class ExplorationPresentation : IDisposable
{
    private readonly Control _owner;
    private readonly ExplorationVisuals? _visuals;
    private readonly Dictionary<MapId, Image> _atlases = [];
    private readonly Dictionary<(MapId Map, int Block), ImageTexture> _blocks = [];
    private readonly Dictionary<(int Sprite, int Direction, int Half, bool Nod), ImageTexture> _sprites = [];
    private readonly Dictionary<(int Portrait, bool Mirror, bool Eyes, bool Mouth), ImageTexture> _portraits = [];
    private readonly Dictionary<ImageTexture, IReadOnlyList<Rect2>> _spriteInk = [];
    private readonly SessionAudio _audio;
    private readonly ColorRect _white;
    private readonly Action _prepareBattle;
    private readonly bool _reducedFlash;
    private int _spriteMounts;
    private WaitToken? _cue;
    private double _cueAge;
    private float _fadeStart;
    private EntityRef? _gesture;
    private bool _nodding;
    private bool _shivering;
    private EntityRef? _mosaic;
    private bool _mosaicOut;
    private MapId? _map;
    private Vector2 _camera;
    private Rect2 _screen;
    private float _scale;
    private const int ViewWidth = 320, ViewHeight = 192;

    internal ExplorationPresentation(Control owner, ExplorationDefinition definition, bool reducedFlash, Action prepareBattle, SessionAudio audio)
    {
        _owner = owner; _visuals = definition.Visuals; _reducedFlash = reducedFlash; _prepareBattle = prepareBattle;
        _audio = audio; Error = audio.Error;
        _white = new ColorRect { Name = "WhiteFade", Color = new Color(1, 1, 1, 0),
            MouseFilter = Control.MouseFilterEnum.Ignore, ZIndex = 100 };
        owner.AddChild(_white);
        _white.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
    }

    private string? _error;
    internal string? Error { get => _error ?? _audio.Error; private set => _error = value; }
    internal int SpriteMounts => _spriteMounts;
    internal object? NodProjection { get; private set; }
    internal object? CameraProjection { get; private set; }
    internal int GestureDraws { get; private set; }
    internal int NodDraws { get; private set; }
    internal int RestoredGestureDraws { get; private set; }
    internal int SoundStarts => _audio.Starts;
    internal int SoundFades { get; private set; }
    internal int SoundCompletions => _audio.Completions;
    internal int SoundStops => _audio.Stops;
    internal string? MusicCue => _audio.MusicCue;
    internal bool MusicPlaying => _audio.MusicPlaying;
    internal bool MusicFinished => _audio.MusicFinished;
    internal double MusicPosition => _audio.MusicPosition;
    internal int PaletteFades { get; private set; }
    internal int ShiverDraws { get; private set; }
    internal int MosaicDraws { get; private set; }
    internal int MosaicOutDraws { get; private set; }
    internal int BattleLoads { get; private set; }
    internal float WhiteOpacity => _white.Color.A;
    internal int SuppressedWhiteCues { get; private set; }
    internal long? CompletedCueToken { get; private set; }
    internal string? CompletedCueKind { get; private set; }
    internal float PaletteBrightness => _owner.Modulate.R;
    internal Vector2 Camera => _camera;
    internal Rect2 Screen => _screen;
    internal string? ActiveCue { get; private set; }

    internal IReadOnlyList<SessionCommand> Update(double delta, SessionSnapshot current)
    {
        List<SessionCommand> completions = [];
        if (Error is not null) return completions;
        try
        {
            var target = _camera;
            if (current.Exploration is { } world)
            {
                if (current.Story.LogicalView is { } view)
                    _camera = target = new Vector2(view.BX.Position / 16f, view.BY.Position / 16f);
                else
                {
                    var area = world.Definition.Traversal.SelectActiveArea(world.PlayerEntity.Position)!.Area;
                    var tracked = current.Story.CameraEntitySlot is { } slot
                        ? world.AllEntities.Single(entity => entity.Slot == slot) : world.PlayerEntity;
                    target = current.Story.CameraEntitySlot is null && current.Story.Cursor is not null && current.Story.CameraTarget is { } destination
                        ? new Vector2(destination.X * 24, destination.Y * 24)
                        : new Vector2(tracked.Motion.X / 16f - ViewWidth / 2f + 12,
                            tracked.Motion.Y / 16f - ViewHeight / 2f + 12);
                    target.X = Mathf.Clamp(target.X, area.MinimumX * 24, Math.Max(area.MinimumX * 24, (area.MaximumX + 1) * 24 - ViewWidth));
                    target.Y = Mathf.Clamp(target.Y, area.MinimumY * 24, Math.Max(area.MinimumY * 24, (area.MaximumY + 1) * 24 - ViewHeight));
                    if (_map != world.Map) { _map = world.Map; _camera = target; }
                    else _camera = _camera.MoveToward(target, (float)(delta * 180));
                }
                foreach (var entity in world.AllEntities.Where(entity => entity.WaitingForSprite && entity.SpriteReady != entity.SpriteRequest))
                {
                    _ = Sprite(entity, false);
                    _spriteMounts++;
                    completions.Add(new EntitySpriteReady(entity.Slot, entity.SpriteRequest));
                }
            }
            if (current.Story.Wait is NodWait nod)
            {
                _cue = nod.Token; _gesture = nod.Entity; _shivering = false; _mosaic = null;
                _nodding = nod.Lowered; ActiveCue = "Gesture";
                var actor = current.Exploration!.Entities[nod.Entity];
                // Mount the existing transformed/normal resources. Rendering never decides
                // source service progress, and no per-frame acknowledgement is needed.
                _ = Sprite(actor, true); _ = Sprite(actor, false);
                if (nod.LogicalDone && !nod.ActualDone && CompletedCueToken != nod.Token.Value)
                {
                    completions.Add(new CompletePresentation(nod.Token, PresentationCueKind.Gesture));
                    CompletedCueToken = nod.Token.Value; CompletedCueKind = "Gesture";
                }
                return completions;
            }
            var wait = current.Story.Wait switch
            {
                FullFadeWait fade => new PresentationWait(fade.Token, new(fade.Kind, "black")),
                PresentationWait presenting => presenting,
                _ => null,
            };
            if (wait is null)
            { _cue = null; _gesture = null; _mosaic = null; _shivering = _nodding = false; ActiveCue = null; return completions; }
            if (CompletedCueToken == wait.Token.Value) return completions;
            if (current.Exploration is null && wait.Cue.Kind is PresentationCueKind.CameraWait or PresentationCueKind.Gesture or PresentationCueKind.EntityEffect)
                throw new InvalidOperationException("presentation-map-unavailable");
            if (_cue != wait.Token)
            {
                _cue = wait.Token; _cueAge = 0; _gesture = null; _mosaic = null; _shivering = _nodding = false;
                ActiveCue = wait.Cue.Kind.ToString();
                if (wait.Cue.Kind is PresentationCueKind.FadeIn or PresentationCueKind.FadeOut)
                {
                    if (wait.Cue.Resource is not ("black" or "white")) throw new InvalidOperationException("fade-binding");
                    if (wait.Cue.Resource == "black")
                    {
                        if (wait.Cue.Kind == PresentationCueKind.FadeIn) _owner.Modulate = Colors.Black;
                        _fadeStart = _owner.Modulate.R;
                    }
                    else
                    {
                        _white.Color = new Color(1, 1, 1, !_reducedFlash && wait.Cue.Kind == PresentationCueKind.FadeIn ? 1 : 0);
                        if (_reducedFlash) SuppressedWhiteCues++;
                    }
                    _cueAge = -delta; // Present the initial palette before advancing the modern half-second service.
                }
                if (wait.Cue.Kind == PresentationCueKind.Gesture)
                {
                    if (wait.Cue.Resource is not ("nod" or "shiver") || wait.Cue.Entity is null) throw new InvalidOperationException("gesture-binding");
                    _gesture = wait.Cue.Entity;
                    _shivering = wait.Cue.Resource == "shiver";
                    _ = Sprite(current.Exploration!.Entities[_gesture.Value], !_shivering);
                }
                else if (wait.Cue.Kind == PresentationCueKind.EntityEffect)
                {
                    if (wait.Cue.Resource is not ("mosaic-in" or "mosaic-out") || wait.Cue.Entity is null) throw new InvalidOperationException("effect-binding");
                    _mosaic = wait.Cue.Entity;
                    _mosaicOut = wait.Cue.Resource == "mosaic-out";
                    _ = Sprite(current.Exploration!.Entities[_mosaic.Value], false);
                    _cueAge = -delta;
                }
                else if (wait.Cue.Kind == PresentationCueKind.BattleLoad)
                {
                    if (current.Active is not ActiveBattle) throw new InvalidOperationException("battle-load-state");
                    _prepareBattle(); BattleLoads++;
                    _cueAge = -delta;
                }
                else if (wait.Cue.Kind == PresentationCueKind.Sound)
                {
                    _audio.Play(wait.Cue.Resource ?? throw new InvalidOperationException("sound-binding"));
                }
                else if (wait.Cue.Kind == PresentationCueKind.PreviousMusic)
                {
                    _audio.PlayPrevious();
                }
                else if (wait.Cue.Kind == PresentationCueKind.SoundFade)
                {
                    _audio.BeginSceneEnd();
                }
            }
            _cueAge += delta;
            bool complete;
            switch (wait.Cue.Kind)
            {
                case PresentationCueKind.FadeIn:
                case PresentationCueKind.FadeOut:
                    float opacity = (float)Math.Clamp(_cueAge / 0.5, 0, 1);
                    if (wait.Cue.Kind == PresentationCueKind.FadeIn) opacity = 1 - opacity;
                    if (wait.Cue.Resource == "white") _white.Color = new Color(1, 1, 1, _reducedFlash ? 0 : opacity);
                    else
                    {
                        float brightness = (1 - opacity) * (wait.Cue.Kind == PresentationCueKind.FadeOut ? _fadeStart : 1);
                        _owner.Modulate = new Color(brightness, brightness, brightness);
                    }
                    complete = _cueAge >= 0.5;
                    if (complete) PaletteFades++;
                    break;
                case PresentationCueKind.CameraWait: complete = _camera.DistanceTo(target) < 0.01f; break;
                case PresentationCueKind.Gesture:
                    _nodding = !_shivering && _cueAge is >= (10.0 / 60) and < (30.0 / 60);
                    // Rendering observes the timeline; culling or a missed phase cannot hold story control.
                    complete = _cueAge >= (_shivering ? 30.0 : 40.0) / 60; break;
                case PresentationCueKind.EntityEffect: complete = _cueAge >= 0.5; break;
                case PresentationCueKind.BattleLoad:
                    // The existing battle board and roster have been mounted for a frame; input is still program-owned.
                    complete = _cueAge > 0; break;
                case PresentationCueKind.Sound: complete = true; break;
                case PresentationCueKind.SoundWait: complete = _audio.FiniteMusicFinished(); break;
                case PresentationCueKind.PreviousMusic: complete = true; break;
                case PresentationCueKind.SoundFade:
                    _audio.FadeOut(_cueAge / 0.5);
                    complete = _cueAge >= 0.5;
                    if (complete) SoundFades++;
                    break;
                default: throw new InvalidOperationException("presentation-binding-" + wait.Cue.Kind);
            }
            if (complete)
            {
                completions.Add(new CompletePresentation(wait.Token, wait.Cue.Kind));
                CompletedCueToken = wait.Token.Value;
                CompletedCueKind = wait.Cue.Kind.ToString();
            }
        }
        catch (Exception error) when (error is InvalidOperationException or ArgumentException or KeyNotFoundException)
        { Error = error.Message; }
        return completions;
    }

    internal bool Draw(ExplorationState world, StoryState story)
    {
        if (_visuals is null) return false;
        try
        {
            if (!_visuals.Maps.TryGetValue(world.Map, out var visual)) throw new InvalidOperationException("map-visual-binding");
            var viewport = _owner.GetViewportRect().Size;
            _scale = Math.Max(1, Math.Min((viewport.X - 40) / ViewWidth, (viewport.Y - 200) / ViewHeight));
            _screen = new(new Vector2((viewport.X - ViewWidth * _scale) / 2, 52), new Vector2(ViewWidth, ViewHeight) * _scale);
            _owner.DrawRect(_screen, new Color(0.03f, 0.04f, 0.06f));
            var area = world.Definition.Traversal.SelectActiveArea(world.PlayerEntity.Position)!;
            var view = story.LogicalView;
            if (view is not null) _camera = new(view.BX.Position / 16f, view.BY.Position / 16f);
            var foreground = view is null ? _camera : new Vector2(view.AX.Position / 16f, view.AY.Position / 16f);
            var overlay = world.Definition.OverlayOffsets[area.OneBasedRecordOrdinal - 1];
            bool hasForeground = view is null ? overlay.X != 0 || overlay.Y != 0 :
                view.Area.ForegroundX != view.Area.BackgroundX || view.Area.ForegroundY != view.Area.BackgroundY;
            var offset = view is null ? overlay : new MapOverlayOffset(0, 0);
            var backgroundDraw = DrawLayer(world, visual, _camera, new(0, 0), false, view is null ? null : false, 0);
            object? foregroundDraw = view is not null && hasForeground
                ? DrawLayer(world, visual, foreground, offset, true, false, 1) : null;
            List<object> actors = [];
            List<object> occlusionDraws = [];
            object? backgroundHigh = view is null ? null : DrawLayer(world, visual, _camera, new(0, 0), false, true, 2);
            object? foregroundHigh = view is not null && hasForeground
                ? DrawLayer(world, visual, foreground, offset, true, true, 3) : null;
            int pass = 4;
            var sourceNod = story.Wait as NodWait;
            NodProjection = null;
            int windows = (story.TextWindow is OpenTextWindow ? 1 : 0) + (story.PortraitWindow is OpenPortraitWindow ? 1 : 0);
            // VDP sprite priority comes from the layer/window comparison. The separate
            // script Priority flag only orders sprites relative to one another.
            bool High(ExplorationEntity entity) => (sbyte)entity.Motion.Layer > windows;
            foreach (var entity in world.AllEntities.Where(entity => entity.Visible).OrderBy(entity => entity.Priority)
                .ThenBy(entity => entity.Motion.Y).ThenBy(entity => entity.Slot))
            {
                bool? highPriority = view is null ? null : High(entity);
                int actorPass = pass++;
                bool gesture = sourceNod is not null ? sourceNod.Entity == entity.Entity : _gesture == entity.Entity;
                bool lowered = gesture && (sourceNod?.Lowered ?? _nodding);
                var texture = Sprite(entity, lowered);
                var point = new Vector2(entity.Motion.X / 16f, entity.Motion.Y / 16f);
                if (sourceNod is null && gesture && _shivering) point.X += (int)(_cueAge * 60 / 5) % 2 == 0 ? 1 : -1;
                var destination = new Rect2(_screen.Position + (point - _camera) * _scale, new Vector2(24, 24) * _scale);
                bool visible = _screen.Intersects(destination);
                actors.Add(new { entity = entity.Entity.Value, slot = entity.Slot, sprite = entity.Sprite,
                    facing = entity.Motion.Facing, layer = entity.Motion.Layer, spritePriority = entity.Priority, highPriority, pass = actorPass,
                    x = destination.Position.X, y = destination.Position.Y,
                    width = destination.Size.X, height = destination.Size.Y, visible,
                    texture = texture.GetInstanceId().ToString() });
                if (gesture && sourceNod is not null)
                    NodProjection = new { simulationTick = story.SimulationTick, token = sourceNod.Token.Value,
                        entity = entity.Entity.Value, slot = entity.Slot, sprite = entity.Sprite,
                        facing = entity.Motion.Facing, animationCounter = entity.Motion.AnimationCounter,
                        elapsed = sourceNod.Elapsed, lowered, visible,
                        texture = texture.GetInstanceId(), normalTexture = Sprite(entity, false).GetInstanceId(),
                        width = texture.GetWidth(), height = texture.GetHeight() };
                if (!visible) continue;
                var bounds = destination;
                bool mirror = entity.Motion.Facing is 0 or 4 or 7;
                IReadOnlyList<Rect2> inkRuns = _spriteInk[texture];
                if (_mosaic == entity.Entity)
                {
                    List<Rect2> sampledInk = [];
                    double age = _mosaicOut ? 0.5 - _cueAge : _cueAge;
                    int block = age < 0.1 ? 8 : age < 0.2 ? 6 : age < 0.3 ? 4 : age < 0.4 ? 2 : 1;
                    for (int y = 0; y < 24; y += block)
                        for (int x = 0; x < 24; x += block)
                        {
                            _owner.DrawTextureRectRegion(texture,
                                new(destination.Position + new Vector2(mirror ? 24 - x - block : x, y) * _scale,
                                    Vector2.One * block * _scale), new(x, y, 1, 1));
                            if (inkRuns.Any(run => run.HasPoint(new(x, y)))) sampledInk.Add(new(x, y, block, block));
                        }
                    inkRuns = sampledInk;
                    MosaicDraws++;
                    if (_mosaicOut) MosaicOutDraws++;
                }
                else
                {
                    // Godot flips a negative-width texture rect around its unchanged position.
                    if (mirror) destination.Size = new(-destination.Size.X, destination.Size.Y);
                    _owner.DrawTextureRect(texture, destination, false);
                }
                if (highPriority == false)
                {
                    // Keep sprite-to-sprite order independent of display priority.
                    // Restore high-priority map pixels only under this sprite's ink;
                    // transparent sprite pixels must not erase an earlier high sprite.
                    var ink = inkRuns.Select(run => new Rect2(bounds.Position +
                        new Vector2(mirror ? 24 - run.End.X : run.Position.X, run.Position.Y) * _scale,
                        run.Size * _scale)).ToArray();
                    var actor = (entity.Entity, bounds, (IReadOnlyList<Rect2>)ink);
                    occlusionDraws.Add(DrawLayer(world, visual, _camera, new(0, 0), false, true, pass++, actor));
                    if (hasForeground) occlusionDraws.Add(DrawLayer(world, visual, foreground, offset, true, true, pass++, actor));
                }
                if (gesture)
                {
                    GestureDraws++;
                    if (_shivering) ShiverDraws++;
                    if (lowered) NodDraws++;
                    else if (sourceNod is not null ? sourceNod.Elapsed >= 30 : _cueAge >= 30.0 / 60) RestoredGestureDraws++;
                }
            }
            // The bound A origin already includes the foreground layout offset.
            if (view is null && hasForeground) foregroundDraw = DrawLayer(world, visual, foreground, offset, true, null, pass++);
            CameraProjection = new { simulationTick = story.SimulationTick, token = story.Wait?.Token.Value,
                map = world.Map.Value, targetSlot = view?.TargetSlot, bound = view is not null,
                x = _screen.Position.X, y = _screen.Position.Y, width = _screen.Size.X, height = _screen.Size.Y,
                scale = _scale, windows, background = backgroundDraw, foreground = foregroundDraw, backgroundHigh, foregroundHigh, actors, occlusionDraws };
            int? portraitId = (story.PortraitWindow as OpenPortraitWindow)?.Portrait;
            byte flags = (story.PortraitWindow as OpenPortraitWindow)?.Flags ?? 0;
            // Preserve old content's display hint without admitting its unknown service gate.
            if (story.PortraitWindow is UnknownPortraitWindow { LegacySpeaker: { } speaking } legacy &&
                world.TryResolveEntity(speaking, out var speakingEntity) && speakingEntity.Sprite is { } sprite)
            { portraitId = _visuals.Sprites[sprite].Portrait; flags = legacy.LegacyFlags; }
            _owner.SetMeta("portrait_projection", new Godot.Collections.Dictionary
                { ["id"] = portraitId is { } drawn ? drawn : -1, ["flags"] = flags });
            if (portraitId is { } portrait)
            {
                var work = (story.PortraitWindow as OpenPortraitWindow)?.Work;
                var visualPortrait = _visuals.Portraits[portrait];
                var tiles = Enumerable.Range(0, 64).ToArray();
                foreach (var change in work?.EyesClosed == true ? visualPortrait.Eyes! : [])
                    tiles[change.Y * 8 + change.X] = change.AlternateY * 8 + change.AlternateX;
                foreach (var change in work?.MouthOpen == true ? visualPortrait.Mouth! : [])
                    tiles[change.Y * 8 + change.X] = change.AlternateY * 8 + change.AlternateX;
                var key = (portrait, (flags & 0x40) != 0, work?.EyesClosed == true, work?.MouthOpen == true);
                if (!_portraits.TryGetValue(key, out var texture))
                {
                    using var raster = Image(visualPortrait.Raster);
                    using var composed = raster.GetRegion(new(0, 0, 64, 64));
                    for (int tile = 0; tile < tiles.Length; tile++)
                        if (tiles[tile] != tile)
                            composed.BlitRect(raster, new(tiles[tile] % 8 * 8, tiles[tile] / 8 * 8, 8, 8), new(tile % 8 * 8, tile / 8 * 8));
                    using var crop = composed.GetRegion(new(0, 0, 48, 56));
                    if (key.Item2) crop.FlipX();
                    _portraits[key] = texture = ImageTexture.CreateFromImage(crop);
                }
                var destination = new Rect2((flags & 0x80) != 0 ? viewport.X - 92 : 20,
                    work is null ? viewport.Y - 150 : 52 + work.Y * 12, 72, 84);
                _owner.SetMeta("portrait_projection", new Godot.Collections.Dictionary
                {
                    ["id"] = portrait, ["flags"] = flags, ["eyesClosed"] = key.Item3, ["mouthOpen"] = key.Item4,
                    ["tiles"] = new Godot.Collections.Array<int>(tiles), ["mirrored"] = key.Item2,
                    ["x"] = destination.Position.X, ["y"] = destination.Position.Y,
                    ["width"] = destination.Size.X, ["height"] = destination.Size.Y,
                    ["simulationTick"] = story.SimulationTick,
                });
                _owner.DrawRect(destination.Grow(3), new Color(0.06f, 0.07f, 0.12f));
                _owner.DrawTextureRect(texture, destination, false);
            }
            return true;
        }
        catch (Exception error) when (error is InvalidOperationException or ArgumentException or KeyNotFoundException)
        { Error = error.Message; return true; }
    }

    private object DrawLayer(ExplorationState world, ExplorationMapVisual visual, Vector2 origin, MapOverlayOffset offset,
        bool overlay, bool? highPriority, int pass, (EntityRef Entity, Rect2 Bounds, IReadOnlyList<Rect2> Ink)? actor = null)
    {
        int left = (int)Math.Floor(origin.X / 24), top = (int)Math.Floor(origin.Y / 24);
        int draws = 0;
        object? first = null;
        List<object> overlaps = [];
        for (int y = top; y <= top + ViewHeight / 24; y++)
            for (int x = left; x <= left + ViewWidth / 24 + 1; x++)
            {
                int sourceX = x + offset.X, sourceY = y + offset.Y;
                if (sourceX is < 0 or > 63 || sourceY is < 0 or > 63) continue;
                int block = world.Layout[sourceX, sourceY] & 0x3FF;
                if (overlay && block == 0) continue;
                var rectangle = new Rect2(_screen.Position + (new Vector2(x * 24, y * 24) - origin) * _scale, new Vector2(24, 24) * _scale);
                if (!rectangle.Intersects(_screen) || actor is { } masked && !rectangle.Intersects(masked.Bounds)) continue;
                var texture = Block(visual, block);
                // Reuse the existing block resource; split its nine tile regions by the
                // retained VDP priority bit instead of lifting an entire plane above actors.
                int count = highPriority is null ? 1 : 9;
                for (int tile = 0; tile < count; tile++)
                {
                    int word = visual.Blocks[block][tile];
                    if (highPriority is { } high && ((word & 0x8000) != 0) != high) continue;
                    var tileOffset = highPriority is null ? Vector2.Zero : new Vector2(tile % 3 * 8, tile / 3 * 8);
                    var region = new Rect2(rectangle.Position + tileOffset * _scale,
                        Vector2.One * (highPriority is null ? 24 : 8) * _scale);
                    var clipped = region.Intersection(_screen);
                    if (clipped.Size.X <= 0 || clipped.Size.Y <= 0) continue;
                    foreach (var mask in actor?.Ink ?? [_screen])
                    {
                        if (!clipped.Intersects(mask)) continue;
                        var covered = clipped.Intersection(mask);
                        _owner.DrawTextureRectRegion(texture, covered,
                            new(tileOffset + (covered.Position - region.Position) / _scale, covered.Size / _scale));
                        draws++;
                        first ??= new { sourceX, sourceY, block, tile, word, x = covered.Position.X, y = covered.Position.Y,
                            width = covered.Size.X, height = covered.Size.Y };
                        if (actor is { } painted)
                            overlaps.Add(new { entity = painted.Entity.Value, sourceX, sourceY, block, tile, word,
                                texture = texture.GetInstanceId().ToString(), x = covered.Position.X, y = covered.Position.Y,
                                width = covered.Size.X, height = covered.Size.Y });
                    }
                }
            }
        return new { x = origin.X, y = origin.Y, offsetX = offset.X, offsetY = offset.Y, highPriority, pass, draws, first, overlaps };
    }

    private ImageTexture Block(ExplorationMapVisual visual, int block)
    {
        if (_blocks.TryGetValue((visual.Map, block), out var texture)) return texture;
        if (block >= visual.Blocks.Count) throw new InvalidOperationException("map-block-binding");
        if (!_atlases.TryGetValue(visual.Map, out var atlas)) _atlases[visual.Map] = atlas = Image(visual.Atlas);
        using var image = Godot.Image.CreateEmpty(24, 24, false, Godot.Image.Format.Rgba8);
        for (int tile = 0; tile < 9; tile++)
        {
            int word = visual.Blocks[block][tile], index = (word & 0x3FF) - 0x100;
            if (index < 0) continue;
            if (index >= 640) throw new InvalidOperationException("map-tile-binding");
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                {
                    int sx = (word & 0x800) != 0 ? 7 - x : x, sy = (word & 0x1000) != 0 ? 7 - y : y;
                    image.SetPixel(tile % 3 * 8 + x, tile / 3 * 8 + y,
                        atlas.GetPixel((index % 16 * 8 + sx) * visual.Scale, (index / 16 * 8 + sy) * visual.Scale));
                }
        }
        _blocks[(visual.Map, block)] = texture = ImageTexture.CreateFromImage(image);
        return texture;
    }

    private ImageTexture Sprite(ExplorationEntity entity, bool nod)
    {
        if (_visuals is null || entity.Sprite is not { } sprite || !_visuals.Sprites.TryGetValue(sprite, out var visual))
            throw new InvalidOperationException("entity-sprite-binding");
        int direction = entity.Motion.Facing switch { 1 => 0, 3 => 2, _ => 1 };
        int half = entity.Motion.AnimationCounter > 15 && entity.Motion.AnimationCounter < 128 ? 1 : 0;
        var key = (sprite, direction, half, nod);
        if (_sprites.TryGetValue(key, out var texture)) return texture;
        using var sheet = Image(visual.Directions[direction]);
        using var image = sheet.GetRegion(new(half * 24, 0, 24, 24));
        if (nod)
        {
            // sub_45D70 moves the central head band down one pixel and clears the top tile row.
            for (int y = 11; y >= 0; y--)
                for (int x = 7; x <= 16; x++) image.SetPixel(x, y + 1, image.GetPixel(x, y));
            for (int x = 0; x < 24; x++) image.SetPixel(x, 0, Colors.Transparent);
        }
        _sprites[key] = texture = ImageTexture.CreateFromImage(image);
        List<Rect2> ink = [];
        for (int y = 0; y < 24; y++)
            for (int x = 0; x < 24;)
            {
                if (image.GetPixel(x, y).A == 0) { x++; continue; }
                int start = x++;
                while (x < 24 && image.GetPixel(x, y).A != 0) x++;
                ink.Add(new(start, y, x - start, 1));
            }
        _spriteInk[texture] = ink;
        return texture;
    }

    private static Image Image(ExplorationRaster raster)
    {
        if (raster.Format == "rgba8") return Godot.Image.CreateFromData(raster.Width, raster.Height, false, Godot.Image.Format.Rgba8, raster.CopyBytes());
        var image = new Image();
        if (image.LoadPngFromBuffer(raster.CopyBytes()) != Godot.Error.Ok || image.GetWidth() != raster.Width || image.GetHeight() != raster.Height)
        { image.Dispose(); throw new InvalidOperationException("raster-decode"); }
        return image;
    }

    public void Dispose()
    {
        foreach (var texture in _blocks.Values.Concat(_sprites.Values).Concat(_portraits.Values)) texture.Dispose();
        foreach (var image in _atlases.Values) image.Dispose();
    }
}
