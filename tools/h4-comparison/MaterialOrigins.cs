using static H4Comparison.Operands;

namespace H4Comparison;

internal sealed class MaterialOrigins
{
    private readonly MaterialReport report = new();
    private SceneMaterials? scene;
    private AudioMaterials? audio;
    public object Handle(string op, object? message)
    {
        if (op == "materials-drain") return report.Page();
        report.Begin(discard: op == "materials-error");
        try
        {
            object? control = null;
            if (op == "materials-start")
            {
                if (scene is not null) throw new InvalidDataException("Materials already started");
                scene = new SceneMaterials(report, At(message, "pins"));
                audio = new AudioMaterials(report, scene, At(message, "pins"));
            }
            else if (op == "materials-error") report.Error(At(message, "error"));
            else
            {
                if (scene is null || audio is null) throw new InvalidDataException("Materials not started");
                if (op == "materials-scene") control = scene.Handle((string)At(message, "phase")!, message);
                else if (op == "materials-audio") control = audio.Handle((string)At(message, "phase")!, message);
                else throw new InvalidDataException("Unknown materials operation");
            }
            return report.Complete(control);
        }
        catch (MaterialFactError error) { return report.Complete(error: Dict(("fact", error.Index))); }
        catch (OperandError error) { return report.Complete(error: Dict(("kind", error.Kind), ("message", error.Detail))); }
    }
}
