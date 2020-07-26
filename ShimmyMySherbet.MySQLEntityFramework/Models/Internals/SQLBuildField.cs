using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Models.Internals
{
    public class SQLBuildField
    {
        public SQLType Type;
        public string Name;
        public bool PrimaryKey = false;
        public bool Unique = false;
        public bool Indexed = false;
        public bool AutoIncrement = false;
        public object Default = null;
        public bool Null = false;
    }
}
