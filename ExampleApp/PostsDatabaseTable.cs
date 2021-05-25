using ShimmyMySherbet.MySQL.EF.Core;
using System.Collections.Generic;

namespace ExampleApp
{
    public class PostsDatabaseTable : DatabaseTable<UserPost>
    {
        public PostsDatabaseTable(string name) : base(name)
        {
        }

        public List<UserPost> GetPosts(UserAccount acc)
        {
            return Query("SELECT * FROM @TABLE WHERE UserID=@0", acc.ID);
        }
    }
}