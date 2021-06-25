using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Models.Internals
{
    public class SQLMetaField
    {
        public string Name;
        public int Index;
        public FieldInfo Field;
        public bool OmitUpdate = false;

        public bool IncludeUpdate = true;
        public SQLMetaField(string Name = null, int Index = 0, FieldInfo Field = null, bool omitupdate = false)
        {
            this.Name = Name;
            this.Index = Index;
            this.Field = Field;
            OmitUpdate = omitupdate;
        }
    }
}
