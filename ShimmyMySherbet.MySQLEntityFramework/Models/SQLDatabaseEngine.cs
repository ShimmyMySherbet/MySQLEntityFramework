using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Models
{
    public sealed class SQLDatabaseEngine : Attribute
    {
        public readonly string DatabaseEngine;
        public SQLDatabaseEngine(string Engine)
        {
            DatabaseEngine = Engine;
        }
    }
}
