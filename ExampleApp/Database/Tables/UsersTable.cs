using ExampleApp.Database.Models;
using ShimmyMySherbet.MySQL.EF.Core;

namespace ExampleApp.Database.Tables
{
    public class UsersTable : DatabaseTable<UserAccount>
    {
        public UsersTable(string tableName) : base(tableName)
        {
        }
    }
}