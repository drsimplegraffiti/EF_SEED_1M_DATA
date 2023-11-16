using System.Diagnostics;
using Bogus;
using EntityAppConsole.Data;
using EntityAppConsole.Domain;
using Microsoft.EntityFrameworkCore;

namespace EntityAppConsole.App;

class Program
{
    private static readonly FootballLeagueDbContext _context = new FootballLeagueDbContext();

    static async Task Main(string[] args)
    {
        // create database
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
        
        // setup bogus faker
        var faker = new Faker<Product>();
        faker.RuleFor(p => p.Code, f => f.Commerce.Ean13());
        faker.RuleFor(p => p.Description, f => f.Commerce.ProductName());
        faker.RuleFor(p => p.Category, f => f.Commerce.Categories(1)[0]);
        faker.RuleFor(p => p.Price, f => f.Random.Decimal(1, 1000));

// generate 1 million products
        var products = faker.Generate(1_000_000);

        var batches = products
            .Select((p, i) => (Product: p, Index: i))
            .GroupBy(x => x.Index / 100_000)
            .Select(g => g.Select(x => x.Product).ToList())
            .ToList();

// insert batches
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var count = 0;
        foreach (var batch in batches)
        {
            count++;
            Console.WriteLine($"Inserting batch {count} of {batches.Count}...");

            await _context.Products.AddRangeAsync(batch);
            await _context.SaveChangesAsync();
        }

        stopwatch.Stop();

        Console.WriteLine($"Elapsed time: {stopwatch.Elapsed}");
        Console.WriteLine("Press any key to exit...");
        await AddNewMatches();
        Console.WriteLine("Press any key to end");
        Console.ReadKey();
    }


    static async Task AddNewMatches()
    {
        var matches = new List<Match>()
        {
           new Match(){AwayTeamId= 1, HomeTeamId = 2, Date = new DateTime()},
           new Match(){AwayTeamId= 2, HomeTeamId = 3, Date = new DateTime()},
        };
        await _context.Matches.AddRangeAsync(matches);
        await _context.SaveChangesAsync();
    }
   
}