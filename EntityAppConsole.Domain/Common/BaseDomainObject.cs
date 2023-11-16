namespace EntityAppConsole.Domain.Common;

public abstract class BaseDomainObject // abstract mean you cant instantiate it by itself
{
    public int Id { get; set; }
}