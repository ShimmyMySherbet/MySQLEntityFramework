using ShimmyMySherbet.MySQL.EF.Core;
using ShimmyMySherbet.MySQL.EF.Models;
using ShimmyMySherbet.MySQL.EF.Models.Interfaces;

namespace ExampleApp
{
    public class Database : DatabaseClient
    {
        public DatabaseTable<UserAccount> Accounts { get; } = new DatabaseTable<UserAccount>("users");
        public PostsDatabaseTable Posts { get; } = new PostsDatabaseTable("Posts");

        public Database(DatabaseSettings settings) : base(settings)
        {
        }

        public Database(IConnectionProvider provider) : base(provider)
        {
        }
    }
}