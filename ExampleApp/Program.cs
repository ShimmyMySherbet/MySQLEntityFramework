using ShimmyMySherbet.MySQL.EF.Models;
using ShimmyMySherbet.MySQL.EF.Models.ConnectionProviders;
using System;

namespace ExampleApp
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var settings = new DatabaseSettings("127.0.0.1", "TestDatabase", "kn8hSzrg2OVhTWHN", "test");
            ThreadedConnectionProvider provider = new ThreadedConnectionProvider(settings);


            var db = new Database(provider);

            Console.WriteLine($"Connected: {db.Connect(out string fail)}");
            Console.WriteLine($"<>Status: {fail}");
            db.CheckSchema();

            var users = db.Accounts.Query("SELECT * FROM @TABLE", "user_10");

            foreach(var u in users)
            {
                PrintUser(u);
            }


            var usr = new UserAccount()
            {
                SteamID = 4324234324,
                Created = DateTime.Now,
                HashData = new byte[5],
                EmailAddress = "usrss@gg.com",
                Username = "userss"
            };

            //db.Accounts.Insert(usr);





            Console.ReadLine();
        }

        public static void PrintUser(UserAccount User)
        {
            Console.WriteLine($"User ID: {User.ID}");
            Console.WriteLine($"Username: {User.Username}");
            Console.WriteLine($"Hash Data Length: {User.HashData.Length}");
            Console.WriteLine($"SteamID: {User.SteamID}");
            Console.WriteLine($"Email Address: {User.EmailAddress}");
            if (User.Created.HasValue)
                Console.WriteLine($"Date Created: {User.Created.Value.ToLongDateString()} {User.Created.Value.ToShortTimeString()}");
            else Console.WriteLine("Date Created: NULL");
            Console.WriteLine();
        }
    }
}