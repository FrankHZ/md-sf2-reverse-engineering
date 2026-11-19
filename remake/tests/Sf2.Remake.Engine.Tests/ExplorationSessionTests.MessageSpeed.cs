using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed partial class ExplorationSessionTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public void ResidentRestoresTheCallersMessageSpeedAfterAWait(byte original)
    {
        var (definition, before) = ControlWorld(script: [new SaveMessageSpeed(), new SetMessageSpeed(3),
            new WaitProgramTicks(1), new RestoreMessageSpeed(), new WaitProgramTicks(1), new EndProgram()]);
        var settings = new ExplorationTextSettings(original, 0, 0);
        var story = before.Story.Copy(before.Story.Cursor, textSettings: settings,
            entityEvent: new(ControlledPlayer, 0, 1));
        var changed = ProgramRunner.Run(definition, before.WithStory(story), []);
        Assert.Null(changed.Failure);
        Assert.Equal(3, changed.Snapshot.Story.TextSettings!.MessageSpeed);
        Assert.Equal(original, changed.Snapshot.Story.EntityEvent!.SavedMessageSpeed);

        var restored = ExplorationDispatcher.Submit(definition, changed.Snapshot,
            new AdvanceSimulation(changed.Snapshot.Story.Wait!.Token));
        Assert.Null(restored.Failure);
        Assert.Equal(settings, restored.Snapshot.Story.TextSettings);
        Assert.Null(restored.Snapshot.Story.EntityEvent!.SavedMessageSpeed);
    }
}
