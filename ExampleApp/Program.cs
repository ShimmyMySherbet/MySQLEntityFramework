using ShimmyMySherbet.MySQL.EF.Models;
using System;

namespace ExampleApp
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var settings = new DatabaseSettings("127.0.0.1", "TestDatabase", "kn8hSzrg2OVhTWHN", "test");
            var db = new Database(settings);

            Console.WriteLine($"Connected: {db.Connect()}");
            db.CheckSchema();

            var users = db.Accounts.Query("SELECT * FROM @TABLE", "user_10");

            foreach(var u in users)
            {
                PrintUser(u);
            }


            Console.ReadLine();
        }

        public static void PrintUser(UserAccount User)
        {
            Console.WriteLine($"User ID: {User.ID}");
            Console.WriteLine($"Username: {User.Username}");
            Console.WriteLine($"Hash Data: {string.Join(", ", User.HashData)}");
            Console.WriteLine($"SteamID: {User.SteamID}");
            Console.WriteLine($"Email Address: {User.EmailAddress}");
            if (User.Created.HasValue)
                Console.WriteLine($"Date Created: {User.Created.Value.ToLongDateString()} {User.Created.Value.ToShortTimeString()}");
            else Console.WriteLine("Date Created: NULL");
            Console.WriteLine();
        }
    }
}