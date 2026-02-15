using SpaceParser9000.Core.Models;

namespace SpaceParser9000.Application.Extensions;

public static class CommandExtensions
{
    public static void SendToCommandLine(this Command command)
    {
        string commandName = command.CommandName.ToString();
        string key = command.Key.ToString();
        string value = command.Value.ToString();

        if (commandName == string.Empty
            && key == string.Empty
            && value == string.Empty)
        {
            Console.WriteLine("Empty command");
        }
        
        Console.WriteLine($"Command name: {commandName}, key: {key}, value: {value}");
    }
}