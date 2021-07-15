using ShimmyMySherbet.MySQL.EF.Models.Internals;

namespace ShimmyMySherbet.MySQL.EF.Models.TypeModel.Custom
{
    public class SQLIPv6 : SQLType
    {
        public override int Length => 39;
        public override string TypeName => "VARCHAR";
        public override bool NoSign => true;
    }
}