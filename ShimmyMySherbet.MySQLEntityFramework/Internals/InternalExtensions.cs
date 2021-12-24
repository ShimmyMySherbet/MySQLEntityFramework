using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace ShimmyMySherbet.MySQL.EF.Internals
{
    internal static class InternalExtensions
    {
        public static void Add(this MySqlCommand com, IEnumerable<ParamObject> parameters)
        {
            foreach (var p in parameters)
            {
                com.Parameters.AddWithValue(p.Key, p.Value);
            }
        }
    }
}