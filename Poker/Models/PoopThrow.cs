namespace Poker.Models;

public sealed class PoopThrow
{
    public required string Id { get; init; }

    public required string Emoji { get; init; }

    public required string TargetParticipantId { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public int DurationMs { get; init; } = 1100;

    public double FromXVw { get; init; }

    public double FromYVh { get; init; }

    public double FromRotationDeg { get; init; }

    public bool IsActive(DateTime nowUtc)
    {
        return nowUtc - CreatedAtUtc < TimeSpan.FromMilliseconds(DurationMs + 120);
    }
}
