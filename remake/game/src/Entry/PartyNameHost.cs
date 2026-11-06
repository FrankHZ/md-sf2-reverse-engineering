using Godot;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.GodotAdapter.Entry;

namespace Sf2.Remake.GodotAdapter;

public sealed partial class GameRoot
{
    private void RefreshPartyName()
    {
        if (_session?.Current.Story.Wait is not NameWait wait || _nameEntry is not null ||
            _entry is not null || _slots is not null || _settings is not null || _windowRecovery is not null) return;
        var snapshot = _session.Current;
        _nameEntry = new NameEntryView
        {
            Name = "NameEntryView",
            MemberName = DisplayNames.Member(_session.Definition, snapshot.Story, wait.Member),
            Draft = "",
            Done = name =>
            {
                var current = _session.Current;
                var result = _session.Submit(new(current.SessionId, current.Revision, null, new NameMember(wait.Token, name)));
                if (result.Failure is { } failure) { Fail(failure); return; }
                CloseName(); ApplyHostPause(); DisarmInput();
            },
        };
        _hostUi.AddChild(_nameEntry); ApplyHostPause(); DisarmInput();
    }
}
