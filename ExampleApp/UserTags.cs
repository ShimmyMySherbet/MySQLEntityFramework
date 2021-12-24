using System.Collections.Generic;
using ShimmyMySherbet.MySQL.EF.Models;
using ShimmyMySherbet.MySQL.EF.Models.TypeModel;

namespace ExampleApp
{
    public class UserTags
    {
        [SQLPrimaryKey, SQLVarChar(128)]
        public string UserName { get; set; }

        [SQLSerialize(ESerializeFormat.JSON)]
        public List<string> Tags { get; set; } = new List<string>();
    }
}