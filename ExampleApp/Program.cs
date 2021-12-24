using ShimmyMySherbet.MySQL.EF.Internals;
using ShimmyMySherbet.MySQL.EF.Models;
using ShimmyMySherbet.MySQL.EF.Models.ConnectionProviders;
using System;
using System.Collections.Generic;
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
            var dataabse = new Database(provider);

            Console.WriteLine($"Connected: {dataabse.Connect(out string fail)}");
            Console.WriteLine($"<>Status: {fail}");
            dataabse.CheckSchema();
            dataabse.AutoUpdateInstanceKey = true;



            var tstOb = new CompositeTestObject()
            {
                Active = DateTime.Now,
                Class = "Staff",
                Profile = "Info bout me",
                ID = 0 // Don't have to set it to 0
                // just to show it is 0 beforehand
            };

            dataabse.Composites.Insert(tstOb);

            Console.WriteLine($"Class: {tstOb.Class}");
            Console.WriteLine($"ID: {tstOb.ID}");

            Console.ReadLine();

            var allUserTags = dataabse.UserTags.Query("SELECT * FROM @TABLE;");
            foreach (UserTags usr in allUserTags)
            {
                PrintUserTags(usr);
            }

            while (true)
            {
                Console.Write("Name: ");
                var username = Console.ReadLine();
                Console.WriteLine("Tags:");
                var tags = new List<string>();
                while (true)
                {
                    var t = Console.ReadLine();
                    if (string.IsNullOrEmpty(t))
                        break;
                    tags.Add(t);
                }
                var taguser = new UserTags()
                {
                    UserName = username,
                    Tags = tags
                };

                dataabse.UserTags.Insert(taguser);
                PrintUserTags(taguser);
            }


        }

        public static void PrintUserTags(UserTags ob)
        {
            Console.WriteLine($"Username: {ob.UserName}");
            Console.WriteLine($"Tags:");
            foreach(var t in ob.Tags)
                Console.WriteLine($"  [Tag] {t}");
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