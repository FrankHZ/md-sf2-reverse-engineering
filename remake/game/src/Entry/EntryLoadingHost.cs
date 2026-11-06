using Godot;
using Sf2.Remake.GodotAdapter.Localization;

namespace Sf2.Remake.GodotAdapter;

public sealed partial class GameRoot
{
    private bool _entryLoading;
    private bool _entryQuitRequested;

    // Only detached content/session/disk work belongs here. The caller captures configured
    // paths on the host thread and publishes nodes, labels and audio after this returns.
    private async Task<T> LoadEntry<T>(Func<T> work, bool publishing = false)
    {
        _entryLoading = true;
        var panel = _slots!;
        var entry = _entry;
        var session = _session;
        var tree = GetTree();
        bool exited = false;
        void Exiting() => exited = true;
        TreeExiting += Exiting;
        var back = panel.Cancel;
        bool returning = false;
        bool Current() => !exited && GodotObject.IsInstanceValid(this) && !IsQueuedForDeletion() && IsInsideTree() &&
            GodotObject.IsInstanceValid(panel) && !panel.IsQueuedForDeletion() && panel.IsInsideTree() &&
            _slots == panel && _entry == entry && _session == session;
        panel.ShowLoading(GameText.Get(publishing ? "entry.publishing" : "entry.loading"), publishing ? null : () =>
        {
            returning = true;
            panel.ShowDetail(GameText.Get("entry.returning"));
        });
        DisarmInput();
        try
        {
            // Let the loading feedback reach a rendered frame before starting admission.
            await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
            await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
            if (!Current()) throw new OperationCanceledException();
            var pending = Task.Run(work);
            // Resume on the Godot thread, without relying on a worker's synchronization context.
            while (!pending.IsCompleted)
                await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
            T result;
            try { result = pending.GetAwaiter().GetResult(); }
            catch when (!Current() || returning || _entryQuitRequested)
            { throw new OperationCanceledException(); }
            if (!Current()) throw new OperationCanceledException();
            if (_entryQuitRequested || returning) throw new OperationCanceledException();
            // A window recovery/quit cover retains this operation's result until its
            // original panel is usable. No completion mounts behind a recovery modal.
            while (_windowRecovery is not null || _windowQuit is not null)
            {
                await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
                if (!Current()) throw new OperationCanceledException();
                if (_entryQuitRequested) throw new OperationCanceledException();
            }
            return result;
        }
        finally
        {
            if (GodotObject.IsInstanceValid(this)) TreeExiting -= Exiting;
            _entryLoading = false;
            if (Current())
            {
                panel.FinishLoading(); DisarmInput();
                if (_entryQuitRequested)
                {
                    _entryQuitRequested = false;
                    Callable.From(RequestQuit).CallDeferred();
                }
                else if (returning && back is not null) Callable.From(back).CallDeferred();
            }
        }
    }
}
