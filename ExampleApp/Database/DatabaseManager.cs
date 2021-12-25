using ExampleApp.Database.Tables;
using ShimmyMySherbet.MySQL.EF.Core;
using ShimmyMySherbet.MySQL.EF.Models.Interfaces;

namespace ExampleApp.Database
{
    public class DatabaseManager : DatabaseClient
    {
        public UsersTable Users { get; } = new UsersTable("Test_Users");

        public PermissionsTable Permissions { get; } = new PermissionsTable("Test_Perms");

        public UserCommentsTable Comments { get; } = new UserCommentsTable("Test_Comments");

        public DatabaseManager(IConnectionProvider connectionProvider) : base(connectionProvider)
        {
        }
    }
}