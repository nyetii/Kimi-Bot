namespace Kimi.Repository.Dtos;

public record UserScoreDto
{
    public ulong UserId { get; init; }
    public string Nickname { get; init; }
    public uint Score { get; init; }
    public uint MessageCount { get; init; }

    public UserScoreDto(ulong userId, string nickname, uint score, uint messageCount)
    {
        UserId = userId;
        Nickname = nickname;
        Score = score;
        MessageCount = messageCount;
    }
}