using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Models
{
    public sealed class SQLDefault : Attribute
    {
        public readonly object DefaultValue;
        public SQLDefault(object Default)
        {
            DefaultValue = Default;
        }
    }
}
