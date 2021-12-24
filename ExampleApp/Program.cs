using ShimmyMySherbet.MySQL.EF.Internals;
using ShimmyMySherbet.MySQL.EF.Models;
using ShimmyMySherbet.MySQL.EF.Models.ConnectionProviders;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace ExampleApp
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var settings = new DatabaseSettings("127.0.0.1", "TestDatabase", "kn8hSzrg2OVhTWHN", "test");
            var provider = new ThreadedConnectionProvider(settings);
            var db = new Database(provider);

            Console.WriteLine($"Connected: {db.Connect(out string fail)}");
            Console.WriteLine($"<>Status: {fail}");
            db.CheckSchema();



            var usrs = db.Composites.Query("SELECT * FROM @TABLE;");

            foreach (var usr in usrs)
            {
                PrintUser(usr);
            }



            while (true)
            {
                //Console.Write("User ID: ");
                //var id = ulong.Parse(Console.ReadLine());
                ulong id = 0;
                Console.Write("Class: ");
                var cls = Console.ReadLine();

                var desc = $"User {id}; Class: {cls}. Local: {DateTime.Now.Ticks}";

                var cr = DateTime.Now;

                var newUser = new CompositeTestObject()
                {
                    ID = id,
                    Active = cr,
                    Class = cls,
                    Profile = desc
                };

                db.Composites.Insert(newUser);
                PrintUser(newUser);
            }


        }

        public static void PrintUser(CompositeTestObject User)
        {
            Console.WriteLine($"User ID: {User.ID}");
            Console.WriteLine($"Class: {User.Class}");
            Console.WriteLine($"Profile: {User.Profile}");
            Console.WriteLine($"Active: {User.Active.ToShortDateString()}");
            Console.WriteLine();
        }
    }
}