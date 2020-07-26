using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Models.TypeModel
{
    public sealed class SQLSigned : Attribute
    {
        public bool Signed;
        public SQLSigned(bool Signed)
        {
            this.Signed = Signed;
        }
    }
}
