using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Models.TypeModel
{
    public sealed class SQLNetType : Attribute
    {
        public Type Type;
        public SQLNetType(Type T)
        {
            Type = T;
        }
    }
}
