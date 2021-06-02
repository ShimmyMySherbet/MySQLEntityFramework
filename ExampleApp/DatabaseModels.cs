using ShimmyMySherbet.MySQL.EF.Models;
using System;

namespace ExampleApp
{
    public class UserAccount
    {
        [SQLPrimaryKey, SQLAutoIncrement]
        public int ID;

        [SQLUnique]
        public string Username;

        public byte[] HashData;

        [SQLIndex]
        public ulong SteamID;

        public string EmailAddress;

        public DateTime? Created;
    }

    public class UserPost
    {
        [SQLPrimaryKey, SQLAutoIncrement]
        public int ID;

        [SQLIndex]
        public int UserID;

        public string Title;

        public string Content;

        public DateTime Posted;
    }
}