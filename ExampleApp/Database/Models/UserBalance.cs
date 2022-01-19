using ShimmyMySherbet.MySQL.EF.Models;

namespace ExampleApp.Database.Models
{
    public class UserBalance
    {
        [SQLPrimaryKey]
        public ulong UserID { get; set; }

        public double Balance { get; set; }
    }
}