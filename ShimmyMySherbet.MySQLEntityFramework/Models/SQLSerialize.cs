using System;

namespace ShimmyMySherbet.MySQL.EF.Models
{
    public sealed class SQLSerialize : Attribute
    {
        public ESerializeFormat Format { get; }

        public SQLSerialize()
        {
            Format = ESerializeFormat.JSON;
        }

        public SQLSerialize(ESerializeFormat fomat)
        {
            Format = fomat;
        }
    }
}