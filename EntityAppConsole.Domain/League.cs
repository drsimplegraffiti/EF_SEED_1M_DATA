using EntityAppConsole.Domain.Common;

namespace EntityAppConsole.Domain;

public class League: BaseDomainObject
{
    public string Name { get; set; }
    public List<Team> Teams { get; set; }
}