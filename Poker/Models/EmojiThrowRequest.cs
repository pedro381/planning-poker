namespace Poker.Models;

public sealed class EmojiThrowRequest
{
    public required string TargetParticipantId { get; init; }

    public required string Emoji { get; init; }
}
