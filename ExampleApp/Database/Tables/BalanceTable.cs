using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExampleApp.Database.Models;
using ShimmyMySherbet.MySQL.EF.Core;

namespace ExampleApp.Database.Tables
{
    public class BalanceTable : DatabaseTable<UserBalance>
    {
        public BalanceTable(string tableName) : base(tableName)
        {
        }
    }
}
