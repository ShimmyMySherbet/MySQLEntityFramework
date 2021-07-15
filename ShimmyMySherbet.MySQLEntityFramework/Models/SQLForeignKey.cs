using System;

namespace ShimmyMySherbet.MySQL.EF.Models
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SQLForeignKey : Attribute
    {
        private Func<string> m_Ref;
        private string m_Field;

        public string Table => m_Ref();
        private string Field => m_Field;

        public SQLForeignKey(string table, string field)
        {
            m_Ref = () => table;
            m_Field = field;
        }

        public SQLForeignKey(Func<string> table, string field)
        {
            m_Ref = table;
            m_Field = field;
        }
    }
}