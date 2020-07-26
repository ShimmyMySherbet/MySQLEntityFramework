# MySQLEntityFramework
A Lightweight MySQL Entity Adapter for .NET

# Usage

Basic usage of this library is centered around MySQLEntityClient. This class povides access to most SQL functions. When working with a table, you use a class associated with it.
This class is also what you use to read/write entries from a database table.

For the following examples, this class is used:
```cs
public class UserAccount
{
    [SQLPrimaryKey, SQLAutoIncrement, SQLOmit]
    public int ID;

    [SQLUnique]
    public string Username;

    public byte[] HashData;

    [SQLIndex]
    public ulong SteamID;

    public string EmailAddress;

    public DateTime Created;
}
```
## Creating the MySQLEntityClient

The MySQLEntityClient is used for most functions. If you need pure performance (sub 0.2ms for simple queries and inserts) you should use the SQLConverter and EntityCommandBuilder classes more specific methods rather than the Entity Client's generalised methods.

This Client has two modes, Single Connection mode and Multiple Connection mode.

In Single Connection Mode, a single SQL connection is made, and maintained. All methods in the client will use this single connection, locking it while it's in use. This means, in this mode, if multiple methods are invoked accross two or more threads, the methods will block until the connection is available.
This mode is ideal for mostly single threaded screnarios that need to transfer alot of data (e.g., 50K queries in 4 sec on a single thread).

Multiple Connection Mode

In this mode, a new connection is made each time a connection is needed. This means that the client can be used accross many threads simultaneously without blocking.

This mode is ideal for database wrappers that will likely be accessed across multiple threads.

```cs
bool SingleConnectionMode = true;
MySQLEntityClient EntityClient = new MySQLEntityClient("127.0.0.1", "UserName", "SuperSecretPassword", "Database", 3306, SingleConnectionMode);

Console.WriteLine($"Connected: {EntityClient.Connected}");
```
*From here on, this MySQLEntityClient will just be referanced as EntityClient in code snippets*

## Creating a Database Table

This uses the model of the supplied class, including any SQL attributes on it's fields, to create an equivilant database table.

```cs
EntityClient.CreateTable<UserAccount>("Users");
```

## Querying

Selecting multiple entries:

```cs
List<UserAccount> Accounts = EntityClient.Query<UserAccount>("SELECT * FROM Users");
```

Selecting a single entry:

This method returns null if there are no results.

```cs
UserAccount userAccount1 = EntityClient.QuerySingle<UserAccount>("SELECT * FROM Users WHERE ID = 1");
```

Using Parameters

Most of MySQLEntityClient's methods provide an easy way to create command parameters. Parameters are a feature of MySQL.Data that allows you to securely represent a variable in a MySQLCommand.
These parameters safely format and escape the variable, to prevent SQL injection attacks and ensure proper query formatting. These should be used when working with strings or class types (e.g., DateTime).

```cs
UserAccount BobsAccount = EntityClient.QuerySingle<UserAccount>("SELECT * FROM Users WHERE Username = @0 AND EmailAddresss = @1", "Bob", "BobsMail@mail.com");
```

## Inserting

Since the ID field of UserAccount has sQLOmit, it is omitted from the insert. This means that the value will resolve to the default value. In this case, since it is also tagged as AutoIncrement when the table was created, it will resolve to the new auto-increment value.

```cs
UserAccount NewAccount = new UserAccount()
{
   Username = "NewUser",
   EmailAddress = "Email@Address.com",
   SteamID = 123456789,
   Created = DateTime.Now,
   HashData = new byte[] { 10, 21, 21 }
};
EntityClient.Insert(NewAccount, "Users");
```

## Updating

This method requires that the supplied object's class has a field tagged as SQLPrimaryKey.

```cs
BobsAccount.EmailAddress = "BobsNewEmailAddress@email.com";
EntityClient.Update(BobsAccount, "Users");
```
 
 ## Deleting
 
 This method requires that the supplied object's class has a field tagged as SQLPrimaryKey.
 
 ```cs
 EntityClient.Delete(BobsAccount, "Users");
 ```
 ## Checking for a table
 
 This method allows you to check if a table exists by it's name in the current database.
 
 ```cs
if (EntityClient.TableExists("Users"))
{
   Console.WriteLine("Table Exists.");
} else
{
   Console.WriteLine("Tabe does not exist.");
}
```

## Checking Connection Status

If ReuseSingleConnection (Single Connection Mode) is enabled, it will return the connection status of the active MySQL connection. If the client is in Multiple Connection mode, this will attempt to create a new connection, and returns if the connection was successful.

```cs
if (EntityClient.Connected)
{
    Console.WriteLine("Connected!");
} else
{
    Console.WriteLine("Connection Failed.");
}
```
## Deleting a table

This method will drop a table and all of it's contents. 

```cs
EntityClient.DeleteTable("Users");
```

# SQL Attributes

For a full list of SQL Attribues, see the wiki page https://github.com/ShimmyMySherbet/MySQLEntityFramework/wiki/SQL-Attributes


