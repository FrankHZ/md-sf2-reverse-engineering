namespace Sf2.Remake.Application.Runtime;

// The ordinary product start is Normal. Empty input retains the immutable source default.
public sealed record NewGameOptions(string ProtagonistName = "");

public static class NewGameName
{
    public const int MaximumLength = 7;
    public const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789,.-!?#' ";

    public static string? Error(string? value) => value is null ? "name-required" :
        value.Length > MaximumLength ? "name-too-long" :
        value.Any(character => !Alphabet.Contains(character)) ? "name-character" : null;
}
