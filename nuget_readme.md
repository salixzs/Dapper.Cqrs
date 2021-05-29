# Dapper.Cqrs

Libraries (NuGet packages) to help employ [CQRS](https://martinfowler.com/bliki/CQRS.html) (Command Query Responsibility Segregation) pattern to database access in various .Net projects. Approach unifies database data retrieval under CQRS handler, which is the only dependency for business logic (getting rid of multiple repository injections).

Package uses [Dapper](https://stackexchange.github.io/Dapper/) - a simple object mapper for .Net built by StackOverflow developers and is one of the most performing ORMs in .Net landscape (if not fastest).

Solution is separating concerns into Abstractions (no dependencies), Database-specific and Testing helpers to be used in more architecturally utilizable way with [practical developer usage ease](https://github.com/salixzs/Dapper.Cqrs/wiki/Productivity) and ability to [validate "magic string queries"](https://github.com/salixzs/Dapper.Cqrs/wiki/QueryTesting) against database to avoid runtime fails due to database structural changes not followed in code.

[Repo](https://github.com/salixzs/Dapper.Cqrs) containing actual usage [sample project](https://github.com/salixzs/Dapper.Cqrs/wiki/AspNet5ApiSample) and Visual Studio [item templates](https://github.com/salixzs/Dapper.Cqrs/wiki/Productivity#provided-templates) for most of development needs.

> Detailed documentation is in [WIKI](https://github.com/salixzs/Dapper.Cqrs/wiki).

# Usage

When packages are added and set-up (see "Installation" section below) - as application developer you need to do two things (besides writing tests).

* Create `IQuery` (Data retrieval) or `ICommand` (Data modification) implementation classes based on provided base classes.
* Inject `ICommandQueryContext` into your business logic class and use it to execute `IQuery` and `ICommand` classes against database engine. This is the only dependency required to work with database (Yeah, no more numerous repositories to depend upon!)

## IQuery & MsSqlQuery*Base
Required to be able to read data from database. Create new class and implement its interface via provided base classes.
Case class `MsSqlQueryMultipleBase<T>` is class, implementing most of `IQuery` interface demands to retrieve multiple records (`IEnumerable<T>`) from database.
Base class `MsSqlQuerySingleBase<T>` does the same, but to retrieve only one record mapped to database poco object `T`.

## ICommand
Required to be able to modify data in database. Create new class and implement its interface via provided base classes.

## Execution
Inject `ICommandQueryContext` into your business logic classes, which require to work with data in database.
```csharp
public class SampleLogic : ISampleLogic
{
    private readonly ICommandQueryContext _db;
    
    // Constructor - yes, inject just this one (no more numerous repositories!)
    public SampleLogic(ICommandQueryContext db) => _db = db;
}
```

Then in this class methods you can use this injected object and use its methods with prepared `IQuery` and `ICommand` classes to do database calls.

```csharp
// Reading data
public async Task<IEnumerable<SampleData>> GetAll(int refId) => 
    await _db.QueryAsync(new SampleQuery(refId));

// Creating (saving) data
public async Task<int> Create(SampleData dataObject) => 
    await _db.ExecuteAsync(new SampleCreateCommand(dataObject));
```

## Testability
As package uses interfaces for everything - it is easy to be mocked for isolation in pure unit-testing routines.

`Testing` package includes prepared base classes and helpers to automatically find all Queries and Commands in your project and validate SQL statements in those against database for syntax validity. There are helpers for other testing needs, like compare DTO with database object and write/read tests.

## Registration
Register package components with your dependency injection container, like here for MS.Extensions.DI
```csharp
services.AddScoped<IMsSqlContext, DatabaseContext>(svc =>
    new DatabaseContext(
        connectionString,
        svc.GetService<ILogger<DatabaseContext>>()));
services.AddScoped<IDatabaseSession, SqlDatabaseSession>();
services.AddScoped<ICommandQueryContext, CommandQueryContext>();
```

### Like what I created?
<a href="https://www.buymeacoffee.com/salixzs" target="_blank"><img src="https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png" alt="Buy Me A Coffee" style="height: 32px !important;width: 146px !important;box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;-webkit-box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;" ></a>