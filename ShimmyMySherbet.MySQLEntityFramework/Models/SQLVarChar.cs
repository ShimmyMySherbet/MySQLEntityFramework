using ShimmyMySherbet.MySQL.EF.Models.Exceptions;
using ShimmyMySherbet.MySQL.EF.Models.Internals;
using ShimmyMySherbet.MySQL.EF.Models.TypeModel;

namespace ShimmyMySherbet.MySQL.EF.Models
{
    public class SQLVarChar : SQLType
    {
        private int m_Length;
        public override int Length => m_Length;

        public SQLVarChar(int Length)
        {
            if (Length <= 0)
            {
                throw new SQLInvalidLengthException("VarChar length cannot be less than 1");
            }
            m_Length = Length;
        }
    }
}