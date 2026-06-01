using Aufgabe3_RestfulApi.Model;
using Bogus;

namespace Aufgabe3_RestfulApi.Infrastructure;
public static class Seeding {
    public static async Task<int> SeedWithBogusAsync(this BlogsContext db)
    {
        // https://zetcode.com/csharp/bogus/
        Randomizer.Seed = new Random(1938); // Allways the same random number

        var tagEntityFramework = new Tag("TagEF", "Entity Framework");
        var tagDotNet = new Tag("TagNet", ".NET");
        var tagDotNetMaui = new Tag("TagMaui", ".NET MAUI");
        var tagAspDotNet = new Tag("TagAsp", "ASP.NET");
        var tagAspDotNetCore = new Tag("TagAspC", "ASP.NET Core");
        var tagDotNetCore = new Tag("TagC", ".NET Core");
        var tagHacking = new Tag("TagHx", "Hacking");
        var tagLinux = new Tag("TagLin", "Linux");
        var tagSqlite = new Tag("TagLite", "SQLite");
        var tagVisualStudio = new Tag("TagVS", "Visual Studio");
        var tagGraphQl = new Tag("TagQL", "GraphQL");
        var tagCosmosDb = new Tag("TagCos", "CosmosDB");
        var tagBlazor = new Tag("TagBl", "Blazor");

        await db.Tags.AddRangeAsync(tagEntityFramework, tagDotNet, tagDotNetMaui,
            tagAspDotNet, tagAspDotNetCore, tagDotNetCore, tagHacking, tagLinux,
            tagSqlite, tagVisualStudio, tagGraphQl, tagBlazor);
        await db.SaveChangesAsync();


        var authors = new Faker<Author>("de").CustomInstantiator(f =>
        {
            return new Author(f.Name.FullName())
            {

                Contact = new Address(
                    Street: f.Address.StreetName(),
                    City: f.Address.City(),
                    Postcode: f.Address.ZipCode(),
                    Country: f.Address.Country()
                )
            };
        }
        )
        .Generate(100)
        .ToList();

        db.Authors.AddRange(authors);
        await db.SaveChangesAsync();



        var tags = db.Tags.ToList();

        var blogs = new Faker<Blog>("de").CustomInstantiator(f =>
        {
            return new Blog(name: f.Commerce.ProductName());
        })
        .Generate(10)
        .ToList();

        await db.Blogs.AddRangeAsync(blogs);
        await db.SaveChangesAsync();

        var posts = new Faker<Post>("de").CustomInstantiator(f =>
        {
            var p = new Post(
                title: f.Lorem.Word(),
                content: f.Lorem.Paragraph(),
                publishedOn: f.Date.Recent(100)
                )
            {
                Author = null,
            };
            // Postgres error
            // https://stackoverflow.com/questions/69961449/net6-and-datetime-problem-cannot-write-datetime-with-kind-utc-to-postgresql-ty/70142836#70142836
            p.PublishedOn = DateTime.SpecifyKind(p.PublishedOn, DateTimeKind.Utc);
            // Tags
            int r = f.Random.Int(1, 5);
            var l = new List<Tag>(tags);
            for (int i = 0; i < r; i++)
            {
                var t = f.PickRandom<Tag>(l);
                p.Tags.Add(t);
                l.Remove(t);
            }

            p.Blog = f.PickRandom<Blog>(blogs);
            p.Author = f.PickRandom<Author>(authors);
            return p;
        })
        .Generate(50)
            .ToList();

        await db.Posts.AddRangeAsync(posts);
        await db.SaveChangesAsync();

        return 0;


    }

    public static async Task<int> SeedAsync(this BlogsContext db)
    {
        var tagEntityFramework = new Tag("TagEF", "Entity Framework");
        var tagDotNet = new Tag("TagNet", ".NET");
        var tagDotNetMaui = new Tag("TagMaui", ".NET MAUI");
        var tagAspDotNet = new Tag("TagAsp", "ASP.NET");
        var tagAspDotNetCore = new Tag("TagAspC", "ASP.NET Core");
        var tagDotNetCore = new Tag("TagC", ".NET Core");
        var tagHacking = new Tag("TagHx", "Hacking");
        var tagLinux = new Tag("TagLin", "Linux");
        var tagSqlite = new Tag("TagLite", "SQLite");
        var tagVisualStudio = new Tag("TagVS", "Visual Studio");
        var tagGraphQl = new Tag("TagQL", "GraphQL");
        var tagCosmosDb = new Tag("TagCos", "CosmosDB");
        var tagBlazor = new Tag("TagBl", "Blazor");

        var maddy = new Author("Maddy Montaquila")
        {
            Contact = new("1 Main St", "Camberwick Green", "CW1 5ZH", "UK")
        };
        var jeremy = new Author("Jeremy Likness")
        {
            Contact = new("2 Main St", "Chigley", "CW1 5ZH", "UK")
        };
        var dan = new Author("Daniel Roth")
        {
            Contact = new("3 Main St", "Camberwick Green", "CW1 5ZH", "UK")
        };
        var arthur = new Author("Arthur Vickers")
        {
            Contact = new("15a Main St", "Chigley", "CW1 5ZH", "UK")
        };
        var brice = new Author("Brice Lambson")
        {
            Contact = new("4 Main St", "Chigley", "CW1 5ZH", "UK")
        };

        var blogs = new List<Blog>
        {
            new(".NET Blog")
            {
                Posts =
                {
                    new Post(
                        "Productivity comes to .NET MAUI in Visual Studio 2022",
                        "Visual Studio 2022 17.3 is now available and...",
                        DateTime.SpecifyKind(new DateTime(2022, 8, 9), DateTimeKind.Utc)  )
                        { Tags = { tagDotNetMaui, tagDotNet }, Author = maddy},
                    new Post(
                        "Announcing .NET 7 Preview 7", ".NET 7 Preview 7 is now available with improvements to System.LINQ, Unix...",
                        DateTime.SpecifyKind(new DateTime(2022, 8, 9), DateTimeKind.Utc)) { Tags = { tagDotNet }, Author = jeremy},
                    new Post(
                        "ASP.NET Core updates in .NET 7 Preview 7", ".NET 7 Preview 7 is now available! Check out what's new in...",
                        DateTime.SpecifyKind(new DateTime(2022, 8, 9), DateTimeKind.Utc))
                    {
                        Tags = { tagDotNet, tagAspDotNet, tagAspDotNetCore }, Author = dan,
                    },
                    new FeaturedPost(
                        "Announcing Entity Framework 7 Preview 7: Interceptors!",
                        "Announcing EF7 Preview 7 with new and improved interceptors, and...",
                        new DateTime(2022, 8, 9),
                        "Loads of runnable code!")
                    {
                        Tags = { tagEntityFramework, tagDotNet, tagDotNetCore }, Author = arthur,
                    }
                },
            },
            new("1unicorn2")
            {
                Posts =
                {
                    new Post(
                        "Hacking my Sixth Form College network in 1991",
                        "Back in 1991 I was a student at Franklin Sixth Form College...",
                        new DateTime(2020, 4, 10)) { Tags = { tagHacking }, Author = arthur, },
                    new FeaturedPost(
                        "All your versions are belong to us",
                        "Totally made up conversations about choosing Entity Framework version numbers...",
                        new DateTime(2020, 3, 26),
                        "Way funny!") { Tags = { tagEntityFramework }, Author = arthur,  },
                    new Post(
                        "Moving to Linux", "A few weeks ago, I decided to move from Windows to Linux as...",
                        new DateTime(2020, 3, 7)) { Tags = { tagLinux }, Author = arthur, },
                    new Post(
                        "Welcome to One Unicorn 2.0!", "I created my first blog back in 2011..",
                        new DateTime(2020, 2, 29)) { Tags = { tagEntityFramework }, Author = arthur, }
                }
            },
            new("Brice's Blog")
            {
                Posts =
                {
                    new FeaturedPost(
                        "SQLite in Visual Studio 2022", "A couple of years ago, I was thinking of ways...",
                        new DateTime(2022, 7, 26), "Love for VS!")
                    {
                        Tags = { tagSqlite, tagVisualStudio }, Author = brice,
                    },
                    new Post(
                        "On .NET - Entity Framework Migrations Explained",
                        "This week, @JamesMontemagno invited me onto the On .NET show...",
                        new DateTime(2022, 5, 4))
                    {
                        Tags = { tagEntityFramework, tagDotNet }, Author = brice,
                    },
                    new Post(
                        "Dear DBA: A silly idea", "We have fun on the Entity Framework team...",
                        new DateTime(2022, 3, 31)) { Tags = { tagEntityFramework }, Author = brice,  },
                    new Post(
                        "Microsoft.Data.Sqlite 6", "It’s that time of year again. Microsoft.Data.Sqlite version...",
                        new DateTime(2021, 11, 8)) { Tags = { tagSqlite, tagDotNet }, Author = brice,  }
                }
            },
            new("Developer for Life")
            {
                Posts =
                {
                    new Post(
                        "GraphQL for .NET Developers", "A comprehensive overview of GraphQL as...",
                        new DateTime(2021, 7, 1))
                    {
                        Tags = { tagDotNet, tagGraphQl, tagAspDotNetCore }, Author = jeremy,
                    },
                    new FeaturedPost(
                        "Azure Cosmos DB With EF Core on Blazor Server",
                        "Learn how to build Azure Cosmos DB apps using Entity Framework Core...",
                        new DateTime(2021, 5, 16),
                        "Blazor FTW!")
                    {
                        Tags =
                        {
                            tagDotNet,
                            tagEntityFramework,
                            tagAspDotNetCore,
                            tagCosmosDb,
                            tagBlazor
                        },
                        Author = jeremy,
                    },
                    new Post(
                        "Multi-tenancy with EF Core in Blazor Server Apps",
                        "Learn several ways to implement multi-tenant databases in Blazor Server apps...",
                        new DateTime(2021, 4, 29))
                    {
                        Tags = { tagDotNet, tagEntityFramework, tagAspDotNetCore, tagBlazor },
                        Author = jeremy,
                    },
                    new Post(
                        "An Easier Blazor Debounce", "Where I propose a simple method to debounce input without...",
                        new DateTime(2021, 4, 12))
                    {
                        Tags = { tagDotNet, tagAspDotNetCore, tagBlazor }, Author = jeremy,
                    }
                }
            }
        };

        await db.AddRangeAsync(blogs);
        return await db.SaveChangesAsync();
    }

    public static int Seed(this BlogsContext db)
    {
        var tagEntityFramework = new Tag("TagEF", "Entity Framework");
        var tagDotNet = new Tag("TagNet", ".NET");
        var tagDotNetMaui = new Tag("TagMaui", ".NET MAUI");
        var tagAspDotNet = new Tag("TagAsp", "ASP.NET");
        var tagAspDotNetCore = new Tag("TagAspC", "ASP.NET Core");
        var tagDotNetCore = new Tag("TagC", ".NET Core");
        var tagHacking = new Tag("TagHx", "Hacking");
        var tagLinux = new Tag("TagLin", "Linux");
        var tagSqlite = new Tag("TagLite", "SQLite");
        var tagVisualStudio = new Tag("TagVS", "Visual Studio");
        var tagGraphQl = new Tag("TagQL", "GraphQL");
        var tagCosmosDb = new Tag("TagCos", "CosmosDB");
        var tagBlazor = new Tag("TagBl", "Blazor");

        var maddy = new Author("Maddy Montaquila")
        {
            Contact = new("1 Main St", "Camberwick Green", "CW1 5ZH", "UK")
        };
        var jeremy = new Author("Jeremy Likness")
        {
            Contact = new("2 Main St", "Chigley", "CW1 5ZH", "UK")
        };
        var dan = new Author("Daniel Roth")
        {
            Contact = new("3 Main St", "Camberwick Green", "CW1 5ZH", "UK")
        };
        var arthur = new Author("Arthur Vickers")
        {
            Contact = new("15a Main St", "Chigley", "CW1 5ZH", "UK")
        };
        var brice = new Author("Brice Lambson")
        {
            Contact = new("4 Main St", "Chigley", "CW1 5ZH", "UK")
        };

        var blogs = new List<Blog>
        {
            new(".NET Blog")
            {
                Posts =
                {
                    new Post(
                        "Productivity comes to .NET MAUI in Visual Studio 2022",
                        "Visual Studio 2022 17.3 is now available and...",
                        DateTime.SpecifyKind(new DateTime(2022, 8, 9), DateTimeKind.Utc)  )
                        { Tags = { tagDotNetMaui, tagDotNet }, Author = maddy},
                    new Post(
                        "Announcing .NET 7 Preview 7", ".NET 7 Preview 7 is now available with improvements to System.LINQ, Unix...",
                        DateTime.SpecifyKind(new DateTime(2022, 8, 9), DateTimeKind.Utc)) { Tags = { tagDotNet }, Author = jeremy},
                    new Post(
                        "ASP.NET Core updates in .NET 7 Preview 7", ".NET 7 Preview 7 is now available! Check out what's new in...",
                        DateTime.SpecifyKind(new DateTime(2022, 8, 9), DateTimeKind.Utc))
                    {
                        Tags = { tagDotNet, tagAspDotNet, tagAspDotNetCore }, Author = dan,
                    },
                    new FeaturedPost(
                        "Announcing Entity Framework 7 Preview 7: Interceptors!",
                        "Announcing EF7 Preview 7 with new and improved interceptors, and...",
                        new DateTime(2022, 8, 9),
                        "Loads of runnable code!")
                    {
                        Tags = { tagEntityFramework, tagDotNet, tagDotNetCore }, Author = arthur,
                    }
                },
            },
            new("1unicorn2")
            {
                Posts =
                {
                    new Post(
                        "Hacking my Sixth Form College network in 1991",
                        "Back in 1991 I was a student at Franklin Sixth Form College...",
                        new DateTime(2020, 4, 10)) { Tags = { tagHacking }, Author = arthur, },
                    new FeaturedPost(
                        "All your versions are belong to us",
                        "Totally made up conversations about choosing Entity Framework version numbers...",
                        new DateTime(2020, 3, 26),
                        "Way funny!") { Tags = { tagEntityFramework }, Author = arthur,  },
                    new Post(
                        "Moving to Linux", "A few weeks ago, I decided to move from Windows to Linux as...",
                        new DateTime(2020, 3, 7)) { Tags = { tagLinux }, Author = arthur, },
                    new Post(
                        "Welcome to One Unicorn 2.0!", "I created my first blog back in 2011..",
                        new DateTime(2020, 2, 29)) { Tags = { tagEntityFramework }, Author = arthur, }
                }
            },
            new("Brice's Blog")
            {
                Posts =
                {
                    new FeaturedPost(
                        "SQLite in Visual Studio 2022", "A couple of years ago, I was thinking of ways...",
                        new DateTime(2022, 7, 26), "Love for VS!")
                    {
                        Tags = { tagSqlite, tagVisualStudio }, Author = brice,
                    },
                    new Post(
                        "On .NET - Entity Framework Migrations Explained",
                        "This week, @JamesMontemagno invited me onto the On .NET show...",
                        new DateTime(2022, 5, 4))
                    {
                        Tags = { tagEntityFramework, tagDotNet }, Author = brice,
                    },
                    new Post(
                        "Dear DBA: A silly idea", "We have fun on the Entity Framework team...",
                        new DateTime(2022, 3, 31)) { Tags = { tagEntityFramework }, Author = brice,  },
                    new Post(
                        "Microsoft.Data.Sqlite 6", "It’s that time of year again. Microsoft.Data.Sqlite version...",
                        new DateTime(2021, 11, 8)) { Tags = { tagSqlite, tagDotNet }, Author = brice,  }
                }
            },
            new("Developer for Life")
            {
                Posts =
                {
                    new Post(
                        "GraphQL for .NET Developers", "A comprehensive overview of GraphQL as...",
                        new DateTime(2021, 7, 1))
                    {
                        Tags = { tagDotNet, tagGraphQl, tagAspDotNetCore }, Author = jeremy,
                    },
                    new FeaturedPost(
                        "Azure Cosmos DB With EF Core on Blazor Server",
                        "Learn how to build Azure Cosmos DB apps using Entity Framework Core...",
                        new DateTime(2021, 5, 16),
                        "Blazor FTW!")
                    {
                        Tags =
                        {
                            tagDotNet,
                            tagEntityFramework,
                            tagAspDotNetCore,
                            tagCosmosDb,
                            tagBlazor
                        },
                        Author = jeremy,
                    },
                    new Post(
                        "Multi-tenancy with EF Core in Blazor Server Apps",
                        "Learn several ways to implement multi-tenant databases in Blazor Server apps...",
                        new DateTime(2021, 4, 29))
                    {
                        Tags = { tagDotNet, tagEntityFramework, tagAspDotNetCore, tagBlazor },
                        Author = jeremy,
                    },
                    new Post(
                        "An Easier Blazor Debounce", "Where I propose a simple method to debounce input without...",
                        new DateTime(2021, 4, 12))
                    {
                        Tags = { tagDotNet, tagAspDotNetCore, tagBlazor }, Author = jeremy,
                    }
                }
            }
        };

        db.AddRange(blogs);
        return db.SaveChanges();
    }
}
