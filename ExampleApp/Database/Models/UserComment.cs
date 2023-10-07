using System;
using ShimmyMySherbet.MySQL.EF.Models;

namespace ExampleApp.Database.Models
{
    [SQLCharSet(SQLCharSet.ServerDefault)] // Specifying default charset
    public class UserComment
    {   // Composite primary key
        [SQLPrimaryKey]
        public ulong UserID { get; set; }

        [SQLPrimaryKey]
        public ulong PostID { get; set; }

        public string Content { get; set; }

        [SQLOmitInsert, SQLNull] // Only provide this value when updating, leave default on insert
        public DateTime? Updated { get; set; } // Nullable / optional value

        [SQLOmitUpdate] // Only provide this value when inserting, leave default on insert
        public DateTime Posted { get; set; } = DateTime.Now;
    }
}