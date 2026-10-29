using Godot;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.GodotAdapter.Battles;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Exploration;

// Only reading position lives here. Every value is read again from the live field party.
internal sealed partial class PartyStatusView : Control
{
    private readonly BattleStatusCard _card = new() { Name = "MemberCard" };
    private readonly ContextHint _header = new() { Name = "PartyHeader" };
    private readonly ContextHint _hint = new() { Name = "PartyHint" };
    private int _index;
    private bool _requiresRelease;
    private string? _unavailable;
    private int _count;
    private static BattleActorStartInput[] Members(GameSession session)
    {
        var party = session.Current.Exploration!.Party;
        return party.Actors.Where(actor => Deployment(session, actor.Actor)?.Faction == BattleFaction.Ally).ToArray();
    }
    private static BattleDeploymentDefinition? Deployment(GameSession session, ActorRef actor) =>
        session.Definition.Encounters[session.Current.Exploration!.Party.Encounter].Deployments.FirstOrDefault(row => row.Actor == actor) ??
        session.Definition.Encounters.Values.SelectMany(value => value.Deployments)
            .FirstOrDefault(row => row.Faction == BattleFaction.Ally && row.Actor == actor);
    internal bool Open => Visible;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); MouseFilter = MouseFilterEnum.Ignore;
        var shade = new ColorRect { Color = new(0, 0, 0, 0.65f), MouseFilter = MouseFilterEnum.Ignore };
        shade.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(shade);
        AddChild(_header); AddChild(_card); AddChild(_hint); Hide();
    }

    internal void Begin(GameSession session, GameInput input)
    { _index = 0; _requiresRelease = true; Show(); Present(session, input); }

    internal void Present(GameSession session, GameInput input)
    {
        if (!Open) return;
        if (!input.GameplayHeld) _requiresRelease = false;
        var party = session.Current.Exploration?.Party;
        if (!session.Current.CanWaitAtInput || party is null) { Hide(); return; }
        var members = Members(session); _count = members.Length;
        _index = Math.Clamp(_index, 0, Math.Max(0, _count - 1));
        _header.Text.Text = $"Party Status · Read only    Gold: {party.Gold?.ToString() ?? "Unavailable"}    Member {(_index + 1)}/{_count}";
        _header.Arrange(GetViewportRect().Size); _header.Position = new(_header.Position.X, 32);
        _card.Hide(); _unavailable = null;
        if (_count > 0)
        {
            var actor = members[_index];
            var deployment = Deployment(session, actor.Actor);
            if (deployment is null) _unavailable = "Member details unavailable: no admitted definition.";
            else
            {
                string name = BattleCharacterNames.Party(actor.Actor, deployment.Initialization?.AllyPartyMember,
                    session.Definition.Exploration!.MemberNames);
                _card.Present(actor.Actor, name, "Party member", actor.Hp, actor.Progress?.MaxHp ?? deployment.Definition.MaxHp,
                    actor.Mp, actor.Progress?.MaxMp ?? deployment.Definition.MaxMp);
                _card.Position = (GetViewportRect().Size - _card.RequestedSize) / 2;
            }
        }
        else _unavailable = "Party information unavailable: no carried members.";
        _hint.Text.Text = (_unavailable is null ? "" : _unavailable + "\n") +
            $"{input.Hint(GameAction.Left)} / {input.Hint(GameAction.Right)}: Member    " +
            $"{input.Hint(GameAction.Cancel)} / {input.Hint(GameAction.Equipment)}: Close";
        _hint.Arrange(GetViewportRect().Size);
    }

    internal void Suspend() => _requiresRelease = true;
    internal void Handle(GameAction action, GameSession session, GameInput input)
    {
        if (_requiresRelease) return;
        if (action is GameAction.Cancel or GameAction.Equipment or GameAction.Confirm) { Hide(); return; }
        int count = Members(session).Length;
        int step = action is GameAction.Left or GameAction.Up ? -1 : action is GameAction.Right or GameAction.Down ? 1 : 0;
        if (count > 0) _index = (_index + step + count) % count;
        Present(session, input);
    }

    internal object Observe() => new { visible = IsVisibleInTree(), index = _index, count = _count, requiresRelease = _requiresRelease,
        header = _header.Text.Text, hint = _hint.Text.Text, unavailable = _unavailable, card = _card.Observe() };
}

