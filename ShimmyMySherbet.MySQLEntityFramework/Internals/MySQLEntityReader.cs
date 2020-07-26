using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using ShimmyMySherbet.MySQL.EF.Internals;
using ShimmyMySherbet.MySQL.EF.Models.TypeModel;
#pragma warning disable CA2100
namespace ShimmyMySherbet.MySQL.EF.Internals
{
    public sealed class MySQLEntityReader
    {
        public SQLTypeHelper IndexedHelper;
        public List<T> RetriveFromDatabase<T>(MySqlConnection Connection, string Command, params object[] Arguments)
        {
            if (Connection.State != System.Data.ConnectionState.Open) Connection.Open();
            using (MySqlCommand Command_ = new MySqlCommand(Command, Connection))
            {
                for (int i = 0; i < Arguments.Length; i++)
                {
                    if (Command.Contains($"{{{i}}}")) Command_.Parameters.AddWithValue($"{{{i}}}", Arguments[i]);
                    if (Command.Contains($"@{i}")) Command_.Parameters.AddWithValue($"@{i}", Arguments[i]);
                }
                using (MySqlDataReader Reader = Command_.ExecuteReader())
                {
                    SQLConverter Converter = new SQLConverter();
                    Converter.TypeHelper = IndexedHelper;
                    return Converter.ReadModelsFromReader<T>(Reader);
                }
            }
        }
        public List<T> RetriveClassesFromDatabase<T>(MySqlConnection Connection, string Command, params object[] Arguments)
        {
            if (Connection.State != System.Data.ConnectionState.Open) Connection.Open();
            using (MySqlCommand Command_ = new MySqlCommand(Command, Connection))
            {
                for (int i = 0; i < Arguments.Length; i++)
                {
                    if (Command.Contains($"{{{i}}}")) Command_.Parameters.AddWithValue($"{{{i}}}", Arguments[i]);
                    if (Command.Contains($"@{i}")) Command_.Parameters.AddWithValue($"@{i}", Arguments[i]);
                }
                using (MySqlDataReader Reader = Command_.ExecuteReader())
                {
                    SQLConverter Converter = new SQLConverter();
                    return Converter.ReadClasses<T>(Reader);
                }
            }
        }
        public List<T> RetriveSQLTypesFromDatabase<T>(MySqlConnection Connection, string Command, params object[] Arguments)
        {
            if (Connection.State != System.Data.ConnectionState.Open) Connection.Open();
            using (MySqlCommand Command_ = new MySqlCommand(Command, Connection))
            {
                for (int i = 0; i < Arguments.Length; i++)
                {
                    if (Command.Contains($"{{{i}}}")) Command_.Parameters.AddWithValue($"{{{i}}}", Arguments[i]);
                    if (Command.Contains($"@{i}")) Command_.Parameters.AddWithValue($"@{i}", Arguments[i]);
                }
                using (MySqlDataReader Reader = Command_.ExecuteReader())
                {
                    SQLConverter Converter = new SQLConverter();
                    return Converter.ReadSQLBaseTypes<T>(Reader, IndexedHelper);
                }
            }
        }
    }
}
