namespace ShimmyMySherbet.MySQL.EF.Models.Internals
{
    public class SQLBuildField
    {
        public SQLType Type;
        public SQLType OverrideType = null;

        public bool SetLength = false;

        public string SQLRepresentation
        {
            get
            {
                if (OverrideType != null)
                {
                    return OverrideType.SQLRepresentation;
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