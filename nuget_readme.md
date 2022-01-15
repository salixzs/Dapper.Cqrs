# Dapper.Cqrs

A set of NuGet packages provides ability to use [CQRS (Command Query Responsibility Segregation)](https://martinfowler.com/bliki/CQRS.html) pattern for database data reading (Queries) or its modification (Commands). It includes ambient connection and transaction handling, so after *easy* [set-up](https://github.com/salixzs/Dapper.Cqrs/wiki/Setup), developers are only required to write [Query](https://github.com/salixzs/Dapper.Cqrs/wiki/IQuery) and [Command](https://github.com/salixzs/Dapper.Cqrs/wiki/ICommand) classes which usually are database SQL statements, wrapped in separate classes, with a little bit of parameter handling (business as usual).

Solution is using [Dapper Micro-ORM](https://dapperlib.github.io/Dapper/), a simple object mapper for .Net built by StackOverflow developers and is one of the most performing ORMs in .Net landscape (if not fastest).

Packages are built with [practical developer usage ease](https://github.com/salixzs/Dapper.Cqrs/wiki/Productivity) in mind and ability to [validate "magic string queries"](https://github.com/salixzs/Dapper.Cqrs/wiki/QueryTesting) against database to avoid runtime fails due to database structural changes becoming inconsistent with code as well as other helpers to ease writing tests for logic that uses these packages.

Simplistic short example on how usage might look in your code:
```csharp
public class MyBusinessLogic : IMyBusinessLogic
{
    // field, assigned in constructor, which provides ability 
    // to execute Queries and Commands onto database
    private readonly ICommandQueryContext _db;
    
    // Class constructor - inject DB access context into field above
    public MyBusinessLogic(ICommandQueryContext db) => _db = db;
    
    // Business logic method
    public async Task<IEnumerable<MyObject>> BusinessOperation(int myObjectId)
    {
        // COMMAND: Executing some INSERT into Audit table implemented in ICommand class
        await _db.ExecuteAsync(new AuditRecordCommand(auditData));
        
        // QUERY: Get the object to return implemented in IQuery class
        return await _db.QueryAsync(new GetMyObjectQuery(myObjectId));
    }
}

// ICommand class used in Logic class above to insert Audit record
public sealed class AuditRecordCommand : MsSqlCommandBase
{
    private readonly AuditRecord _dbObject;
    public AuditRecordCommand(AuditRecord dbObject) => _dbObject = dbObject;
    public override object Parameters => _dbObject;
    public override string SqlStatement => @"INSERT INTO Audit (Field1, Field2...) VALUES (@Field1, @Field2)";
}

// IQuery class used to retrieve record in logic class above
public sealed class GetMyObjectQuery : MsSqlQuerySingleBase<MyObject>
{
    public override string SqlStatement => "SELECT * FROM MyObject";
}
```

When first command (AuditRecordInsert) is executed, dbContext object creates and opens DB connection and creates a transaction on it, then executes the INSERT command with passed object as parameter for it. After - query object (GetMyObjectQuery) is reusing the same connection and transaction to get the data and return it to caller. Upon operation end and dbContext dispose - transaction is committed and connection closed. In case there are exceptions in business logic (including database operations) - transaction is rollback for entire operation (unit-of-work).

This approach provides a big flexibility of using database data in any of your business logic without relaying onto numerous repositories, as well as by using native database language for optimized data modification and retrieval, giving way bigger performance benefits in compare to magically generated SQL statements by OOP language (like in Entity Framework, Linq-to-SQL).

Code in packages are covered by unit-tests and assembly-tests to levels more than 90% to ensure they are of the best quality possible and to work as expected.

Approach is succesfully used in huge project (>3M code lines) and database of 200 tables with ~10TB of data.

## Package suite

Approach is separated into three assemblies (packages):

### [Abstractions](https://www.nuget.org/packages/Salix.Dapper.Cqrs.Abstractions/)
Provide common interfaces and base functionalities suitable for all database engines. This package does not demand specific database client as dependency. 
It contains `IQuery` and `ICommand` interfaces as well as internal interfaces and implementation to employ Unit-of-Work database connection and transaction handling.

### [Database-specific implementation(s)](https://www.nuget.org/packages/Salix.Dapper.Cqrs.MsSql/) - *now only MsSQL*
Classes and functionalities to work with database engine, handling its specific connection by opening, closing and transaction handling transparently for application which is using it.

### [Helpers to write tests against database](https://www.nuget.org/packages/Salix.Dapper.Cqrs.MsSql.Testing.XUnit/) - XUnit/MsSql
Classes and functionalities to be able to write integration/assembly tests, using real database engine, which eases checking SQL statements in Queries and Commands, Compares C# POCO data objects with database structure and saving/reading test data.

# Is it better than Entity Framework?

As with everything - yes and no. Depending on... (your lifestyle and preferences here)

Using Dapper and similar in projects are usually tied to need for solid database data retrieval performance or getting data from very complex queries. Latest EF Core 6 introducing quite a performance boost, so it might not be the case lately, however... 

When using Entity Framework you are rarely concerned about SQL syntax and what magic is hidden behind your LINQ statements. It works perfectly for simple solutions and probably would be recommended approach for projects/people who do not want to deal with database query language (effectively making data storage mechanism a second class citizen to application).

But as soon as database becomes more complex and required data retrieval involves many objects - not following generated SQL behind LINQ can lead to considerable performance problems (N+1, Cartesian product). Then different non-standard side approaches are implemented (Stored Procedures, Views, Custom solutions - ADO.NET or using some MicroORM as fallback for such situations).

So using Dapper will make you write SQL statements and do more boilerplate coding, but it increases control over how and when data is read and write to database and bringing data storage as a first class citizen in your application.

This project just wraps Dapper into CQRS approach and allows to get all project SQL statements in "magic strings" easily located by C# code (e.g. for automated validation).
