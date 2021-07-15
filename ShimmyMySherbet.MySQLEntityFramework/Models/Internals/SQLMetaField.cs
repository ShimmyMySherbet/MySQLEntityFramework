using ShimmyMySherbet.MySQL.EF.Internals;
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
        public int FieldIndex;
        public IClassField Field;
        public bool Omit = false;
        public bool OmitOnNull = false;
        public bool OmitOnUpdate = false;

        public bool IsPrimaryKey = false;
        public bool IsForeignKey = false;
        public bool AutoIncrement = false;
        public bool DBNull = false;
        public bool Unique = false;
        public bool Ignore = false;
        


        public bool IncludeUpdate
        {
            get => !OmitOnUpdate;
            set => OmitOnUpdate = !value;
        }
        public SQLMetaField(string Name = null, int Index = 0, IClassField field = null, bool omitupdate = false)
        {
            this.Name = Name;
            this.FieldIndex = Index;
            this.Field = field;
        }
        public SQLMetaField(string Name = null, int Index = 0, FieldInfo field = null, bool omitupdate = false)
        {
            this.Name = Name;
            this.FieldIndex = Index;
            this.Field = new ClassField(field, Index);
        }
        public SQLMetaField()
        {
        }
    }
}
