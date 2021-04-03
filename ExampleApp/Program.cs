using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using ShimmyMySherbet.MySQL.EF.Core;
using ShimmyMySherbet.MySQL.EF.Models;

namespace ExampleApp
{
    public class Program
    {
        private static void Main(string[] args)
        {
            MySQLEntityClient Client = new MySQLEntityClient("127.0.0.1", "TestDatabase", "kn8hSzrg2OVhTWHN", "test", 3306, true);
            Client.Connect();
            Console.WriteLine($"Connected: {Client.Connected}");

            if (!Client.TableExists("Users"))
            {
                Client.CreateTable<UserAccount>("Users");
            }


            int UserCount = Client.QuerySingle<int>("Select COUNT(*) From Users");
            Console.WriteLine($"Accounts: {UserCount}");


            Console.Write("Create User Account. [Y/N]");

            if (Console.ReadKey().Key == ConsoleKey.Y)
            {
                Console.WriteLine();
                UserAccount userAccount = new UserAccount();

                Console.Write("UserName: ");
                userAccount.Username = Console.ReadLine();

                Console.Write("Email Address: ");
                userAccount.EmailAddress = Console.ReadLine();

                Console.Write("SteamID: ");
                userAccount.SteamID = Convert.ToUInt64(Console.ReadLine());

                userAccount.Created = DateTime.Now;

                // Sudo Hash Data
                byte[] Buffer = new byte[16];
                using (RNGCryptoServiceProvider RNG = new RNGCryptoServiceProvider())
                    RNG.GetBytes(Buffer);

                userAccount.HashData = Buffer;

                Client.InsertUpdate(userAccount, "Users");
            }
            Console.WriteLine();

            Console.Write("Look up Username: ");
            string LookupUsername = Console.ReadLine();
            UserAccount LookupAccount = Client.QuerySingle<UserAccount>("SELECT * FROM Users WHERE Username = @0", LookupUsername);
            if (LookupAccount == null)
            {
                Console.WriteLine("No user account exists with that name.");
            } else
            {
                PrintUser(LookupAccount);
                Console.Write("Change Account Email. [Y/N]");
                if (Console.ReadKey().Key == ConsoleKey.Y)
                {
                    Console.WriteLine();
                    Console.Write("New Email Address: ");
                    string Email = Console.ReadLine();
                    LookupAccount.EmailAddress = Email;

                    Client.Update(LookupAccount, "Users");
                    Console.WriteLine("Updated.");
                } else
                {
                    Console.Write("Delete User Account. [Y/N]");
                    if (Console.ReadKey().Key == ConsoleKey.Y)
                    {
                        Client.Delete(LookupAccount, "Users");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }


            Console.WriteLine();
            Console.Write("Show 10 Users. [Y/N]");
            if (Console.ReadKey().Key == ConsoleKey.Y)
            {
                Console.WriteLine();
                foreach(UserAccount Account in Client.Query<UserAccount>("SELECT * FROM Users LIMIT 10"))
                {
                    PrintUser(Account);
                }
            }
            Console.WriteLine();





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