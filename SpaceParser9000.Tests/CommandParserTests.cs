using SpaceParser9000.Application.Services;
using Xunit;

namespace SpaceParser9000.Tests;

public class CommandParserTests
{
    [Theory]
    [InlineData("set testKey testValue")]
    [InlineData("_ _ _")]
    [InlineData("x y z")]
    public void Parse_WhenInputHasThreeArgs_SetsThreeProperties(string input)
    {
        //Act
        var command = CommandParser.Parse(input);
        
        //Assert
        Assert.False(string.IsNullOrWhiteSpace(command.CommandName.ToString()));
        Assert.False(string.IsNullOrWhiteSpace(command.Key.ToString()));
        Assert.False(string.IsNullOrWhiteSpace(command.Value.ToString()));
    }
    
    [Theory]
    [InlineData("get testKey")]
    [InlineData("_ _")]
    [InlineData("x y")]
    public void Parse_WhenInputHasTwoArgs_SetsTwoProperties(string input)
    {
        //Act
        var command = CommandParser.Parse(input);
        
        //Assert
        Assert.False(string.IsNullOrWhiteSpace(command.CommandName.ToString()));
        Assert.False(string.IsNullOrWhiteSpace(command.Key.ToString()));
        Assert.True(string.IsNullOrWhiteSpace(command.Value.ToString()));
    }
    
    [Theory]
    [InlineData("asdokasod,asd")]
    [InlineData("__")]
    [InlineData("xy")]
    public void Parse_WhenInputHasOneArg_ReturnsEmptyCommand(string input)
    {
        //Act
        var command = CommandParser.Parse(input);
        
        //Assert
        Assert.True(string.IsNullOrWhiteSpace(command.CommandName.ToString()));
        Assert.True(string.IsNullOrWhiteSpace(command.Key.ToString()));
        Assert.True(string.IsNullOrWhiteSpace(command.Value.ToString()));
    }
    
    [Theory]
    [InlineData("set  testKey   testValue")]
    [InlineData("_ _  _")]
    [InlineData("x y s")]
    public void Parse_WhenInputHasExtraSpaces_IgnoresExtraSpaces(string input)
    {
        //Act
        var command = CommandParser.Parse(input);
        
        //Assert
        Assert.False(string.IsNullOrWhiteSpace(command.CommandName.ToString()));
        Assert.False(string.IsNullOrWhiteSpace(command.Key.ToString()));
        Assert.False(string.IsNullOrWhiteSpace(command.Value.ToString()));
    }
}