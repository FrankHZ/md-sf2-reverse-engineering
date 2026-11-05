namespace Sf2.Remake.GodotAdapter;

public sealed partial class GameRoot
{
    private string? _fieldEntryFailure;

    private void RequestFieldEntry(Action ready)
    {
        if (_exploration?.MenuEntryPending == true) return;
        _fieldEntryFailure = null;
        if (_exploration?.BeginMenuEntry(error =>
        {
            _fieldEntryFailure = error;
            if (error is null) ready();
            else OpenSystemMenu();
        }) == true) { DisarmInput(); return; }
        ready();
    }
}
