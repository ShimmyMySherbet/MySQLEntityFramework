using MySql.Data.MySqlClient;
using ShimmyMySherbet.MySQL.EF.Internals;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Models
{
    public static class Extensions
    {
        public static bool IsAttributeSet<T>(MemberInfo member) where T : Attribute
        {
            return Attribute.IsDefined(member, typeof(T));
        }

        public static T GetAttribute<T>(MemberInfo member) where T : Attribute
        {
            var obj = Attribute.GetCustomAttribute(member, typeof(T));
            if (obj != null && obj is T t) return t;
            return default(T);
        }

        public static int EFExecuteNonQuery(this MySqlConnection connection, string command, params object[] parameters)
        {
            using (var cmd = EntityCommandBuilder.BuildCommand(connection, command, parameters))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        public static async Task<int> EFExecuteNonQueryAsync(this MySqlConnection connection, string command, params object[] parameters)
        {
            using (var cmd = EntityCommandBuilder.BuildCommand(connection, command, parameters))
            {
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task<int> EFDeleteAsync<T>(this MySqlConnection connection, T instance, string table)
        {
            using (var cmd = EntityCommandBuilder.BuildDeleteCommand(instance, table, connection))
            {
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        public static int EFDelete<T>(this MySqlConnection connection, T instance, string table)
        {
            using (var cmd = EntityCommandBuilder.BuildDeleteCommand(instance, table, connection))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        public static int EFInsert<T>(this MySqlConnection connection, T instance, string table)
        {
            using (var cmd = EntityCommandBuilder.BuildInsertCommand(instance, table, out _, connection))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        public static async Task<int> EFInsertAsync<T>(this MySqlConnection connection, T instance, string table)
        {
            using (var cmd = EntityCommandBuilder.BuildInsertCommand(instance, table, out _, connection))
            {
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task<int> EFInsertUpdateAsync<T>(this MySqlConnection connection, T instance, string table)
        {
            using (var cmd = EntityCommandBuilder.BuildInsertUpdateCommand(instance, table, connection))
            {
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        public static int EFInsertUpdate<T>(this MySqlConnection connection, T instance, string table)
        {
            using (var cmd = EntityCommandBuilder.BuildInsertUpdateCommand(instance, table, connection))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        public static async Task<int> EFUpdateAsync<T>(this MySqlConnection connection, T instance, string table)
        {
            using (var cmd = EntityCommandBuilder.BuildUpdateCommand(instance, table, connection))
            {
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        public static int EFUpdate<T>(this MySqlConnection connection, T instance, string table)
        {
            using (var cmd = EntityCommandBuilder.BuildUpdateCommand(instance, table, connection))
            {
                return cmd.ExecuteNonQuery();
            }
        }
    }
}