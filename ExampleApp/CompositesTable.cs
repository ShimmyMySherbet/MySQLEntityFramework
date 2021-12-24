using ShimmyMySherbet.MySQL.EF.Core;

namespace ExampleApp
{
    public class CompositesTable : DatabaseTable<CompositeTestObject>
    {
        public CompositesTable() : base("CompositeUsers")
        {
        }
    }
}