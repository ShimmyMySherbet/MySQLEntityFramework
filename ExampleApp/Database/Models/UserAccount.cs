using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShimmyMySherbet.MySQL.EF;
using ShimmyMySherbet.MySQL.EF.Models;
using ShimmyMySherbet.MySQL.EF.Models.TypeModel;
namespace ExampleApp.Database.Models
{
    public class UserAccount
    {
        [SQLPrimaryKey, SQLAutoIncrement]
        public ulong ID { get; set; }

        [SQLUnique, SQLIndex, SQLVarChar(28)]
        public string Username { get; set; }

        public string EmailAddress { get; set; }

        public DateTime LastLogin { get; set; } = DateTime.Now;

        [SQLOmitUpdate] // Do not update this value, only provide on insert
        public DateTime AccountCreated { get; set; } = DateTime.Now;
    }
}
