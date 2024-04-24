namespace MoreFairStats;

public class RoundStats
{
    public Dictionary<string, LadderStats>? Ladders { get; set; }
    public List<string>? RoundTypes { get; set; }
    public string? BasePointsToPromote { get; set; }
    public string? CreatedOn { get; set; }
    public string? ClosedOn { get; set; }
    public int Number { get; set; }
}

public class RoundAppearance
{
    public Dictionary<string, LadderAppearance>? Ladders { get; set; }
    public List<string>? RoundTypes { get; set; }
    public string? BasePointsToPromote { get; set; }
    public string? CreatedOn { get; set; }
    public string? ClosedOn { get; set; }
    public int Number { get; set; }
}

public class LadderStats
{
    public List<Ranker>? Rankers { get; set; }
    public string? CreatedOn { get; set; }
    public string? BasePointsToPromote { get; set; }
    public List<string>? LadderTypes { get; set; }
}

public class LadderAppearance
{
    public string? CreatedOn { get; set; }
    public string? BasePointsToPromote { get; set; }
    public List<string>? LadderTypes { get; set; }
    public int Rank { get; set; }
    public string? Points { get; set; }
    public string? Power { get; set; }
    public int Bias { get; set; }
    public int Multi { get; set; }
    public string? Grapes { get; set; }
    public string? Vinegar { get; set; }
    public bool AutoPromote { get; set; }
    public bool Growing { get; set; }
}

public class Ranker
{
    public int AccountId { get; set; }
    public string? UserName { get; set; }
    public int Rank { get; set; }
    public string? Points { get; set; }
    public string? Power { get; set; }
    public int Bias { get; set; }
    public int Multi { get; set; }
    public int AssholePoints { get; set; }
    public string? Grapes { get; set; }
    public string? Vinegar { get; set; }
    public bool AutoPromote { get; set; }
    public bool Growing { get; set; }
}

public class PlayerStats
{
    public int AccountId { get; set; }
    public string? UserName { get; set; }
    public int AHPoints { get; set; }
    public List<RoundAppearance> RoundAppearances { get; set; } = [];
}

public class ByPlayerExtension : IComparer<PlayerStats>
{
    public int Compare(PlayerStats? x, PlayerStats? y)
    {
        if (x == null || y == null) throw new ArgumentNullException();
        if (x.AccountId == y.AccountId) return 0;
        if (x.AccountId < y.AccountId) return -1;
        return 1;
    }
}

public class Config
{
    public int CurrentMaxRound { get; set; }
}