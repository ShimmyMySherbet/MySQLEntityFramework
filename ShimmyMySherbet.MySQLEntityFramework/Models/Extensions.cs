using MySql.Data.MySqlClient;
using System;
using System.Reflection;

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


        public static void EFExecuteNonQuery(this MySqlConnection connection, string command, params object[] parameters)
        {




        }
    }
}