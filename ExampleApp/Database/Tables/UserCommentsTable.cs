using ExampleApp.Database.Models;
using ShimmyMySherbet.MySQL.EF.Core;

namespace ExampleApp.Database.Tables
{
    public class UserCommentsTable : DatabaseTable<UserComment>
    {
        public UserCommentsTable(string tableName) : base(tableName)
        {
        }
    }
}