using System.ComponentModel;
using System.Diagnostics;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ExampleApp.Database;
using ExampleApp.Database.Models;
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
            //Database.AutoUpdateInstanceKey = true; // Auto update class auto increment primary key on insert

            RunConsole().GetAwaiter().GetResult();
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
                var arguments = command.Substring(targetMethod.Length + 1).Split(' ').ToArray();
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
                    if (!converter.CanConvertTo(mParam[i].ParameterType) && mParam[i].ParameterType != typeof(ulong))
                    {
                        Console.WriteLine($"Cant convert to {mParam[i].ParameterType.Name}");
                        continue;
                    }

                    if (mParam[i].ParameterType == typeof(ulong))
                    {
                        param[i] = ulong.Parse(arguments[i]);
                    }
                    else
                    {
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
    }
}