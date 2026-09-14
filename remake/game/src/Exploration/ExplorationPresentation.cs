using Godot;
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
    private readonly Dictionary<(int Portrait, bool Mirror), ImageTexture> _portraits = [];
    private readonly AudioStreamPlayer _music;
    private int _spriteMounts;
    private WaitToken? _cue;
    private double _cueAge;
    private EntityRef? _gesture;
    private bool _nodding;
    private MapId? _map;
    private Vector2 _camera;
    private Rect2 _screen;
    private float _scale;
    private const int ViewWidth = 320, ViewHeight = 192;

    internal ExplorationPresentation(Control owner, ExplorationDefinition definition)
    {
        _owner = owner; _visuals = definition.Visuals;
        _music = new AudioStreamPlayer { Name = "ExplorationMusic" };
        owner.AddChild(_music);
    }

    internal string? Error { get; private set; }
    internal int SpriteMounts => _spriteMounts;
    internal int GestureDraws { get; private set; }
    internal int NodDraws { get; private set; }
    internal int RestoredGestureDraws { get; private set; }
    internal int SoundStarts { get; private set; }
    internal int SoundFades { get; private set; }
    internal Vector2 Camera => _camera;
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
                var area = world.Definition.Traversal.SelectActiveArea(world.PlayerEntity.Position)!.Area;
                target = current.Story.Cursor is not null && current.Story.CameraTarget is { } destination
                    ? new Vector2(destination.X * 24, destination.Y * 24)
                    : new Vector2(world.PlayerEntity.Motion.X / 16f - ViewWidth / 2f + 12,
                        world.PlayerEntity.Motion.Y / 16f - ViewHeight / 2f + 12);
                target.X = Mathf.Clamp(target.X, area.MinimumX * 24, Math.Max(area.MinimumX * 24, (area.MaximumX + 1) * 24 - ViewWidth));
                target.Y = Mathf.Clamp(target.Y, area.MinimumY * 24, Math.Max(area.MinimumY * 24, (area.MaximumY + 1) * 24 - ViewHeight));
                if (_map != world.Map) { _map = world.Map; _camera = target; }
                else _camera = _camera.MoveToward(target, (float)(delta * 180));
                foreach (var entity in world.AllEntities.Where(entity => entity.WaitingForSprite && entity.SpriteReady != entity.SpriteRequest))
                {
                    _ = Sprite(entity, false);
                    _spriteMounts++;
                    completions.Add(new EntitySpriteReady(entity.Slot, entity.SpriteRequest));
                }
            }
            if (current.Story.Wait is not PresentationWait wait)
            { _cue = null; _gesture = null; _nodding = false; ActiveCue = null; return completions; }
            if (current.Exploration is null && wait.Cue.Kind is PresentationCueKind.CameraWait or PresentationCueKind.Gesture)
                throw new InvalidOperationException("presentation-map-unavailable");
            if (_cue != wait.Token)
            {
                _cue = wait.Token; _cueAge = 0;
                ActiveCue = wait.Cue.Kind.ToString();
                if (wait.Cue.Kind == PresentationCueKind.Gesture)
                {
                    if (wait.Cue.Resource != "nod" || wait.Cue.Entity is null) throw new InvalidOperationException("gesture-binding");
                    _gesture = wait.Cue.Entity;
                    _ = Sprite(current.Exploration!.Entities[_gesture.Value], true);
                }
                else if (wait.Cue.Kind == PresentationCueKind.Sound)
                {
                    if (wait.Cue.Resource is not ("MUSIC_JOIN" or "MUSIC_SAD_JOIN")) throw new InvalidOperationException("sound-binding");
                    _music.Stop();
                    var previous = _music.Stream; _music.Stream = null; previous?.Dispose();
                    _music.Stream = JoinCue(wait.Cue.Resource == "MUSIC_SAD_JOIN");
                    _music.VolumeDb = -12; _music.Play(); SoundStarts++;
                }
            }
            _cueAge += delta;
            bool complete;
            switch (wait.Cue.Kind)
            {
                case PresentationCueKind.CameraWait: complete = _camera.DistanceTo(target) < 0.01f; break;
                case PresentationCueKind.Gesture:
                    _nodding = _cueAge is >= (10.0 / 60) and < (30.0 / 60);
                    // Rendering observes the timeline; culling or a missed phase cannot hold story control.
                    complete = _cueAge >= 40.0 / 60; break;
                case PresentationCueKind.Sound: complete = _music.Playing; break;
                case PresentationCueKind.SoundFade:
                    _music.VolumeDb = (float)(-12 - 60 * Math.Min(1, _cueAge / 0.5));
                    complete = _cueAge >= 0.5;
                    if (complete) { _music.Stop(); SoundFades++; }
                    break;
                default: throw new InvalidOperationException("presentation-binding-" + wait.Cue.Kind);
            }
            if (complete) completions.Add(new CompletePresentation(wait.Token, wait.Cue.Kind));
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
            DrawLayer(world, visual, new(0, 0), false);
            foreach (var entity in world.AllEntities.Where(entity => entity.Visible).OrderBy(entity => entity.Motion.Y).ThenBy(entity => entity.Slot))
            {
                bool gesture = _gesture == entity.Entity;
                var texture = Sprite(entity, gesture && _nodding);
                var point = new Vector2(entity.Motion.X / 16f, entity.Motion.Y / 16f);
                var destination = new Rect2(_screen.Position + (point - _camera) * _scale, new Vector2(24, 24) * _scale);
                if (!_screen.Intersects(destination)) continue;
                bool mirror = entity.Motion.Facing is 0 or 4 or 7;
                if (mirror) { destination.Position += new Vector2(destination.Size.X, 0); destination.Size = new(-destination.Size.X, destination.Size.Y); }
                _owner.DrawTextureRect(texture, destination, false);
                if (gesture)
                {
                    GestureDraws++;
                    if (_nodding) NodDraws++;
                    else if (_cueAge >= 30.0 / 60) RestoredGestureDraws++;
                }
            }
            var overlay = world.Definition.OverlayOffsets[area.OneBasedRecordOrdinal - 1];
            if (overlay.X != 0 || overlay.Y != 0) DrawLayer(world, visual, overlay, true);
            EntityRef? speaker = story.TextWindow is OpenTextWindow text ? text.Speaker : null;
            if (speaker is { } speaking && world.Entities.TryGetValue(speaking, out var speakingEntity) && speakingEntity.Sprite is { } sprite &&
                _visuals.Sprites[sprite].Portrait is { } portrait)
            {
                byte flags = ((OpenTextWindow)story.TextWindow).SpeakerFlags;
                var key = (portrait, (flags & 0x40) != 0);
                if (!_portraits.TryGetValue(key, out var texture))
                {
                    using var raster = Image(_visuals.Portraits[portrait].Raster);
                    using var crop = raster.GetRegion(new(0, 0, 48, 56));
                    if (key.Item2) crop.FlipX();
                    _portraits[key] = texture = ImageTexture.CreateFromImage(crop);
                }
                var destination = new Rect2((flags & 0x80) != 0 ? viewport.X - 92 : 20, viewport.Y - 150, 72, 84);
                _owner.DrawRect(destination.Grow(3), new Color(0.06f, 0.07f, 0.12f));
                _owner.DrawTextureRect(texture, destination, false);
            }
            return true;
        }
        catch (Exception error) when (error is InvalidOperationException or ArgumentException or KeyNotFoundException)
        { Error = error.Message; return true; }
    }

    private void DrawLayer(ExplorationState world, ExplorationMapVisual visual, MapOverlayOffset offset, bool overlay)
    {
        int left = (int)Math.Floor(_camera.X / 24), top = (int)Math.Floor(_camera.Y / 24);
        for (int y = top; y <= top + ViewHeight / 24; y++)
            for (int x = left; x <= left + ViewWidth / 24 + 1; x++)
            {
                int sourceX = x + offset.X, sourceY = y + offset.Y;
                if (sourceX is < 0 or > 63 || sourceY is < 0 or > 63) continue;
                int block = world.Layout[sourceX, sourceY] & 0x3FF;
                if (overlay && block == 0) continue;
                var rectangle = new Rect2(_screen.Position + (new Vector2(x * 24, y * 24) - _camera) * _scale, new Vector2(24, 24) * _scale);
                var clipped = rectangle.Intersection(_screen);
                if (clipped.Size.X <= 0 || clipped.Size.Y <= 0) continue;
                _owner.DrawTextureRectRegion(Block(visual, block), clipped,
                    new((clipped.Position - rectangle.Position) / _scale, clipped.Size / _scale));
            }
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

    // Explicit project-authored presentation mapping. It preserves command identity, not the original driver waveform.
    private static AudioStreamWav JoinCue(bool sad)
    {
        const int rate = 22050, count = rate * 2;
        byte[] samples = new byte[count * 2];
        double[] notes = sad ? [261.63, 311.13, 392] : [261.63, 329.63, 392];
        for (int index = 0; index < count; index++)
        {
            double time = index / (double)rate, envelope = Math.Min(1, time * 20) * Math.Min(1, (2 - time) * 20);
            short value = (short)(6500 * envelope * notes.Sum(note => Math.Sin(Math.Tau * note * time)) / notes.Length);
            samples[index * 2] = (byte)(value & 255); samples[index * 2 + 1] = (byte)((value >> 8) & 255);
        }
        return new AudioStreamWav { Format = AudioStreamWav.FormatEnum.Format16Bits, MixRate = rate, Stereo = false, Data = samples,
            LoopMode = AudioStreamWav.LoopModeEnum.Forward, LoopBegin = 0, LoopEnd = count };
    }

    public void Dispose()
    {
        foreach (var texture in _blocks.Values.Concat(_sprites.Values).Concat(_portraits.Values)) texture.Dispose();
        foreach (var image in _atlases.Values) image.Dispose();
        _music.Stop();
        var stream = _music.Stream; _music.Stream = null; stream?.Dispose();
    }
}
