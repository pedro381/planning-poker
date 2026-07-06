using Poker.Models;

namespace Poker.Services;

public sealed class PlanningPokerService
{
    private readonly object _syncRoot = new();
    private readonly Dictionary<string, PokerRoom> _rooms = new(StringComparer.OrdinalIgnoreCase);

    public event Action? RoomsChanged;

    public IReadOnlyList<string> VotingCards { get; } = [
        "0", "1", "2", "3", "5", "8", "13", "21", "34", "55", "89", "?", "☕"
    ];

    public (string roomCode, string participantId) CreateRoom(string userName)
    {
        var roomCode = GenerateRoomCode();
        var participantId = Guid.NewGuid().ToString("N");

        lock (_syncRoot)
        {
            var room = new PokerRoom { Code = roomCode };
            room.Participants.Add(new Participant
            {
                Id = participantId,
                Name = userName.Trim()
            });

            _rooms[roomCode] = room;
        }

        NotifyChanged();
        return (roomCode, participantId);
    }

    public bool JoinRoom(string roomCode, string userName, string? participantId, out string resolvedParticipantId)
    {
        resolvedParticipantId = string.Empty;
        var normalizedRoomCode = NormalizeRoomCode(roomCode);

        lock (_syncRoot)
        {
            if (!_rooms.TryGetValue(normalizedRoomCode, out var room))
            {
                return false;
            }

            resolvedParticipantId = string.IsNullOrWhiteSpace(participantId)
                ? Guid.NewGuid().ToString("N")
                : participantId;

            var participantIdForLookup = resolvedParticipantId;
            var participant = room.Participants.FirstOrDefault(p => p.Id == participantIdForLookup);
            if (participant is null)
            {
                room.Participants.Add(new Participant
                {
                    Id = resolvedParticipantId,
                    Name = userName.Trim()
                });
            }
            else
            {
                participant.Name = userName.Trim();
            }
        }

        NotifyChanged();
        return true;
    }

    public PokerRoom? GetRoom(string roomCode)
    {
        var normalizedRoomCode = NormalizeRoomCode(roomCode);

        lock (_syncRoot)
        {
            if (!_rooms.TryGetValue(normalizedRoomCode, out var room))
            {
                return null;
            }

            CleanupExpiredPoopThrows(room);

            return CloneRoom(room);
        }
    }

    public void ThrowEmoji(string roomCode, string targetParticipantId, string emoji)
    {
        var normalizedRoomCode = NormalizeRoomCode(roomCode);
        var normalizedEmoji = string.IsNullOrWhiteSpace(emoji) ? "💩" : emoji.Trim();

        lock (_syncRoot)
        {
            if (!_rooms.TryGetValue(normalizedRoomCode, out var room))
            {
                return;
            }

            if (!room.Participants.Any(p => p.Id == targetParticipantId))
            {
                return;
            }

            CleanupExpiredPoopThrows(room);
            room.PoopThrows.Add(CreateRandomPoopThrow(targetParticipantId, normalizedEmoji));
        }

        NotifyChanged();
    }

    public void ThrowPoop(string roomCode, string targetParticipantId)
    {
        ThrowEmoji(roomCode, targetParticipantId, "💩");
    }

    public void SetVote(string roomCode, string participantId, string vote)
    {
        var normalizedRoomCode = NormalizeRoomCode(roomCode);

        lock (_syncRoot)
        {
            if (!_rooms.TryGetValue(normalizedRoomCode, out var room))
            {
                return;
            }

            var participant = room.Participants.FirstOrDefault(p => p.Id == participantId);
            if (participant is null)
            {
                return;
            }

            participant.Vote = vote;
        }

        NotifyChanged();
    }

    public void RevealVotes(string roomCode)
    {
        var normalizedRoomCode = NormalizeRoomCode(roomCode);

        lock (_syncRoot)
        {
            if (_rooms.TryGetValue(normalizedRoomCode, out var room))
            {
                room.VotesRevealed = true;
            }
        }

        NotifyChanged();
    }

    public void StartNewRound(string roomCode)
    {
        var normalizedRoomCode = NormalizeRoomCode(roomCode);

        lock (_syncRoot)
        {
            if (!_rooms.TryGetValue(normalizedRoomCode, out var room))
            {
                return;
            }

            room.VotesRevealed = false;
            foreach (var participant in room.Participants)
            {
                participant.Vote = null;
            }
        }

        NotifyChanged();
    }

    private string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        string roomCode;
        do
        {
            roomCode = string.Create(6, chars, static (buffer, source) =>
            {
                for (var i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = source[Random.Shared.Next(source.Length)];
                }
            });
        }
        while (_rooms.ContainsKey(roomCode));

        return roomCode;
    }

    private static string NormalizeRoomCode(string roomCode)
    {
        return roomCode.Trim().ToUpperInvariant();
    }

    private static PokerRoom CloneRoom(PokerRoom room)
    {
        var clone = new PokerRoom
        {
            Code = room.Code,
            VotesRevealed = room.VotesRevealed
        };

        foreach (var participant in room.Participants)
        {
            clone.Participants.Add(new Participant
            {
                Id = participant.Id,
                Name = participant.Name,
                Vote = participant.Vote
            });
        }

        foreach (var poopThrow in room.PoopThrows)
        {
            clone.PoopThrows.Add(new PoopThrow
            {
                Id = poopThrow.Id,
                Emoji = poopThrow.Emoji,
                TargetParticipantId = poopThrow.TargetParticipantId,
                CreatedAtUtc = poopThrow.CreatedAtUtc,
                DurationMs = poopThrow.DurationMs,
                FromXVw = poopThrow.FromXVw,
                FromYVh = poopThrow.FromYVh,
                FromRotationDeg = poopThrow.FromRotationDeg
            });
        }

        return clone;
    }

    private static PoopThrow CreateRandomPoopThrow(string targetParticipantId, string emoji)
    {
        var side = Random.Shared.Next(4);
        var randomX = Random.Shared.NextDouble() * 100d - 50d;
        var randomY = Random.Shared.NextDouble() * 100d - 50d;

        var (fromXVw, fromYVh) = side switch
        {
            0 => (-110d, randomY),
            1 => (110d, randomY),
            2 => (randomX, -110d),
            _ => (randomX, 110d)
        };

        return new PoopThrow
        {
            Id = Guid.NewGuid().ToString("N"),
            Emoji = emoji,
            TargetParticipantId = targetParticipantId,
            CreatedAtUtc = DateTime.UtcNow,
            DurationMs = 900 + Random.Shared.Next(0, 450),
            FromXVw = fromXVw,
            FromYVh = fromYVh,
            FromRotationDeg = Random.Shared.NextDouble() * 70d - 35d
        };
    }

    private static void CleanupExpiredPoopThrows(PokerRoom room)
    {
        var now = DateTime.UtcNow;
        room.PoopThrows.RemoveAll(t => !t.IsActive(now));
    }

    private void NotifyChanged() => RoomsChanged?.Invoke();
}
