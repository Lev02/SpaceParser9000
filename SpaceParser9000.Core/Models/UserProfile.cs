using SpaceParser9000.Generator;

namespace SpaceParser9000.Core.Models;

[GenerateBinarySerializer]
public partial record UserProfile
{
    public required int Id { get; init; }
    public required string Username { get; init; }
    public required DateTime CreatedAt { get; init; }
}