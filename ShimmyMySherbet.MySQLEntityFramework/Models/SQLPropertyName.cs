using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Models
{
    public sealed class SQLPropertyName : Attribute
    {
        public readonly string Name;
        public SQLPropertyName(string Name)
        {
            this.Name = Name;
        }
    }
}
