namespace SpaceParser9000.Core.Models;

public record UserProfile
{
    public required int Id { get; init; }
    public required string Username { get; init; }
    public required DateTime CreatedAt { get; init; }
}