using Godot;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Exploration;

// Disposable member selection/focus only; original member-screen summary + aligned details.
internal sealed partial class PartyStatusView : Control
{
    private readonly MemberPageView _page = new() { Name = "MemberLayer" };
    private readonly FieldActionView _actions = new() { Name = "FieldActions" };
    private readonly IdentityIcons _icons = new();
    private object? _iconSource;
    private SessionFailure? _assetFailure;
    internal Func<Sf2.Remake.Application.Runtime.Exploration.FieldCommand, long, SessionResult>? SubmitField { get; set; }
    private GameSession? _session;
    private GameInput? _input;
    private PartyDetails? _shown;
    internal MemberPageView? NativeMembers => Open && !_actions.Open && _page.IsVisibleInTree() ? _page : null;
    internal bool Open => Visible;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(_page); AddChild(_actions);
        _page.RequestAction = action => Handle(action, _session!, _input!);
        _page.Back = Hide;
        _page.Refresh = () => { if (Open) Present(_session!, _input!); };
        _page.Primary = Prepare;
        Hide();
    }

    public override void _ExitTree() { _icons.Dispose(); _iconSource = null; }
    private string _entry = "actions";
    internal void PrepareGraphics(GameSession session)
    {
        if (ReferenceEquals(_iconSource, session.Definition)) return;
        var visuals = session.Definition.Exploration?.Visuals;
        _icons.Configure(visuals?.IdentityIcons, visuals?.HdIdentityIcons);
        _iconSource = session.Definition; _assetFailure = null;
    }
    internal void Begin(GameSession session, GameInput input, string entry = "actions")
    {
        _entry = entry;
        try { PrepareGraphics(session); }
        catch (InvalidOperationException error)
        { _assetFailure = new(SessionFailureKind.AdapterError, error.Message, "presentation.identityIcons", "Member identity icon decoding failed."); }
        _page.Details.ConfigureIcons(_icons, session.Definition);
        _session = session; _input = input;
        _page.Reset(entry == "magic" ? 1 : entry == "item" ? 2 : 0);
        _actions.Hide();
        Show(); Present(session, input);
    }

    internal void Present(GameSession session, GameInput input)
    {
        _icons.Mode = input.Settings.GraphicsMode;
        if (!Open) return;
        _page.Visible = !_actions.Open;
        if (_actions.Open) { _actions.Present(); return; }
        _session = session; _input = input;
        if (!session.Current.CanWaitAtInput || session.QueryPartyDetails() is not { } party) { Hide(); return; }
        var entries = party.Members.Select(member =>
        {
            bool available = member.Deployment is not null && member.Carried.Initialized != false;
            string condition = !available ? GameText.Get("party.value-unavailable") :
                member.Carried.Absent ? GameText.Get("party.absent") : member.Carried.Hp == 0 ? GameText.Get("party.defeated") : "";
            return new MemberRosterEntry(member.Carried.Actor, MemberName(member, session),
                condition, available, PortraitFor(session, member));
        }).ToArray();
        _shown = party;
        _page.PresentRoster(entries, GameText.Get("party.member"), input);
        var member = entries.Length == 0 ? null : party.Members[_page.Index];
        _page.Details.Present(member, member is null ? GameText.Get("party.unavailable") : entries[_page.Index].Name, session);
        string unavailable = GameText.Get("party.value-unavailable");
        _page.PresentRecords(member?.Carried.Kills?.ToString() ?? unavailable, member?.Carried.Defeats?.ToString() ?? unavailable,
            party.Gold?.ToString() ?? unavailable);
        string hint = GameText.Get("party.hint", input.CompactHint(GameAction.Confirm), input.CompactHint(GameAction.Up),
            input.CompactHint(GameAction.Down), _page.FocusLabel, input.CompactHint(GameAction.Target),
            input.CompactHint(GameAction.Cancel), input.CompactHint(GameAction.Equipment),
            input.CompactHint(GameAction.Left), input.CompactHint(GameAction.Right));
        string fallback = GameText.Fallback("party.", true);
        if (fallback.Length > 0) hint += " · " + fallback;
        if (_assetFailure is { } failure) hint += "\n" + GameText.Get("prepare.failure",
            GameText.Get("prepare.recovery.unsupported"), failure.Code, failure.Field, failure.Message);
        _page.PresentFooter(hint, GameText.Get("party.close"),
            GameText.Get(_entry == "magic" ? "prepare.magic" : _entry == "item" ? "field-menu.item" : "party.prepare"));
    }

    internal static string MemberName(PartyMemberDetails member, GameSession session)
    {
        int? binding = member.Deployment?.Initialization?.AllyPartyMember;
        if (member.Deployment is null)
            return CharacterNames.Actor(session.Current.Story, member.Carried.Actor, GameText.Get("party.name-unavailable"));
        return binding is { } index ? DisplayNames.Member(session.Definition, session.Current.Story, index)
            : CharacterNames.Actor(session.Current.Story, member.Carried.Actor, session.Definition.PrivateDefinitions is null
                ? CharacterNames.ReadableIdentity(member.Carried.Actor) : GameText.Get("party.name-unavailable"));
    }

    private MemberPortrait? PortraitFor(GameSession session, PartyMemberDetails member)
    {
        var visuals = session.Definition.Exploration?.Visuals;
        int? binding = member.Deployment?.Initialization?.AllyPartyMember;
        var appearance = session.Current.Exploration?.Population?.AllySprites?.FirstOrDefault(row => row.Character == binding);
        int? sprite = appearance is null ? null : appearance.JoinedFlag is { } joined && !session.Current.Story.Flags.Contains(joined)
            ? appearance.UnjoinedSprite : appearance.ForClass(member.Carried.Progress?.ClassId);
        bool hd = _input?.Settings.GraphicsMode == "hd";
        return MemberPortrait.ForSprite(visuals, sprite, hd);
    }

    internal void Suspend() { _page.Suspend(); _actions.Suspend(); }
    private void Prepare()
    {
        if (_shown is null || _page.Count == 0 || SubmitField is null) return;
        var member = _shown.Members[_page.Index];
        _page.Suspend();
        _actions.Begin(_session!, _input!, member.Carried.Actor, MemberName(member, _session!), SubmitField, _icons, _entry);
    }
    internal void Handle(GameAction action, GameSession session, GameInput input)
    {
        if (_actions.Open) { _actions.Handle(action); _page.Suspend(); return; }
        _page.Handle(action == GameAction.Equipment ? GameAction.Cancel : action);
        Present(session, input);
    }

    internal void HandlePointer(InputEvent input)
    {
        if (_actions.Open) { _actions.HandlePointer(input); _page.Suspend(); }
    }

    internal object Observe() => _page.Observe(IsVisibleInTree(), _actions.Observe(),
        _shown?.Members.Select(member => new { actor = member.Carried.Actor.Value, hp = member.Carried.Hp, mp = member.Carried.Mp,
            exp = member.Carried.Exp, member.Carried.Kills, member.Carried.Defeats, absent = member.Carried.Absent,
            member.MaxHp, member.MaxMp, member.Level, member.Attack, member.Defense, member.Agility, member.Spells, member.Inventory }),
        _assetFailure);
}
