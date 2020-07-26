using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Models.Internals
{
    public class SQLFieldReferance
    {
        public int Index;
        public string Name;
        public Type Type;

        public SQLFieldReferance(int Index, string Name, Type T)
        {
            this.Index = Index;
            this.Name = Name;
            Type = T;
        }
    }
}
