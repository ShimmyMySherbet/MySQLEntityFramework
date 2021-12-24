using ShimmyMySherbet.MySQL.EF.Core;

namespace ExampleApp
{
    public class CompositesTable : DatabaseTable<CompositeTestObject>
    {
        public CompositesTable() : base("CompositeUsers")
        {
        }
    }

    public class UserTagsTable : DatabaseTable<UserTags>
    {
        public UserTagsTable() : base("SertestTable")
        {
        }
    }
}