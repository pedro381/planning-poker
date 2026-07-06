namespace Poker.Models;

public sealed class PokerRoom
{
    public required string Code { get; init; }

    public bool VotesRevealed { get; set; }

    public List<Participant> Participants { get; } = [];

    public List<PoopThrow> PoopThrows { get; } = [];
}
