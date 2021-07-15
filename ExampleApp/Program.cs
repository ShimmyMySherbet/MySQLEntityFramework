using ShimmyMySherbet.MySQL.EF.Internals;
using ShimmyMySherbet.MySQL.EF.Models;
using ShimmyMySherbet.MySQL.EF.Models.ConnectionProviders;
using System;
using System.Diagnostics;

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



            var users = db.Accounts.Query("SELECT * FROM @TABLE");

            foreach (var u in users)
            {
                PrintUser(u);
            }

            while (true)
            {
                Console.Write("new user: ");
                var usrd = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(usrd)) break;

                var usr = new UserAccount()
                {
                    SteamID = (ulong)Math.Abs(usrd.GetHashCode()),
                    Created = DateTime.Now,
                    HashData = new byte[usrd.Length],
                    EmailAddress = $"{usrd}@gg.com",
                    Username = usrd
                };

                Stopwatch sw = new Stopwatch();
                sw.Start();
                db.Accounts.Insert(usr);
                sw.Stop();

                Console.WriteLine($"Inserted in {sw.ElapsedMilliseconds}ms ({sw.ElapsedTicks} ticks)");
                //Console.WriteLine($"Field: {EntityCommandBuilder.F_Build / 10000}ms ({EntityCommandBuilder.F_Build} ticks), Command Total: {EntityCommandBuilder.C_Build / 10000}ms ({EntityCommandBuilder.C_Build} ticks)");
            }
            Console.WriteLine("done.");
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