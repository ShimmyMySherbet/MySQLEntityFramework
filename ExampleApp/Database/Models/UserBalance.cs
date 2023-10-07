using ShimmyMySherbet.MySQL.EF.Models;

namespace ExampleApp.Database.Models
{
    [SQLCharSet(SQLCharSet.utf8mb4)]
    public class UserBalance
    {
        [SQLPrimaryKey]
        public ulong UserID { get; set; }

        public double Balance { get; set; }
    }
}