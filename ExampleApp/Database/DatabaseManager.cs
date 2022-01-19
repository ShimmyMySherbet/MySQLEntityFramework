using ExampleApp.Database.Tables;
using ShimmyMySherbet.MySQL.EF.Core;
using ShimmyMySherbet.MySQL.EF.Models.Interfaces;

namespace ExampleApp.Database
{
    public class DatabaseManager : DatabaseClient
    {
        // The DatabaseClient will automatically find all Table fields in the class, and init them
        // This can be disabled by setting autoInit to false in DatabaseClient's constructor

        // Calling CheckSchema will call CheckSchema for all tables, automatically generating missing tables
        public UsersTable Users { get; } = new UsersTable("Test_Users");

        public PermissionsTable Permissions { get; } = new PermissionsTable("Test_Perms");

        public UserCommentsTable Comments { get; } = new UserCommentsTable("Test_Comments");

        public BalanceTable Balances { get; } = new BalanceTable("Test_Balances");

        public DatabaseManager(IConnectionProvider connectionProvider) : base(connectionProvider)
        {
        }
    }
}