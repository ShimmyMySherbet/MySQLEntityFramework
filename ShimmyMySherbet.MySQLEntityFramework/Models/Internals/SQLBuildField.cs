using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Models.Internals
{
    public class SQLBuildField
    {
        public SQLType Type;
        public string OverrideType = null;

        public bool SetLength = false;

        public string SQLRepresentation
        {
            get
            {
                if (OverrideType != null)
                {
                    return OverrideType;
                }
                return Type.SQLRepresentation;
            }
        }

        public string Name;
        public bool PrimaryKey = false;
        public bool Unique = false;
        public bool Indexed = false;
        public bool AutoIncrement = false;
        public object Default = null;
        public bool Null = false;
    }
}
