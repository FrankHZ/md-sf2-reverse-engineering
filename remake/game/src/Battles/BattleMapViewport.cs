using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter.Battles;

internal sealed partial class BattleMapViewport : Control
{
    private const int CellSize = 40;
    private readonly Control _board = new() { Name = "Board", MouseFilter = MouseFilterEnum.Ignore };
    private readonly Dictionary<ActorRef, Label> _markers = [];
    private readonly Dictionary<ActorRef, Sprite2D> _sprites = [];
    private readonly Dictionary<ActorRef, int> _spriteIds = [];
    private readonly Dictionary<(int Sprite, int Direction, int Frame), ImageTexture> _textures = [];
    private BattleMovementState? _movement;
    private double _movementElapsed;
    private double _movementProgress;
    private bool _movementDelivered;
    private ExplorationVisuals? _visuals;
    private BattleSceneDefinition? _sceneContent;
    private readonly List<ColorRect> _preview = [];
    private IReadOnlyList<MapPosition> _focus = [];
    private Vector2 _mapSize;

    public override void _Ready()
    {
        ClipContents = true;
        MouseFilter = MouseFilterEnum.Ignore;
        AddChild(_board);
        Resized += Frame;
    }

    internal void Build(BattleDefinition definition, ScenarioDefinition content)
    {
        _visuals = content.Exploration?.Visuals;
        _sceneContent = content.BattleScenes;
        _mapSize = new(definition.Width * CellSize, definition.Height * CellSize);
        _board.Size = _mapSize;
        for (int y = 0; y < definition.Height; y++)
            for (int x = 0; x < definition.Width; x++)
            {
                var surface = definition.Terrain[y * 48 + x].Surface;
                _board.AddChild(new ColorRect { Position = new(x * CellSize, y * CellSize), Size = new(CellSize - 2, CellSize - 2),
                    Color = surface == TerrainSurface.Barrier ? new(0.12f, 0.14f, 0.18f) : surface == TerrainSurface.Brush ? new(0.19f, 0.34f, 0.24f) : new(0.25f, 0.29f, 0.35f),
                    MouseFilter = MouseFilterEnum.Ignore });
            }
        foreach (var actor in definition.Deployments)
        {
            var marker = new Label { Name = "Actor_" + actor.Actor.Value, Size = new(CellSize - 4, CellSize - 8),
                ClipText = true, MouseFilter = MouseFilterEnum.Ignore };
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
                    Scale = Vector2.One * (CellSize / 24f) };
                _board.AddChild(image); _sprites.Add(actor.Actor, image);
            }
        }
    }

    internal void Present(BattlePresentation projection)
    {
        if (_movement?.Token != projection.Movement?.Token)
        { _movementElapsed = _movementProgress = 0; _movementDelivered = false; }
        _movement = projection.Movement;
        foreach (var marker in _markers.Values) marker.Visible = false;
        foreach (var sprite in _sprites.Values) sprite.Hide();
        foreach (var actor in projection.Markers)
        {
            var node = _markers[actor.Actor];
            node.Visible = true;
            node.Position = new(actor.Position.X * CellSize + 2, actor.Position.Y * CellSize + 4);
            node.Text = actor.Text;
            node.Modulate = actor.Selected ? Colors.Gold : actor.Ally ? Colors.LightSkyBlue : Colors.LightCoral;
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
                Size = new(CellSize - 10, 3), Color = Colors.Gold, MouseFilter = MouseFilterEnum.Ignore };
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
                facing = (int)sprite.GetMeta("facing", -1), walkingFrame = (int)sprite.GetMeta("walkingFrame", 0), flipH = sprite.FlipH, x = sprite.Position.X, y = sprite.Position.Y,
                width = sprite.Texture?.GetWidth(), height = sprite.Texture?.GetHeight() } : null,
            globalRect = Rectangle(node.GetGlobalRect()), insideMap = GetGlobalRect().Encloses(node.GetGlobalRect()) };
    });
    internal static object Rectangle(Rect2 rect) => new
    { x = rect.Position.X, y = rect.Position.Y, width = rect.Size.X, height = rect.Size.Y };
}
