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
            var settings = new DatabaseSettings("127.0.0.1", "TestDatabase", "kn8hSzrg2OVhTWHN", "test");
            var db = new Database(settings);

            Console.WriteLine($"Connected: {db.Connect()}");
            db.CheckSchema();

         
            var user = db.Accounts.QuerySingle("SELECT * FROM @TABLE WHERE Username=@0;", "user_10");
            PrintUser(user);




            UserPost post = new UserPost()
            {
                UserID = user.ID,
                Title = "Sqid is gei",
                Content = "He really is",
                Posted = DateTime.Now
            };


            db.Posts.Insert(post);


            foreach(var p in db.Posts.GetPosts(user))
            {
                Console.WriteLine($"{p.Title} > {p.Content}");
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
            Console.WriteLine($"Date Created: {User.Created.ToLongDateString()} {User.Created.ToShortTimeString()}");
            Console.WriteLine();
        }
    }
}