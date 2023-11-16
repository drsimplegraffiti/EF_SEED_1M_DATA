Add the migration on the Data layer (db context) and not the App layer


##### Script the migration
In the package manager console
- script-migration
in rider, go to the Data layer where you have the db context, go to
- tools -> entity framework core -> Generate script

You get something like
```sql
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Leagues] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    CONSTRAINT [PK_Leagues] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Teams] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [LeagueId] int NOT NULL,
    CONSTRAINT [PK_Teams] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Teams_Leagues_LeagueId] FOREIGN KEY ([LeagueId]) REFERENCES [Leagues] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Teams_LeagueId] ON [Teams] ([LeagueId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20231114173647_Initial', N'7.0.13');
GO

COMMIT;
GO


```


##### Use dotnet cli
>  dotnet ef migrations script -o ab.sql


##### Connect to dbcontext
```csharp
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
    
    public DbSet<Team> Teams { get; set; }
    public DbSet<League> Leagues { get; set; }
}
```



###### Program.cs with async/await
```csharp
using EntityAppConsole.Data;
using EntityAppConsole.Domain;

namespace EntityAppConsole.App;

class Program 
{
    private static readonly FootballLeagueDbContext _context = new FootballLeagueDbContext();
    static async Task Main(string[] args)
    {
        await _context.Leagues.AddAsync(new League{Name = "James"});
        await _context.SaveChangesAsync();
        Console.WriteLine("Press any key to end");
        Console.ReadKey();
    }

}
```


###### Separate
```csharp
using EntityAppConsole.Data;
using EntityAppConsole.Domain;

namespace EntityAppConsole.App;

class Program 
{
    private static readonly FootballLeagueDbContext _context = new FootballLeagueDbContext();
    static async Task Main(string[] args)
    {
        var league = new League() { Name = "La Liga" };
        await _context.Leagues.AddAsync(league);
        await _context.SaveChangesAsync();
        Console.WriteLine("Press any key to end");
        Console.ReadKey();
    }

}
```

##### Bulk insert
```csharp
using EntityAppConsole.Data;
using EntityAppConsole.Domain;

namespace EntityAppConsole.App;

class Program 
{
    private static readonly FootballLeagueDbContext _context = new FootballLeagueDbContext();
    static async Task Main(string[] args)
    {
        var league = new League() { Name = "La Liga" };
        await _context.Leagues.AddAsync(league);
        await _context.SaveChangesAsync();

        await AddTeamsWithLeague(league);
        await _context.SaveChangesAsync();
        Console.WriteLine("Press any key to end");
        Console.ReadKey();
    }

    static async Task AddTeamsWithLeague(League league)
    {
        var teams = new List<Team>
        {
            new Team()
            {
                Name = "Juventus",
                LeagueId = league.Id
            },
            new Team()
            {
                Name = "Barcelona",
                LeagueId = league.Id
            },
            new Team()
            {
                Name = "Liverpool",
                League = league
            },
        };

        await _context.Teams.AddRangeAsync(teams);
    }

}
```

---

##### simple read operation
```csharp
using EntityAppConsole.Data;
using EntityAppConsole.Domain;

namespace EntityAppConsole.App;

class Program 
{
    private static readonly FootballLeagueDbContext _context = new FootballLeagueDbContext();
    static async Task Main(string[] args)
    {
       SimpleSelectQuery();
        Console.WriteLine("Press any key to end");
        Console.ReadLine();
    }

    static void SimpleSelectQuery()
    {
        var leagues = _context.Leagues.ToList();
        foreach (var league in leagues)
        {
            Console.WriteLine($"{league.Id}:{league.Name}");
        }
    }
    
}
```


---

###### Async operation for select all
```csharp

using EntityAppConsole.Data;
using EntityAppConsole.Domain;
using Microsoft.EntityFrameworkCore;

namespace EntityAppConsole.App;

class Program 
{
    private static readonly FootballLeagueDbContext _context = new FootballLeagueDbContext();
    static async Task Main(string[] args)
    {
       SimpleSelectQuery();
        Console.WriteLine("Press any key to end");
        Console.ReadKey();
    }

    static async Task SimpleSelectQuery()
    {
        var leagues = await _context.Leagues.ToListAsync();
        foreach (var league in leagues)
        {
            Console.WriteLine($"{league.Id}:{league.Name}");
        }
    }
    
}
```



###### Query Filter
```csharp
using EntityAppConsole.Data;
using EntityAppConsole.Domain;
using Microsoft.EntityFrameworkCore;

namespace EntityAppConsole.App;

class Program 
{
    private static readonly FootballLeagueDbContext _context = new FootballLeagueDbContext();
    static async Task Main(string[] args)
    {

        await QueryFilter();
        Console.WriteLine("Press any key to end");
        Console.ReadKey();
    }


    private static async Task QueryFilter()
    {
        var leagues = await _context.Leagues.Where(q => q.Name == "La Liga").ToListAsync();
        foreach (var league in leagues)
        {
            Console.WriteLine($"{league.Id}: {league.Name}");
        }
    }
   
}
```

##### Using the readline
```csharp
using EntityAppConsole.Data;
using EntityAppConsole.Domain;
using Microsoft.EntityFrameworkCore;

namespace EntityAppConsole.App;

class Program 
{
    private static readonly FootballLeagueDbContext _context = new FootballLeagueDbContext();
    static async Task Main(string[] args)
    {

        await QueryFilter();
        Console.WriteLine("Press any key to end");
        Console.ReadKey();
    }


    private static async Task QueryFilter()
    {
        Console.WriteLine("Enter league name: ");
        var leagueName = Console.ReadLine();
        var leagues = await _context.Leagues.Where(q => q.Name == leagueName).ToListAsync();
        foreach (var league in leagues)
        {
            Console.WriteLine($"{league.Id}: {league.Name}");
        }
    }
   
}
```


---


###### Using c# Equals
```csharp
using EntityAppConsole.Data;
using EntityAppConsole.Domain;
using Microsoft.EntityFrameworkCore;

namespace EntityAppConsole.App;

class Program 
{
    private static readonly FootballLeagueDbContext _context = new FootballLeagueDbContext();
    static async Task Main(string[] args)
    {

        await QueryFilter();
        Console.WriteLine("Press any key to end");
        Console.ReadKey();
    }


    private static async Task QueryFilter()
    {
        Console.WriteLine("Enter league name: ");
        var leagueName = Console.ReadLine();
        var leagues = await _context.Leagues.Where(q => q.Name.Equals(leagueName)).ToListAsync();
        foreach (var league in leagues)
        {
            Console.WriteLine($"{league.Id}: {league.Name}");
        }
    }
   
}
```

##### Using contain in LINQ
```csharp
using EntityAppConsole.Data;
using EntityAppConsole.Domain;
using Microsoft.EntityFrameworkCore;

namespace EntityAppConsole.App;

class Program 
{
    private static readonly FootballLeagueDbContext _context = new FootballLeagueDbContext();
    static async Task Main(string[] args)
    {

        await QueryFilter();
        Console.WriteLine("Press any key to end");
        Console.ReadKey();
    }


    private static async Task QueryFilter()
    {
        Console.WriteLine("Enter league name or (Part of Name): ");
        var leagueName = Console.ReadLine();
        var leagues = await _context.Leagues.Where(q => q.Name.Contains(leagueName)).ToListAsync();
        foreach (var league in leagues)
        {
            Console.WriteLine($"{league.Id}: {league.Name}");
        }
    }
   
}
```


##### Use EF Functions
```csharp
using EntityAppConsole.Data;
using EntityAppConsole.Domain;
using Microsoft.EntityFrameworkCore;

namespace EntityAppConsole.App;

class Program 
{
    private static readonly FootballLeagueDbContext _context = new FootballLeagueDbContext();
    static async Task Main(string[] args)
    {

        await QueryFilter();
        Console.WriteLine("Press any key to end");
        Console.ReadKey();
    }


    private static async Task QueryFilter()
    {
        Console.WriteLine("Enter league name or (Part of Name): ");
        var leagueName = Console.ReadLine();
        var leagues = await _context.Leagues.Where(q => EF.Functions.Like(q.Name, $"%{leagueName}%")).ToListAsync();
        foreach (var league in leagues)
        {
            Console.WriteLine($"{league.Id}: {league.Name}");
        }
    }
   
}
```


##### Aggregate function
```csharp
using EntityAppConsole.Data;
using EntityAppConsole.Domain;
using Microsoft.EntityFrameworkCore;

namespace EntityAppConsole.App;

class Program 
{
    private static readonly FootballLeagueDbContext _context = new FootballLeagueDbContext();
    static async Task Main(string[] args)
    {

        await AdditionalExecutionMethods();
        Console.WriteLine("Press any key to end");
        Console.ReadKey();
    }


    private static async Task AdditionalExecutionMethods()
    {
        // var l = await _context.Leagues.Where(q => q.Name.Contains("A")).FirstOrDefaultAsync();
        // var l = await _context.Leagues.FirstOrDefaultAsync(q => q.Name.Contains("A"));
        var leagues = _context.Leagues;
        var list = await leagues.ToListAsync();
        var first = await leagues.FirstAsync(); // return error if no record is found
        var firstOrDefault = await leagues.FirstOrDefaultAsync();
        
        var single = await leagues.SingleAsync();
        var singleOrDefault = await leagues.SingleOrDefaultAsync();
        
        var count = await leagues.CountAsync();
        var longCount = await leagues.LongCountAsync();
        
        var min = await leagues.MinAsync();
        var max = await leagues.MaxAsync();

        var league = await leagues.FindAsync(1);  // the key has to be the primary key
    }
   
}
```

---

##### Alternative Linq Syntax
```csharp
using EntityAppConsole.Data;
using EntityAppConsole.Domain;
using Microsoft.EntityFrameworkCore;

namespace EntityAppConsole.App;

class Program 
{
    private static readonly FootballLeagueDbContext _context = new FootballLeagueDbContext();
    static async Task Main(string[] args)
    {
        AlternativeLinqSyntax();
        Console.WriteLine("Press any key to end");
        Console.ReadKey();
    }

    static void AlternativeLinqSyntax()
    {
        var teams = from i in _context.Teams select i;
        foreach (var team in teams)
        {
            Console.WriteLine($"{team.Id}:{team.Name}");
        }
    }
   
   
}
```

##### Adding Async
```csharp
using EntityAppConsole.Data;
using EntityAppConsole.Domain;
using Microsoft.EntityFrameworkCore;

namespace EntityAppConsole.App;

class Program 
{
    private static readonly FootballLeagueDbContext _context = new FootballLeagueDbContext();
    static async Task Main(string[] args)
    {
        await AlternativeLinqSyntax();
        Console.WriteLine("Press any key to end");
        Console.ReadKey();
    }

    static async Task AlternativeLinqSyntax()
    {
        var teams = await ( from i in _context.Teams select i).ToListAsync();
        foreach (var team in teams)
        {
            Console.WriteLine($"{team.Id}:{team.Name}");
        }
    }
   
   
}
```


##### WHERE
```csharp
using EntityAppConsole.Data;
using EntityAppConsole.Domain;
using Microsoft.EntityFrameworkCore;

namespace EntityAppConsole.App;

class Program
{
    private static readonly FootballLeagueDbContext _context = new FootballLeagueDbContext();

    static async Task Main(string[] args)
    {
        await AlternativeLinqSyntax();
        Console.WriteLine("Press any key to end");
        Console.ReadKey();
    }

    static async Task AlternativeLinqSyntax()
    {
        Console.WriteLine("Enter league name or (Part of Name): ");
        var leagueName = Console.ReadLine();

        var teams = await (
            from i in _context.Teams
            where EF.Functions.Like(i.Name, $"%{leagueName}%")
            select i).ToListAsync();
        foreach (var team in teams)
        {
            Console.WriteLine($"{team.Id}:{team.Name}");
        }
    }
}
```

##### Update
```csharp
using EntityAppConsole.Data;
using EntityAppConsole.Domain;
using Microsoft.EntityFrameworkCore;

namespace EntityAppConsole.App;

class Program
{
    private static readonly FootballLeagueDbContext _context = new FootballLeagueDbContext();

    static async Task Main(string[] args)
    {
        await UpdateRecord();
        Console.WriteLine("Press any key to end");
        Console.ReadKey();
    }

    private static async Task GetRecord()
    {
        var league = await _context.Leagues.FindAsync(2);
        Console.WriteLine($"{league.Id}:{league.Name}");
    }

    private static async Task  UpdateRecord()
    {
        // Retrieve record
        var league = await _context.Leagues.FindAsync(2);
        
        //Make changes
        league.Name = "Bundelisga";
        
        // Save changes
        await _context.SaveChangesAsync();
        
        // Get the updated record
        await GetRecord();


    }
}
```

###### Method 2 for update
```csharp
using EntityAppConsole.Data;
using EntityAppConsole.Domain;
using Microsoft.EntityFrameworkCore;

namespace EntityAppConsole.App;

class Program
{
    private static readonly FootballLeagueDbContext _context = new FootballLeagueDbContext();

    static async Task Main(string[] args)
    {
        await UpdateRecordMethod2();
        Console.WriteLine("Press any key to end");
        Console.ReadKey();
    }

    private static async Task GetRecord()
    {
        var league = await _context.Leagues.FindAsync(2);
        Console.WriteLine($"{league.Id}:{league.Name}");
    }

    private static async Task  UpdateRecordMethod2()
    {
        var team = new Team
        {
            Id = 3,
            Name = "Enyimba",
            LeagueId = 1
        };

        _context.Teams.Update(team);
        await _context.SaveChangesAsync();
    }
}
```

##### Simple Delete
```csharp
using EntityAppConsole.Data;
using EntityAppConsole.Domain;
using Microsoft.EntityFrameworkCore;

namespace EntityAppConsole.App;

class Program
{
    private static readonly FootballLeagueDbContext _context = new FootballLeagueDbContext();

    static async Task Main(string[] args)
    {
        await SimpleDelete();
        Console.WriteLine("Press any key to end");
        Console.ReadKey();
    }

    private static async Task SimpleDelete()
    {
        var league = await _context.Leagues.FindAsync(4);
        _context.Leagues.Remove(league);
        await _context.SaveChangesAsync();
    }
}
```

##### Delete with cascade
```csharp
using EntityAppConsole.Data;
using EntityAppConsole.Domain;
using Microsoft.EntityFrameworkCore;

namespace EntityAppConsole.App;

class Program
{
    private static readonly FootballLeagueDbContext _context = new FootballLeagueDbContext();

    static async Task Main(string[] args)
    {
        await DeleteWithRelationship();
        Console.WriteLine("Press any key to end");
        Console.ReadKey();
    }

    private static async Task DeleteWithRelationship()
    {
        var league = await _context.Leagues.FindAsync(2); // because 2 is a foreign key on the Teams table, it will delete all related records
        _context.Leagues.Remove(league);
        await _context.SaveChangesAsync();
    }
}
```

##### AsnoTracking vs Tracking
```csharp
using EntityAppConsole.Data;
using EntityAppConsole.Domain;
using Microsoft.EntityFrameworkCore;

namespace EntityAppConsole.App;

class Program
{
    private static readonly FootballLeagueDbContext _context = new FootballLeagueDbContext();

    static async Task Main(string[] args)
    {
        await TrackingVsNoTracking();
        Console.WriteLine("Press any key to end");
        Console.ReadKey();
    }

    private static async Task TrackingVsNoTracking()
    {
        var withTracking = await _context.Teams.FirstOrDefaultAsync(q => q.Id == 3);
        var withNoTracking = await _context.Teams.AsNoTracking().FirstOrDefaultAsync(q => q.Id == 4);
        // asNotracking does not work with FindAsync
        // AsNoTracking useful for large read operation

        withTracking.Name = "Manchester1";
        withNoTracking.Name = "Arsenal1";

        var entiresBeforeSave = _context.ChangeTracker.Entries();
        await _context.SaveChangesAsync();
        var entiresAfterSave = _context.ChangeTracker.Entries();
        await _context.SaveChangesAsync();
    }
}
```


##### Add new teams with leagueId
```csharp
using EntityAppConsole.Data;
using EntityAppConsole.Domain;
using Microsoft.EntityFrameworkCore;

namespace EntityAppConsole.App;

class Program
{
    private static readonly FootballLeagueDbContext _context = new FootballLeagueDbContext();

    static async Task Main(string[] args)
    {
        await AddNewTeamWithLeagueId();
        Console.WriteLine("Press any key to end");
        Console.ReadKey();
    }


    static async Task AddNewTeamWithLeagueId()
    {
        var team = new Team
        {
            Name = "Enyimba FC",
            LeagueId = 1
        };
        await _context.Teams.AddAsync(team);
        await _context.SaveChangesAsync();
    }
   
}
```

##### Insert using navigation property
```csharp
using EntityAppConsole.Data;
using EntityAppConsole.Domain;
using Microsoft.EntityFrameworkCore;

namespace EntityAppConsole.App;

class Program
{
    private static readonly FootballLeagueDbContext _context = new FootballLeagueDbContext();

    static async Task Main(string[] args)
    {
        await AddNewTeamWithLeagueId();
        Console.WriteLine("Press any key to end");
        Console.ReadKey();
    }


    static async Task AddNewTeamWithLeagueId()
    {
        var league = new League()
        {
            Name = "Seria A"
        };
        var team = new Team
        {
            Name = "Enyimba FC",
            League = league
           
        };
        await _context.Teams.AddAsync(team);
        await _context.SaveChangesAsync();
    }
   
}
```


```csharp
using EntityAppConsole.Data;
using EntityAppConsole.Domain;
using Microsoft.EntityFrameworkCore;

namespace EntityAppConsole.App;

class Program
{
    private static readonly FootballLeagueDbContext _context = new FootballLeagueDbContext();

    static async Task Main(string[] args)
    {
        await AddNewLeagueWithTeams();
        Console.WriteLine("Press any key to end");
        Console.ReadKey();
    }


    static async Task AddNewLeagueWithTeams()
    {
        var teams = new List<Team>()
        {
           new Team(){Name="BARCA"},
           new Team(){Name="Bayern"},
        };
        var league = new League()
        {
            Name = "Bundelisga",
            Teams = teams
           
        };
        await _context.Leagues.AddAsync(league);
        await _context.SaveChangesAsync();
    }
   
}
```



