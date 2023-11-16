using EntityAppConsole.Domain.Common;

namespace EntityAppConsole.Domain;

public class Match:BaseDomainObject 
{
    public int HomeTeamId { get; set; }
    public virtual Team HomeTeam { get; set; } // navigation property
    
    public int AwayTeamId { get; set; }
    public virtual Team AwayTeam { get; set; } // navigation property
    
    public DateTime Date { get; set; }
}