using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ExampleApp.Database;
using ExampleApp.Database.Models;
using Org.BouncyCastle.Bcpg;
using ShimmyMySherbet.MySQL.EF.Models;
using ShimmyMySherbet.MySQL.EF.Models.ConnectionProviders;

namespace ExampleApp
{
    public class Program
    {
        public static DatabaseManager Database;

        private static void Main(string[] args)
        {
            // Can be used in serialized configs, e.g., rocket plugin config
            var settings = new DatabaseSettings("127.0.0.1", "TestDatabase", "kn8hSzrg2OVhTWHN", "test");
            // Different providers change how connections are managed.
            var provider = new SingleConnectionProvider(settings); // singleton provider
            // Database manager class that contains tables
            Database = new DatabaseManager(provider);

            Console.WriteLine($"Connected: {Database.Connect(out string fail)}"); // try to connect to database
            Console.WriteLine($"Status Message: {fail}"); // print user friendly error if failed
            Database.CheckSchema(); // Check Schema (ensure tables exist, if not, create them)
            Database.AutoUpdateInstanceKey = true; // Auto update class auto increment primary key on insert

            RunConsole().GetAwaiter().GetResult();
        }

        private static void Help()
        {
            var ms = typeof(Program).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            foreach (var m in ms)
            {
                Console.WriteLine($"{m.Name}({string.Join(", ", m.GetParameters().Select(x => $"{x.ParameterType.Name} {x.Name}"))})");
            }
        }

        private static async Task<ulong> CreateUser(string username, string email)
        {
            var newUser = new UserAccount()
            {
                EmailAddress = email,
                Username = username
            };
            await Database.Users.InsertAsync(newUser);
            // return user ID as allocated by the database
            return newUser.ID;
        }

        private static void ShowSchema(string table)
        {
            Console.WriteLine(Database.Client.QuerySingle<string>($"SHOW CREATE TABLE `{table}`"));
        }

        private static async Task GrantPermission(ulong userID, string permission, ulong granter)
        {
            // gets user perms from table, or creates new one if user does not have any perms
            var perms = Database.Permissions.QuerySingle("SELECT * FROM @TABLE WHERE UserID=@0;", userID)
                ?? new UserPermissions() { UserID = userID };

            if (!perms.Permissions.Any(x => x.PermissionID == permission))
            {
                perms.Permissions.Add(new Permission() { PermissionID = permission, GrantedBy = granter });

                await Database.Permissions.InsertUpdateAsync(perms); // Update existing or Insert new row
            }
        }

        private static async Task ShowBalances()
        {
            var balances = await Database.Balances.QueryAsync("SELECT * FROM @TABLE;");
            foreach (var bal in balances)
            {
                Console.WriteLine($"[{bal.UserID}] ${bal.Balance}");
            }
        }

        private static void ShowBalancesSync()
        {
            var balances = Database.Balances.Query("SELECT * FROM @TABLE;");
            foreach (var bal in balances)
            {
                Console.WriteLine($"[{bal.UserID}] ${bal.Balance}");
            }
        }

        private static ulong GetUserCountSync()
        {
            return Database.Users.GetRowCount();
        }

        private static async Task<ulong> GetUserCount()
        {
            return await Database.Users.GetRowCountAsync();
        }

        private static async Task PostComment(ulong uid, ulong postid, string value)
        {
            var ent = new UserComment()
            {
                PostID = postid,
                UserID = uid,
                Content = value,
                Posted = DateTime.Now,
                Updated = DateTime.Now
            };
            await Database.Comments.InsertUpdateAsync(ent);
        }


        private static async Task ShowComments(ulong postID)
        {
            var comments = await Database.Comments.QueryAsync("SELECT * FROM @TABLE WHERE PostID=@0;", postID);
            foreach (var c in comments)
            {
                Console.WriteLine($"PostID: {c.PostID}");
                Console.WriteLine($"UserID: {c.UserID}");
                Console.WriteLine($"Posted: {c.Posted.ToShortDateString()}");
                if (c.Updated != null)
                {
                    Console.WriteLine($"Updated: {c.Updated.Value.ToShortDateString()}");
                }
                Console.WriteLine();
                Console.WriteLine(c.Content);
                Console.WriteLine();
            }

        }

        private static async Task ModifyBalance(ulong uid, double amount)
        {
            var bal = await Database.Balances.QuerySingleAsync($"SELECT * FROM @TABLE WHERE UserID=@0;", uid)
                ?? new UserBalance() { UserID = uid };
            bal.Balance += amount;
            await Database.Balances.InsertUpdateAsync(bal);
        }

        #region "Test console code"

        private static async Task RunConsole()
        {
            var converter = new StringConverter();
            var sw = new Stopwatch();
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Green; // fancy
                Console.Write("  > ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                var command = Console.ReadLine();
                var targetMethod = command.Split(' ')[0];
                var arguments = targetMethod.Length != command.Length ? command.Substring(targetMethod.Length + 1).Split(' ').ToArray() : new string[0];
                Console.ForegroundColor = ConsoleColor.Red;
                var mInfo = typeof(Program).GetMethod(targetMethod, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.IgnoreCase);

                if (mInfo == null)
                {
                    Console.WriteLine("Command not found.");
                    continue;
                }

                var mParam = mInfo.GetParameters();

                var param = new object[mParam.Length];

                if (arguments.Length < mParam.Length)
                {
                    Console.WriteLine($"Not enough arguments");
                    continue;
                }

                for (int i = 0; i < param.Length; i++)
                {
                    if (mParam[i].ParameterType == typeof(ulong))
                    {
                        param[i] = ulong.Parse(arguments[i]);
                    }
                    else if (mParam[i].ParameterType == typeof(double))
                    {
                        param[i] = double.Parse(arguments[i]);
                    }
                    else if (mParam[i].ParameterType == typeof(decimal))
                    {
                        param[i] = decimal.Parse(arguments[i]);
                    }
                    else
                    {
                        if (!converter.CanConvertTo(mParam[i].ParameterType) && mParam[i].ParameterType != typeof(ulong))
                        {
                            Console.WriteLine($"Cant convert to {mParam[i].ParameterType.Name}");
                            continue;
                        }
                        param[i] = converter.ConvertTo(arguments[i], mParam[i].ParameterType);
                    }
                }
                Console.ForegroundColor = ConsoleColor.Yellow;
                sw.Restart();
                var ret = mInfo.Invoke(null, param);
                if (ret is Task tsk)
                {
                    await tsk;
                    sw.Stop();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Finished Async in {Math.Round(sw.ElapsedTicks / 10000f, 4)}ms");

                    // check if it's Type<T> with result
                    var f_result = ret.GetType().GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);
                    if (f_result != null)
                    {
                        var rValue = f_result.GetValue(ret);
                        Console.WriteLine($"Async Retun Value: {(rValue == null ? "null" : rValue)}");
                    }
                    else
                    {
                        Console.WriteLine($"No return value.");
                    }
                }
                else
                {
                    sw.Stop();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Finished in {Math.Round(sw.ElapsedTicks / 10000f, 4)}ms");
                    if (mInfo.ReturnType != typeof(void))
                    {
                        Console.WriteLine($"Retun Value: {(ret == null ? "null" : ret)}");
                    }
                }
            }
        }

        #endregion "Test console code"


        private static async Task TestTypeConversion<T>()
        {
            try
            {
                var r = await Database.Balances.QuerySingleAsync<T>("SELECT COUNT(*) FROM @TABLE;");
                Console.WriteLine($"BIGINT -> {typeof(T).Name} pass [{r}]");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BIGINT -> {typeof(T).Name} fail: {ex.Message}");
            }
        }

        private static async Task TestConversions()
        {
            Console.WriteLine("testing reads from BIGINT");
            await TestTypeConversion<byte>();
            await TestTypeConversion<sbyte>();
            await TestTypeConversion<short>();
            await TestTypeConversion<ushort>();
            await TestTypeConversion<int>();
            await TestTypeConversion<uint>();
            await TestTypeConversion<long>();
            await TestTypeConversion<ulong>();
            await TestTypeConversion<decimal>();
            await TestTypeConversion<float>();
            await TestTypeConversion<string>();
            Console.WriteLine("done.");
        }
    }
}