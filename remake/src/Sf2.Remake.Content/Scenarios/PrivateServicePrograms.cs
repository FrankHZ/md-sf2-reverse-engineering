using Sf2.Remake.Application.Content.Scenarios;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

// Private exports encode the native menu calls with their terms. Keep every original
// instruction address valid and let the ordinary program interpreter run the native entry.
internal static class PrivateServicePrograms
{
    internal static void Bind(List<StoryProgram> programs, IReadOnlyDictionary<int, string> texts)
    {
        var ids = programs.Select(program => program.Id).ToHashSet(StringComparer.Ordinal);
        int callerCount = programs.Count;
        for (int p = 0; p < callerCount; p++)
        {
            var caller = programs[p];
            if (!caller.Instructions.Any(op => op is OpenChurch or OpenShop)) continue;
            var instructions = caller.Instructions.ToArray();
            for (int i = 0; i < instructions.Length; i++)
            {
                var service = instructions[i];
                if (service is not (OpenChurch or OpenShop)) continue;
                int text = service is OpenChurch ? 110 : 158;
                string id = $"{caller.Id}-service-{i}";
                Require(texts.ContainsKey(text) && ids.Add(id), "service-entry-binding", "world.programs");
                programs.Add(new(id, [
                    new OpenPortrait(null, UseEventSpeaker: true),
                    new ShowText(TextDisplayMode.Single, null, UseEventSpeaker: true,
                        WaitForAcknowledgement: false, ExplicitWindows: true, TextId: text),
                    new CloseText(), new ClosePortrait(), service, new ReturnProgram()
                ], caller.Source, caller.EntitiesRunning));
                instructions[i] = new CallProgram(new(id, 0));
            }
            programs[p] = new(caller.Id, instructions, caller.Source, caller.EntitiesRunning);
        }
    }
}
