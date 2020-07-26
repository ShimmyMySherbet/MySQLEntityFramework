using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Models.TypeModel
{
    public sealed class SQLLength : Attribute
    {
        public int Length;
        public SQLLength(int Length)
        {
            this.Length = Length;
        }
    }
}
