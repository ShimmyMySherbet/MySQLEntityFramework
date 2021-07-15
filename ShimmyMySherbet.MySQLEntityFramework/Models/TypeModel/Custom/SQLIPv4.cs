using ShimmyMySherbet.MySQL.EF.Models.Internals;

namespace ShimmyMySherbet.MySQL.EF.Models.TypeModel.Custom
{
    public class SQLIPv4 : SQLType
    {
        public override int Length => 15;
        public override bool NoSign => true;
        public override string TypeName => "VARCHAR";
    }
}