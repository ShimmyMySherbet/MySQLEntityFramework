using System;
using ShimmyMySherbet.MySQL.EF.Internals;

namespace ShimmyMySherbet.MySQL.EF.Models.Internals
{
    public class SQLFieldReferance
    {
        public int Index;
        public string Name;
        public Type Type;

        public TypeReader Reader;

        public SQLFieldReferance(int Index, string Name, Type T)
        {
            this.Index = Index;
            this.Name = Name;
            Type = T;
            Reader = SQLConverter.GetTypeReader(T, Index);
        }
    }
}