namespace Poker.Models;

public sealed class Participant
{
    public required string Id { get; init; }

    public required string Name { get; set; }

    public string? Vote { get; set; }

    public bool HasVoted => !string.IsNullOrWhiteSpace(Vote);
}
