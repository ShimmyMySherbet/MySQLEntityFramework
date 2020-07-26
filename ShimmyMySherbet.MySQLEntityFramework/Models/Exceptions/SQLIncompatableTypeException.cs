using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Models.Exceptions
{
    public class SQLIncompatableTypeException : Exception
    {
        private string Message_;
        public SQLIncompatableTypeException(string FieldName = null)
        {
            if (FieldName == null)
            {
                Message_ = "An incompatible type was supplied. Change the field type or manually declare it's SQL type.";
            } else
            {
                Message_ = $"Field '{FieldName}' has an incompatible type. Change the field type, manually declare it's SQL type, or declare it as SQLIgnore.";
            }
        }
        public override string Message => Message_;
    }
}
