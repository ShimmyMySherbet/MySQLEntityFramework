using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Models.Internals
{
    public class LocalFieldReferance
    {
        public string FieldName;
        public string SQLName;
        public Type Type;

        public LocalFieldReferance(string FName, string SName, Type T)
        {
            FieldName = FName;
            SQLName = SName;
            Type = T;
        }
    }
}
