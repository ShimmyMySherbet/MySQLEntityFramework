using ShimmyMySherbet.MySQL.EF.Models;
using System;

namespace ExampleApp
{
    public class UserAccount
    {
        [SQLPrimaryKey, SQLAutoIncrement]
        public int ID { get; set; }

        [SQLUnique]
        public string Username { get; set; }

        public byte[] HashData { get; set; }

        [SQLIndex]
        public ulong SteamID { get; set; }

        public string EmailAddress { get; set; }

        public DateTime? Created { get; set; }
    }

    public class UserPost
    {
        [SQLPrimaryKey, SQLAutoIncrement]
        public int ID { get; set; }

        [SQLIndex]
        public int UserID
        {
            get; set;
        }

        public string Title
        {
            get; set;
        }

        public string Content
        {
            get; set;
        }

        public DateTime Posted
        {
            get; set;
        }
    }
}