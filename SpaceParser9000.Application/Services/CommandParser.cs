using SpaceParser9000.Core.Models;

namespace SpaceParser9000.Application.Services;

public static class CommandParser
{
    private const char CommandSeparator = ' ';
    
    public static Command Parse(ReadOnlySpan<char> input)
    {
        input = input.Trim();
        
        int spacePointer = 0;
        int wordLength = input.IndexOf(CommandSeparator);
        if (wordLength < 0)
            wordLength = input.Length;
        var commandName = input.Slice( spacePointer, wordLength);

        bool keyResult = TryMovePointerToNextWord(input, ref spacePointer, ref wordLength);
        var key = keyResult
            ? input.Slice(spacePointer, wordLength)
            : string.Empty;
        
        bool valueResult = TryMovePointerToNextWord(input, ref spacePointer, ref wordLength);
        var value = valueResult
            ? input.Slice( spacePointer, wordLength)
            : string.Empty;
        
        if (commandName.IsEmpty || key.IsEmpty)
            return default;
        
        return new Command(commandName, key, value);
    }

    private static bool TryMovePointerToNextWord(ReadOnlySpan<char> input, ref int spacePointer, ref int wordLength)
    {
        //используется цикл do while для кейса, когда есть лишние пробелы
        bool isSpace;
        do
        {
            spacePointer += wordLength + 1;
            if (spacePointer >= input.Length)
            {
                if (spacePointer == input.Length)
                {
                    spacePointer--;
                    wordLength = 1;
                    return true;
                }

                return false;
            }
            wordLength = input.Slice(spacePointer).IndexOf(CommandSeparator) + 1;
            if (wordLength == 0)
            {
                wordLength = input.Slice(spacePointer).Length;
                break;
            }

            isSpace = wordLength == 1 && input[spacePointer] == CommandSeparator;
            
        } while (isSpace);

        return true;
    }
}