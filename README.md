# Dapper.Cqrs

<img align="left" src="DapperCQRS.png">

Libraries (NuGet packages) to help employ [CQRS](https://martinfowler.com/bliki/CQRS.html) (Command Query Responsibility Segregation) pattern to database access in various .Net projects. Approach unifies database data retrieval under CQRS handler, which is the only dependency for business logic (getting rid of multiple repository injections).

Package uses [Dapper](https://stackexchange.github.io/Dapper/) - a simple object mapper for .Net built by StackOverflow developers and is one of the most performing ORMs in .Net landscape (if not fastest).

Project was influenced by another repository - [UPNXT-Dapper-cqrs](https://github.com/upnxt/upnxt-dapper-cqrs). Solution is separating concerns into Abstractions (no dependencies), Database-specific and Testing helpers to be used in more architecturally utilizable way with [practical developer usage ease](https://github.com/salixzs/Dapper.Cqrs/wiki/Productivity) and ability to [validate "magic string queries"](https://github.com/salixzs/Dapper.Cqrs/wiki/QueryTesting) against database to avoid runtime fails due to database structural changes not followed in code.

Repo containing actual usage [sample project](https://github.com/salixzs/Dapper.Cqrs/wiki/AspNet5ApiSample) and Visual Studio [item templates](https://github.com/salixzs/Dapper.Cqrs/wiki/Productivity#provided-templates) for most of development needs.

Packages are targeting .Net Standard 2.0

| Package | NuGet | Build |  Code Coverage | Downloads |
| ------- | ----- | ----- | -------------- | --------- |
| Dapper.CQRS.Abstractions | [![Nuget](https://img.shields.io/nuget/v/Salix.Dapper.Cqrs.Abstractions?logo=nuget&logoColor=lightblue&style=plastic)](https://www.nuget.org/packages/Salix.Dapper.Cqrs.Abstractions) | ![Azure DevOps Build ](https://img.shields.io/azure-devops/build/SmartDance/45bc2cc0-bce1-4cac-a1d8-ba3f243940a6/4?style=plastic&logo=azuredevops&logoColor=blue) | [![Azure DevOps coverage](https://img.shields.io/azure-devops/coverage/SmartDance/GitHubProjects/4?color=green&logo=azuredevops&logoColor=blue&style=plastic)](https://dev.azure.com/SmartDance/GitHubProjects/_build?definitionId=4) | ![Nuget](https://img.shields.io/nuget/dt/Salix.Dapper.Cqrs.Abstractions?logo=nuget&logoColor=lightblue&style=plastic) |
| Dapper.CQRS.MsSql | [![Nuget](https://img.shields.io/nuget/v/Salix.Dapper.Cqrs.MsSql?logo=nuget&logoColor=lightblue&style=plastic)](https://www.nuget.org/packages/Salix.Dapper.Cqrs.MsSql) | ![Azure DevOps Build ](https://img.shields.io/azure-devops/build/SmartDance/45bc2cc0-bce1-4cac-a1d8-ba3f243940a6/5?style=plastic&logo=azuredevops&logoColor=blue) | [![Azure DevOps coverage](https://img.shields.io/azure-devops/coverage/SmartDance/GitHubProjects/5?color=green&logo=azuredevops&logoColor=blue&style=plastic)](https://dev.azure.com/SmartDance/GitHubProjects/_build?definitionId=5) | ![Nuget](https://img.shields.io/nuget/dt/Salix.Dapper.Cqrs.MsSql?logo=nuget&logoColor=lightblue&style=plastic) |
| Dapper.CQRS.Testing.MsSql.xUnit | [![Nuget](https://img.shields.io/nuget/v/Salix.Dapper.Cqrs.MsSql.Testing.XUnit?logo=nuget&logoColor=lightblue&style=plastic)](https://www.nuget.org/packages/Salix.Dapper.Cqrs.MsSql.Testing.XUnit) | ![Azure DevOps Build ](https://img.shields.io/azure-devops/build/SmartDance/45bc2cc0-bce1-4cac-a1d8-ba3f243940a6/6?style=plastic&logo=azuredevops&logoColor=blue) | N/A | ![Nuget](https://img.shields.io/nuget/dt/Salix.Dapper.Cqrs.MsSql.Testing.XUnit?logo=nuget&logoColor=lightblue&style=plastic) |

> Detailed documentation is in [WIKI](https://github.com/salixzs/Dapper.Cqrs/wiki).

# Usage

When packages are added and set-up (see "Installation" section below) - as application developer you need to do two things (besides writing tests).

* Create `IQuery` (Data retrieval) or `ICommand` (Data modification) implementation classes based on provided base classes.
* Inject `ICommandQueryContext` into your business logic class and use it to execute `IQuery` and `ICommand` classes against database engine. This is the only dependency required to work with database (Yeah, no more numerous repositories to depend upon!)

## IQuery
Required to be able to read data from database. Create new class and implement its interface (used provided base class to partially do that):
```csharp
public sealed class SampleQuery : MsSqlQueryMultipleBase<SampleData>
{
    // hold parameters for query passed into class
    private readonly int _id;
    
    // Constructor
    public SampleQuery(int id) => _id = id;
    
    // Prepare anonymous object for Dapper to pass parameters for SQL statment
    public override object Parameters => new { RefId = _id };
    
    // Here is actual SQL statement for data query
    public override string SqlStatement => "SELECT Data FROM Table WHERE FkId = @RefId";
}
```
Here base class `MsSqlQueryMultipleBase<T>` is class, implementing most of `IQuery` interface demands to retrieve multiple records (`IEnumerable<T>`) from database.
Base class `MsSqlQuerySingleBase<T>` does the same, but to retrieve only one record mapped to database poco object `T`.

## ICommand
Similar to Query above, but dedicated to data modification statements.
```csharp
public sealed class SampleCreateCommand : MsSqlCommandBase<int>
{
    // Holds passed in object to be created as record in DB.
    private readonly SampleData _sampleObject;
    
    // Constructor, getting object to be saved
    public SampleCreateCommand(SampleData dbObject) =>
        _sampleObject = dbObject ?? throw new ArgumentNullException(nameof(dbObject), "No data passed");
    
    // Actual SQL statement to be executed.
    // Here appended statement to get last autoincrement value from DB == inserted record ID.
    public override string SqlStatement => @"
        INSERT INTO SampleTable (
            Name
        ) VALUES (
            @Name
        );SELECT CAST(SCOPE_IDENTITY() as int)";

    // Prepare object for Dapper to pass as parameters for SQL statement
    public override object Parameters => _sampleObject;
}
```

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

See Sample project testing project for reference to such tests.

# Installation
For .Net projects which needs access to database, reference database engine specific package.
```text
PM> Salix.Dapper.Cqrs.MsSql
```
If you put query and command classes in their own projects, you need to reference just abstractions package
```text
PM> Salix.Dapper.Cqrs.Abstractions
```
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
Components should be registered with scope, equal to one business operation (Unit of Work). As for ASP.NET Web applications/apis this is per-request scope - Scoped.

> Read more detailed documentation in [WIKI](https://github.com/salixzs/Dapper.Cqrs/wiki).


### Like what I created?
<a href="https://www.buymeacoffee.com/salixzs" target="_blank"><img src="https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png" alt="Buy Me A Coffee" style="height: 32px !important;width: 146px !important;box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;-webkit-box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;" ></a>