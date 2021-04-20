# Dapper.Cqrs
> **(Still under construction! NuGet Packages not yet published!)**

Libraries (NuGet packages) to help employ [CQRS](https://martinfowler.com/bliki/CQRS.html) (Command Query Responsibility Segregation) pattern to database access in various .Net projects.

Package uses [Dapper](https://stackexchange.github.io/Dapper/) - a simple object mapper for .Net built by StackOverflow developers and is one of the most performing ORMs in .Net landscape (if not fastest).

Project was influenced by another repository - [UPNXT-Dapper-cqrs](https://github.com/upnxt/upnxt-dapper-cqrs), but built into more utilizable way with practical developer usage ease and looking for ways to be able to validate "magic string queries" against database to avoid runtime fails due to database structural changes not followed in code.

Packages are targeting .Net Standard 2.0

Detailed documentation is in [WIKI](https://github.com/salixzs/Dapper.Cqrs/wiki).

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


# Usage

As application developer you need to do two things (besides writing tests).

* Create `IQuery` or `ICommand` implementation(s)
* Inject `ICommandQueryContext` into your class and use it to execute `IQuery` and `ICommand` classes against database engine.

## IQuery
Required to be able to read data from database. Create new class and implement its interface:
```csharp
public sealed class SampleQuery : MsSqlQueryBase, IQuery<IEnumerable<SampleData>>
{
    // hold parameters for query passed into class
    private readonly int _id;
    
    // Constructor
    public SampleQuery(int id) => _id = id;
    
    // Prepare anonymous object for Dapper to pass parameters for SQL statment
    public override object Parameters => new { RefId = _id };
    
    // Here is actual SQL statement for data query
    public override string SqlStatement => "SELECT Data FROM Table WHERE FkId = @RefId";
    
    // This is to specify internal method to use for query (One or many records?)
    public async Task<IEnumerable<SampleData>> ExecuteAsync(IDatabaseSession session)
            => await session.QueryAsync<SampleData>(this.SqlStatement, this.Parameters);
}
```
Here base class `MsSqlQueryBase` is class, implementing most of `IQuery` interface demands, but with MsSQL specifics, so developers does not have to for each `IQuery` implementation.

## ICommand
Similar to Query, but dedicated to data modification statements.
```csharp
public sealed class SampleCreateCommand : ICommand<int>, ICommandValidator
{
    // Holds passed in object to be created as record in DB.
    private readonly SampleData _sampleObject;
    
    // Constructor, getting object to be saved
    public SampleCreateCommand(SampleData dbObject) =>
            _sampleObject = dbObject ?? throw new ArgumentNullException(nameof(dbObject), "No data passed");
    
    // Actual SQL statement to be executed.
    // Here appended statement to get last autoincrement value from DB == inserted record ID.
    public string SqlStatement => @"
        INSERT INTO SampleTable (
            Name
        ) VALUES (
            @Name
        );SELECT CAST(SCOPE_IDENTITY() as int)";
    
    // Letting Dapper to do its magic with mapping and execution
    public async Task<int> ExecuteAsync(IDatabaseSession session) =>
        await session.ExecuteAsync<int>(this.SqlStatement, _sampleObject);
}
```

## Execution

Inject `ICommandQueryContext` into your business logic classes, which require to work with data in database.
```csharp
public class SampleLogic : ISampleLogic
{
    private readonly ICommandQueryContext _db;
    
    // Constructor
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

### Read more detailed documentation in [WIKI](https://github.com/salixzs/Dapper.Cqrs/wiki).