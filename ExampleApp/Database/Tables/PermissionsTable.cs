using ExampleApp.Database.Models;
using ShimmyMySherbet.MySQL.EF.Core;

namespace ExampleApp.Database.Tables
{
    public class PermissionsTable : DatabaseTable<UserPermissions>
    {
        public PermissionsTable(string tableName) : base(tableName)
        {
        }
    }
}