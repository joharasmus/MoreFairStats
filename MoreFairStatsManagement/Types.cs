using MoreFairStats;

namespace MoreFairStatsManagement;
public class APIRoundStats
{
    public Dictionary<string, LadderStats>? Ladders { get; set; }
    public List<string>? RoundTypes { get; set; }
    public string? BasePointsToPromote { get; set; }
    public string? CreatedOn { get; set; }
    public string? ClosedOn { get; set; }
    public int Number { get; set; }
}
