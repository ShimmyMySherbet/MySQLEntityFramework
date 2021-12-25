using System;
using System.Linq;
using System.Threading.Tasks;
using ExampleApp.Database;
using ExampleApp.Database.Models;
using ShimmyMySherbet.MySQL.EF.Models;
using ShimmyMySherbet.MySQL.EF.Models.ConnectionProviders;

namespace ExampleApp
{
    public class Program
    {
        public static DatabaseManager Database;

        private static void Main(string[] args)
        {
            // Can be used in serialized configs, e.g., rocket plugin config
            var settings = new DatabaseSettings("127.0.0.1", "TestDatabase", "kn8hSzrg2OVhTWHN", "test");
            // Different providers change how connections are managed.
            var provider = new SingleConnectionProvider(settings); // singleton provider
            // Database manager class that contains tables
            Database = new DatabaseManager(provider);

            Console.WriteLine($"Connected: {Database.Connect(out string fail)}"); // try to connect to database
            Console.WriteLine($"Error Message: {fail}"); // print user friendly error if failed
            Database.CheckSchema(); // Check Schema (ensure tables exist, if not, create them)
            Database.AutoUpdateInstanceKey = true; // Auto update class auto increment primary key on insert
        }

        private static async Task<ulong> CreateUser(string username, string email)
        {
            var newUser = new UserAccount()
            {
                EmailAddress = email,
                Username = username
            };
            await Database.Users.InsertAsync(newUser);
            // return user ID as allocated by the database
            return newUser.ID;
        }

        private static async Task GrantPermission(ulong userID, string permission, ulong granter)
        {
            // gets user perms from table, or creates new one if user does not have any perms
            var perms = Database.Permissions.QuerySingle("SELECT * FROM @TABLE WHERE UserID=@0;", userID)
                ?? new UserPermissions() { UserID = userID };

            if (!perms.Permissions.Any(x => x.PermissionID == permission))
            {
                perms.Permissions.Add(new Permission() { PermissionID = permission, GrantedBy = granter });

                await Database.Permissions.InsertUpdateAsync(perms); // Update existing or Insert new row
            }
        }
    }
}