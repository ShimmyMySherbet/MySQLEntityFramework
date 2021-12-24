using System;
using ShimmyMySherbet.MySQL.EF.Models;
using ShimmyMySherbet.MySQL.EF.Models.TypeModel;
using ShimmyMySherbet.MySQL.EF.Models.TypeModel.Custom;

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

        [SQLVarChar(32)]
        public string EmailAddress { get; set; }

        [SQLIPv4]
        public string IPv4 { get; set; }

        [SQLIPv6]
        public string IPv6 { get; set; }

        public DateTime? Created { get; set; }
    }

    public class CompositeTestObject // Testing composite keys (multipel primary keys)
    {
        [SQLPrimaryKey, SQLAutoIncrement]
        public ulong ID { get; set; }

        [SQLPrimaryKey, SQLDefault("User"), SQLVarChar(128)]
        public string Class { get; set; }

        public string Profile { get; set; }

        public DateTime Active { get; set; }
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