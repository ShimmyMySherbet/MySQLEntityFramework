using System.Data;

namespace ShimmyMySherbet.MySQL.EF.Internals
{
    public delegate object TypeReader(IDataReader reader, int index);
}