using ShimmyMySherbet.MySQL.EF.Core;
using ShimmyMySherbet.MySQL.EF.Models;
using System;
using System.Diagnostics;

namespace ExampleApp
{
    public class Program
    {
        private static void Main(string[] args)
        {
            MySQLEntityClient Client = new MySQLEntityClient("127.0.0.1", "TestDatabase", "kn8hSzrg2OVhTWHN", "test", 3306, true);
            Client.Connect();
            var mysqlConnection = Client.ActiveConnection;

            Console.WriteLine($"Connected: {Client.Connected}");

            if (!Client.TableExists("Users"))
            {
                Client.CreateTable<UserAccount>("Users");
            } else
            {
                Client.DeleteTable("Users");
                Client.CreateTable<UserAccount>("Users");
            }

            var max = 1000;

            var exe = new BulkInserter<UserAccount>(mysqlConnection, "Users");

            Console.WriteLine("Starting bulk insert build");

            var build = new Stopwatch();
            build.Start();

            for (uint i = 0; i < max; i++)
            {
                var user = new UserAccount()
                {
                    Created = DateTime.Now,
                    EmailAddress = $"EXTUser{i}",
                    HashData = new byte[6] /*cbf populating with rand data*/,
                    SteamID = i,
                    Username = $"user_{i}"
                };
                exe.Insert(user);
            }

            build.Stop();
            var buildSpeed = Math.Round((max / (double)build.ElapsedMilliseconds) * 1000, 2);
            Console.WriteLine($"Completed build of {max} inserts in {build.ElapsedMilliseconds}ms @ {buildSpeed}p/s");

            Stopwatch commit = new Stopwatch();
            commit.Start();

            var c = exe.Commit();

            commit.Stop();

            var comitSpeed = Math.Round((max / (double)commit.ElapsedMilliseconds) * 1000, 2);
            Console.WriteLine($"Comitted {max} entries in {commit.ElapsedMilliseconds}ms @ {comitSpeed}p/s");
            Console.WriteLine($"Rows Modified: {c}");

            Console.ReadLine();
        }

        public static void PrintUser(UserAccount User)
        {
            Console.WriteLine($"User ID: {User.ID}");
            Console.WriteLine($"Username: {User.Username}");
            Console.WriteLine($"Hash Data: {string.Join(", ", User.HashData)}");
            Console.WriteLine($"SteamID: {User.SteamID}");
            Console.WriteLine($"Email Address: {User.EmailAddress}");
            Console.WriteLine($"Date Created: {User.Created.ToLongDateString()} {User.Created.ToShortTimeString()}");
        }
    }

    public class UserAccount
    {
        [SQLPrimaryKey, SQLAutoIncrement, SQLOmit]
        public int ID;

        [SQLUnique]
        public string Username;

        public byte[] HashData;

        [SQLIndex]
        public ulong SteamID;

        public string EmailAddress;

        public DateTime Created;
    }
}