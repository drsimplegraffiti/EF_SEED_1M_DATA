using EntityAppConsole.Domain.Common;

namespace EntityAppConsole.Domain;

public class Coach: BaseDomainObject
{
    public string Name { get; set; }

    public int? TeamId { get; set; }
    public virtual Team Team { get; set; }
}