using EntityAppConsole.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EntityAppConsole.Data;

public class FootballLeagueDbContext: DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=localhost,1433;Database=EF_CONSOLE;User Id=SA;Password=Bassguitar1;Encrypt=false;TrustServerCertificate=True;")
            .LogTo(Console.WriteLine, new []{DbLoggerCategory.Database.Command.Name}, LogLevel.Information)
            .EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Team>()
            .HasMany(m => m.HomeMatches)
            .WithOne(m => m.HomeTeam)
            .HasForeignKey(m => m.HomeTeamId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict); 
            
        modelBuilder.Entity<Team>()
            .HasMany(m => m.AwayMatches)
            .WithOne(m => m.AwayTeam)
            .HasForeignKey(m => m.AwayTeamId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<League>()
            .HasData(
                new League()
                {
                    Name = "Premier League",
                    Id=1
                }
            );

        modelBuilder.Entity<Team>()
            .HasData(
                new Team()
                {
                    Id=1,
                    Name = "Barcelona",
                    LeagueId = 1
                }
            );
    }
    
    public DbSet<Product> Products { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<League> Leagues { get; set; }
    public DbSet<Match> Matches { get; set; }
    public DbSet<Coach> Coaches { get; set; }
}