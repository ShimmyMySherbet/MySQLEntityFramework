using ShimmyMySherbet.MySQL.EF.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Models.Interfaces
{
    public interface IDatabaseTableInitializer
    {
        string TableName { get; }

        void CheckSchema();

        void SendClient(MySQLEntityClient client);

    }
}
