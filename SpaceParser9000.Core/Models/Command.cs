namespace SpaceParser9000.Core.Models;

public readonly ref struct Command
{
    public Command(ReadOnlySpan<char> commandName, ReadOnlySpan<char> key, ReadOnlySpan<char> value)
    {
        CommandName = commandName;
        Key = key;
        Value = value;
    }

    public ReadOnlySpan<char> CommandName { get; }
    public ReadOnlySpan<char> Key { get; }
    public ReadOnlySpan<char> Value { get; }
}