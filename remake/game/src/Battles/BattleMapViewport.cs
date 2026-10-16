using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.GodotAdapter.Maps;

namespace Sf2.Remake.GodotAdapter.Battles;

internal sealed partial class BattleMapViewport : Control
{
    private const int CellSize = 40;
    private readonly Control _board = new() { Name = "Board", MouseFilter = MouseFilterEnum.Ignore };
    private readonly Dictionary<ActorRef, Label> _markers = [];
    private readonly Dictionary<ActorRef, Sprite2D> _sprites = [];
    private readonly Dictionary<ActorRef, int> _spriteIds = [];
    private readonly Dictionary<(int Sprite, int Direction, int Frame), ImageTexture> _textures = [];
    private readonly Dictionary<ImageTexture, object> _resourceSelectors = [];
    private BattleMovementState? _movement;
    private double _movementElapsed;
    private double _movementProgress;
    private bool _movementDelivered;
    private ExplorationVisuals? _visuals;
    private BattleSceneDefinition? _sceneContent;
    private readonly List<ColorRect> _preview = [];
    private readonly List<ColorRect> _target = [];
    private IReadOnlyList<MapPosition> _focus = [];
    private Vector2 _mapSize;
    private readonly MapBlockTextures _mapTextures = new();
    private readonly List<BattlefieldTerrain> _terrain = [];
    private BattleMapRegion? _region;
    private int _builds;

    public override void _Ready()
    {
        ClipContents = true;
        MouseFilter = MouseFilterEnum.Ignore;
        AddChild(_board);
        Resized += Frame;
    }

    internal void Build(BattleDefinition definition, ScenarioDefinition content)
    {
        foreach (Node child in _board.GetChildren()) { _board.RemoveChild(child); child.QueueFree(); }
        _markers.Clear(); _sprites.Clear(); _spriteIds.Clear(); _preview.Clear(); _target.Clear(); _terrain.Clear(); _focus = [];
        _movement = null; _movementElapsed = _movementProgress = 0; _movementDelivered = false;
        ReleaseTextures();
        _builds++;
        _visuals = content.Exploration?.Visuals;
        _sceneContent = content.ScenesFor(definition.Encounter);
        _mapSize = new(definition.Width * CellSize, definition.Height * CellSize);
        _board.Size = _mapSize;
        _region = definition.SourceMapRegion;
        if (content.PrivateDefinitions is not null && _region is null)
            throw new InvalidOperationException("battlefield-map-region-unavailable");
        if (_region is not null) BuildTerrain(definition, content);
        else BuildAuthoredTerrain(definition);
        foreach (var actor in definition.Deployments)
        {
            var marker = new Label { Name = "Actor_" + actor.Actor.Value, Size = new(CellSize - 4, CellSize - 8),
                ClipText = true, MouseFilter = MouseFilterEnum.Ignore, ZIndex = 1 };
            marker.AddThemeFontSizeOverride("font_size", 10);
            _board.AddChild(marker);
            _markers.Add(actor.Actor, marker);
            if (_sceneContent?.FieldDeath is { } field && _visuals is not null)
            {
                int sprite = actor.Faction == BattleFaction.Ally
                    ? field.AllySprites[actor.Initialization?.AllyPartyMember ?? actor.ProcessingOrder]
                    : field.EnemySprites[actor.Actor];
                if (!_visuals.Sprites.ContainsKey(sprite)) throw new InvalidOperationException("field-actor-resource-unavailable");
                _spriteIds.Add(actor.Actor, sprite);
                var image = new Sprite2D { Name = "MapSprite_" + actor.Actor.Value, TextureFilter = TextureFilterEnum.Nearest,
                    Scale = Vector2.One * (CellSize / 24f), ZIndex = 1 };
                _board.AddChild(image); _sprites.Add(actor.Actor, image);
            }
        }
    }

    private void BuildTerrain(BattleDefinition definition, ScenarioDefinition content)
    {
        var region = _region!;
        if (content.Exploration is not { } exploration || _visuals is null ||
            !exploration.Maps.TryGetValue(region.Map, out var map) || !_visuals.Maps.TryGetValue(region.Map, out var visual))
            throw new InvalidOperationException("battlefield-map-resource-unavailable");
        if (region.Origin.X + definition.Width > 64 || region.Origin.Y + definition.Height > 64)
            throw new InvalidOperationException("battlefield-map-region-bounds");
        var area = map.Traversal.SelectActiveArea(region.Origin)
            ?? throw new InvalidOperationException("battlefield-map-area-unavailable");
        var offset = map.OverlayOffsets[area.OneBasedRecordOrdinal - 1];
        // The connected maps have fixed 1:1 planes. Other plane motion requires
        // an explicit consumer rather than silently flattening it into this board.
        var view = map.ViewAreas.ElementAtOrDefault(area.OneBasedRecordOrdinal - 1);
        if (view is not null && (view.ParallaxAX != 256 || view.ParallaxAY != 256 ||
            view.ParallaxBX != 256 || view.ParallaxBY != 256 || view.AutoscrollAX != 0 ||
            view.AutoscrollAY != 0 || view.AutoscrollBX != 0 || view.AutoscrollBY != 0))
            throw new InvalidOperationException("battlefield-map-plane-motion-unsupported");
        // Main plane coordinates are selected by the source area. Overlay offsets
        // address the other packed plane relative to it, not battle cell zero.
        var background = view is { Layer: not 0 } ? new Vector2I(-offset.X, -offset.Y) : Vector2I.Zero;
        var foreground = view is { Layer: not 0 } ? Vector2I.Zero : new Vector2I(offset.X, offset.Y);
        foreach (bool high in new[] { false, true })
        {
            Add(background, false, high);
            if (offset.X != 0 || offset.Y != 0) Add(foreground, true, high);
        }
        void Add(Vector2I shift, bool overlay, bool high)
        {
            var layer = new BattlefieldTerrain();
            try { layer.Configure(map, visual, region, _mapTextures, definition.Width, definition.Height, CellSize, shift, overlay, high); }
            catch { layer.Free(); throw; }
            _board.AddChild(layer); _terrain.Add(layer);
        }
    }

    private void BuildAuthoredTerrain(BattleDefinition definition)
    {
        for (int y = 0; y < definition.Height; y++)
            for (int x = 0; x < definition.Width; x++)
            {
                var surface = definition.Terrain[y * 48 + x].Surface;
                _board.AddChild(new ColorRect { Position = new(x * CellSize, y * CellSize), Size = new(CellSize - 2, CellSize - 2),
                    Color = surface == TerrainSurface.Barrier ? new(0.12f, 0.14f, 0.18f) : surface == TerrainSurface.Brush ? new(0.19f, 0.34f, 0.24f) : new(0.25f, 0.29f, 0.35f),
                    MouseFilter = MouseFilterEnum.Ignore });
            }
    }

    private void ReleaseTextures()
    {
        foreach (var texture in _textures.Values) texture.Dispose();
        _textures.Clear(); _resourceSelectors.Clear();
        _mapTextures.Dispose();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationPredelete) ReleaseTextures();
    }

    internal void Present(BattlePresentation projection)
    {
        if (_movement?.Token != projection.Movement?.Token)
        { _movementElapsed = _movementProgress = 0; _movementDelivered = false; }
        _movement = projection.Movement;
        foreach (var marker in _markers.Values) marker.Visible = false;
        foreach (var sprite in _sprites.Values) sprite.Hide();
        foreach (var node in _target) { _board.RemoveChild(node); node.QueueFree(); }
        _target.Clear();
        foreach (var actor in projection.Markers)
        {
            var node = _markers[actor.Actor];
            node.Visible = true;
            node.Position = new(actor.Position.X * CellSize + 2, actor.Position.Y * CellSize + 4);
            node.Text = actor.Text;
            node.Modulate = actor.Selected ? Colors.Gold : actor.Ally ? Colors.LightSkyBlue : Colors.LightCoral;
            if (actor.Targeted)
            {
                var origin = new Vector2(actor.Position.X, actor.Position.Y) * CellSize;
                foreach (var edge in new[] { new Rect2(1, 1, CellSize - 2, 2), new Rect2(1, CellSize - 3, CellSize - 2, 2),
                    new Rect2(1, 1, 2, CellSize - 2), new Rect2(CellSize - 3, 1, 2, CellSize - 2) })
                {
                    var border = new ColorRect { Name = "Target_" + actor.Actor.Value, Position = origin + edge.Position, Size = edge.Size,
                        Color = Colors.Cyan, MouseFilter = MouseFilterEnum.Ignore, ZIndex = 3 };
                    _board.AddChild(border); _target.Add(border);
                }
            }
            if (_sprites.TryGetValue(actor.Actor, out var sprite))
            {
                node.Hide(); sprite.Show();
                sprite.Position = new((actor.Position.X + 0.5f) * CellSize, (actor.Position.Y + 0.5f) * CellSize);
                int id = actor.DeathEffect ? 63 : _spriteIds[actor.Actor];
                int facing = actor.Facing ?? (int)sprite.GetMeta("facing", 3);
                int direction = facing switch { 1 => 0, 3 => 2, _ => 1 };
                sprite.Texture = Texture(id, direction);
                sprite.FlipH = facing == 0;
                sprite.Modulate = actor.Selected ? Colors.Gold : Colors.White;
                sprite.SetMeta("mapsprite", id); sprite.SetMeta("facing", facing); sprite.SetMeta("walkingFrame", 0);
            }
        }
        foreach (var node in _preview) { _board.RemoveChild(node); node.QueueFree(); }
        _preview.Clear();
        foreach (var position in projection.Preview)
        {
            var node = new ColorRect { Position = new(position.X * CellSize + 4, position.Y * CellSize + CellSize - 7),
                Size = new(CellSize - 10, 3), Color = Colors.Gold, MouseFilter = MouseFilterEnum.Ignore, ZIndex = 3 };
            _board.AddChild(node);
            _preview.Add(node);
        }
        _focus = projection.Focus;
        Frame();
        ProjectMovement();
    }

    internal CompletePresentation? ConsumeMovement(double delta, bool reduced)
    {
        if (_movement is not { } movement || _movementDelivered) return null;
        _movementElapsed += delta;
        // Delivery duration is modern presentation, not a VInt/RNG opportunity count.
        _movementProgress = Math.Min(1, _movementElapsed / (reduced ? 0.10 : 0.20));
        ProjectMovement();
        if (_movementProgress < 1) return null;
        _movementDelivered = true;
        return new(movement.Token, movement.CompletionKind);
    }

    private void ProjectMovement()
    {
        if (_movement is not { } movement) return;
        var from = new Vector2(movement.From.X, movement.From.Y);
        var to = new Vector2(movement.To.X, movement.To.Y);
        var position = from.Lerp(to, (float)_movementProgress) * CellSize;
        _markers[movement.Actor].Position = position + new Vector2(2, 4);
        if (_sprites.TryGetValue(movement.Actor, out var sprite))
        {
            int direction = movement.Facing switch { 1 => 0, 3 => 2, _ => 1 };
            int frame = _movementProgress is > 0 and < 1 ? (int)(_movementProgress * 4) % 2 : 0;
            sprite.Position = position + Vector2.One * (CellSize / 2f);
            sprite.Texture = Texture(_spriteIds[movement.Actor], direction, frame);
            sprite.FlipH = movement.Facing == 0;
            sprite.SetMeta("facing", movement.Facing); sprite.SetMeta("walkingFrame", frame);
        }
    }

    internal object? ObserveMovement() => _movement is not { } movement ? null : new
    {
        actor = movement.Actor.Value, purpose = movement.Purpose.ToString(), token = movement.Token.Value,
        segment = movement.Segment, from = movement.From, to = movement.To, facing = movement.Facing,
        progress = _movementProgress, delivered = _movementDelivered, path = movement.Path,
    };

    private ImageTexture Texture(int sprite, int direction, int walkingFrame = 0)
    {
        var key = (sprite, direction, walkingFrame);
        if (_textures.TryGetValue(key, out var texture)) return texture;
        var raster = sprite == 63 ? _sceneContent!.Rasters[_sceneContent.FieldDeath!.ExitFrames[direction]]
            : _visuals!.Sprites[sprite].Directions[direction];
        using var sheet = raster.Format == "rgba8"
            ? Image.CreateFromData(raster.Width, raster.Height, false, Image.Format.Rgba8, raster.CopyBytes()) : new Image();
        if (raster.Format == "png" && sheet.LoadPngFromBuffer(raster.CopyBytes()) != Godot.Error.Ok)
            throw new InvalidOperationException("field-sprite-image");
        // Death and idle poses use frame0; movement consumes both admitted halves.
        using var frame = sheet.GetRegion(new Rect2I(walkingFrame * 24, 0, 24, 24));
        _textures[key] = texture = ImageTexture.CreateFromImage(frame);
        _resourceSelectors[texture] = new { sprite, direction, frame = walkingFrame,
            raster = sprite == 63 ? _sceneContent!.FieldDeath!.ExitFrames[direction] : null };
        return texture;
    }

    // The view is reparented during exploration/battle handoff. Cached resources
    // must outlive ExitTree; ref-counted textures are released with this owner.

    private void Frame()
    {
        if (_mapSize == Vector2.Zero || Size.X <= 0 || Size.Y <= 0) return;
        var minimum = _focus.Count == 0 ? _mapSize / 2 : new Vector2(_focus[0].X, _focus[0].Y) * CellSize;
        var maximum = minimum;
        foreach (var position in _focus)
        {
            var point = new Vector2(position.X, position.Y) * CellSize;
            minimum = minimum.Min(point);
            maximum = maximum.Max(point);
        }
        minimum -= Vector2.One * CellSize;
        maximum += Vector2.One * (2 * CellSize);
        var extent = maximum - minimum;
        float scale = Mathf.Min(1, Mathf.Min(Size.X / extent.X, Size.Y / extent.Y));
        _board.Scale = Vector2.One * scale;
        var worldSize = _mapSize * scale;
        var desired = Size / 2 - (minimum + maximum) / 2 * scale;
        _board.Position = new(
            worldSize.X <= Size.X ? (Size.X - worldSize.X) / 2 : Mathf.Clamp(desired.X, Size.X - worldSize.X, 0),
            worldSize.Y <= Size.Y ? (Size.Y - worldSize.Y) / 2 : Mathf.Clamp(desired.Y, Size.Y - worldSize.Y, 0));
    }

    internal int BoardChildren => _board.GetChildCount();
    public string ReadTerrainJson() => System.Text.Json.JsonSerializer.Serialize(new
    {
        region = _region, builds = _builds, textures = _mapTextures.Count, cellSize = CellSize,
        board = Rectangle(new Rect2(_board.Position, _board.Size)), scale = _board.Scale.X,
        viewport = Rectangle(GetGlobalRect()), clip = ClipContents,
        visibleLocal = Rectangle(new Rect2(-_board.Position / _board.Scale, Size / _board.Scale).Intersection(new(Vector2.Zero, _mapSize))),
        layers = _terrain.Select(layer => layer.Observe()).ToArray(),
        children = _board.GetChildren().Select(child => new { name = child.Name.ToString(), id = child.GetInstanceId().ToString() }),
        actors = _sprites.Select(pair => new { id = pair.Key.Value, x = pair.Value.Position.X, y = pair.Value.Position.Y,
            z = pair.Value.ZIndex, texture = pair.Value.Texture?.GetInstanceId().ToString(),
            validTexture = pair.Value.Texture is not null && GodotObject.IsInstanceValid(pair.Value.Texture),
            visible = pair.Value.IsVisibleInTree() }),
        previewZ = _preview.Select(node => node.ZIndex).Distinct().ToArray(),
        targets = _target.Select(node => new { name = node.Name.ToString(), z = node.ZIndex, rect = Rectangle(new Rect2(node.Position, node.Size)) }),
    });
    internal int PreviewCount => _preview.Count;
    internal float Zoom => _board.Scale.X;
    internal object? PreviewRectangle => _preview.Count == 0 ? null : Rectangle(_preview[^1].GetGlobalRect());
    internal bool PreviewInsideMap => _preview.Count > 0 && GetGlobalRect().Encloses(_preview[^1].GetGlobalRect());
    internal IEnumerable<object> ObserveActors(IEnumerable<BattleActorState> actors) => actors.Select(a =>
    {
        var node = _markers[a.Actor];
        return new { id = a.Actor.Value, hp = a.Hp, mp = a.Mp, exp = a.Exp, kills = a.Kills, defeats = a.Defeats, x = a.Position?.X, y = a.Position?.Y,
            maxHp = a.MaxHp, maxMp = a.MaxMp, level = a.Level, defense = a.Defense, move = a.Definition.Move,
            learned = a.Spells, leader = a.Definition.Physical?.Leader == true,
            attack = a.Attack, sourceAttack = a.Definition.Attack, mover = a.Definition.Mover.ToString(),
            status = a.Status, activationWord = a.ActivationWord, aiMemory = a.AiMemory, lastTarget = a.LastTarget?.Value,
            anchorX = a.Deployment.Position?.X, anchorY = a.Deployment.Position?.Y,
            primaryOrder = a.Deployment.Initialization?.PrimaryOrder, secondaryOrder = a.Deployment.Initialization?.SecondaryOrder,
            commandset = a.Deployment.Initialization?.AiCommandset,
            items = a.SourceLoadout?.Items, spells = a.SourceLoadout?.Spells,
            nodeX = node.Position.X, nodeY = node.Position.Y, visible = _sprites.TryGetValue(a.Actor, out var image) ? image.Visible : node.Visible, text = node.Text,
            sprite = _sprites.TryGetValue(a.Actor, out var sprite) ? new { visible = sprite.Visible, resource = (int)sprite.GetMeta("mapsprite", -1),
                resourceSelector = sprite.Texture is ImageTexture bound && _resourceSelectors.TryGetValue(bound, out var selector)
                    ? selector : null,
                texturePresent = sprite.Texture is not null, visibleInTree = sprite.IsVisibleInTree(),
                facing = (int)sprite.GetMeta("facing", -1), walkingFrame = (int)sprite.GetMeta("walkingFrame", 0), flipH = sprite.FlipH, x = sprite.Position.X, y = sprite.Position.Y,
                width = sprite.Texture?.GetWidth(), height = sprite.Texture?.GetHeight() } : null,
            globalRect = Rectangle(node.GetGlobalRect()), insideMap = GetGlobalRect().Encloses(node.GetGlobalRect()) };
    });
    internal static object Rectangle(Rect2 rect) => new
    { x = rect.Position.X, y = rect.Position.Y, width = rect.Size.X, height = rect.Size.Y };
}
