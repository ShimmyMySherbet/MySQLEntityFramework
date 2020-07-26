using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Models.Exceptions
{
    public sealed class NoPrimaryKeyException : Exception
    {
        public override string Message => "The object does not have an associated Primary Key.";
    }
}
