using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Models.Exceptions
{
    public class NoSQLFieldsException : Exception
    {
        public override string Message => "The type does not contain any SQL accessible fields.";
    }
}
