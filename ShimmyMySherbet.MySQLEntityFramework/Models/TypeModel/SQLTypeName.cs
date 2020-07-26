using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Models.TypeModel
{
    public sealed class SQLTypeName : Attribute
    {
        public string Name;
        public SQLTypeName(string TypeName)
        {
            Name = TypeName;
        }
    }
}
