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
        
        public DateTime? Updated { get; set; } // Nullable

        [SQLOmitUpdate]
        public DateTime Posted { get; set; } = DateTime.Now;
    }
}