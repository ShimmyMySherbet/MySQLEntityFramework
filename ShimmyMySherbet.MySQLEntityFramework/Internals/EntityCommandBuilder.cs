﻿using MySql.Data.MySqlClient;
using ShimmyMySherbet.MySQL.EF.Models;
using ShimmyMySherbet.MySQL.EF.Models.Exceptions;
using ShimmyMySherbet.MySQL.EF.Models.Internals;
using ShimmyMySherbet.MySQL.EF.Models.TypeModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

#pragma warning disable CA2100

namespace ShimmyMySherbet.MySQL.EF.Internals
{
    public class EntityCommandBuilder
    {
        private SQLTypeHelper m_TypeHelper = new SQLTypeHelper();

        [ThreadStatic]
        private static Dictionary<Type, List<IClassField>> m_FieldCache = new Dictionary<Type, List<IClassField>>();

        internal static List<IClassField> GetClassFields(Type model, bool autoOmit = true)
        {
            if (m_FieldCache.ContainsKey(model))
            {
                return m_FieldCache[model];
            }
            var fields = new List<IClassField>();
            var index = 0;
            foreach (var field in model.GetFields())
            {
                var f = new ClassField(field, index);

                if (!f.Meta.Ignore)
                {
                    index++;
                    fields.Add(f);
                }
            }


            if (model.GetCustomAttribute<SQLIngoreProperties>() == null)
            {
                foreach (var property in model.GetProperties())
                {
                    var p = new ClassProperty(property, index);
                    if (!p.Meta.Ignore)
                    {
                        index++;
                        fields.Add(p);
                    }
                }
            }

            m_FieldCache[model] = fields;

            return fields;
        }

        internal static List<IClassField> GetClassFields<T>() => GetClassFields(typeof(T));

        internal static List<IClassField> GetClassFields<T>(Func<IClassField, bool> selector) => GetClassFields(typeof(T)).Where(selector).ToList();

        public static MySqlCommand BuildCommand(string command, params object[] args)
        {
            MySqlCommand sqlCommand = new MySqlCommand(command);
            for (int i = 0; i < args.Length; i++)
            {
                if (command.Contains($"{{{i}}}")) sqlCommand.Parameters.AddWithValue($"{{{i}}}", args[i]);
                if (command.Contains($"@{i}")) sqlCommand.Parameters.AddWithValue($"@{i}", args[i]);
            }
            return sqlCommand;
        }

        public static MySqlCommand BuildCommand(MySqlConnection connection, string command, params object[] arguments)
        {
            MySqlCommand sqlCommand = new MySqlCommand(command, connection);
            for (int i = 0; i < arguments.Length; i++)
            {
                if (command.Contains($"{{{i}}}")) sqlCommand.Parameters.AddWithValue($"{{{i}}}", arguments[i]);
                if (command.Contains($"@{i}")) sqlCommand.Parameters.AddWithValue($"@{i}", arguments[i]);
            }
            return sqlCommand;
        }

        public static string BuildCommandContent(string command, int prefix, out PropertyList properties, params object[] args)
        {
            properties = new PropertyList();
            for (int i = args.Length - 1; i > 0; i--)
            {
                if (command.Contains($"{{{i}}}"))
                {
                    command = command.Replace($"{{{i}}}", $"@_{prefix}_{i}");
                    properties.Add($"@_{prefix}_{i}", args[i]);
                }
                if (command.Contains($"@{i}"))
                {
                    command = command.Replace($"@{i}", $"@_{prefix}_{i}");
                    properties.Add($"@_{prefix}_{i}", args[i]);
                }
            }
            return command;
        }

        public static MySqlCommand BuildInsertCommand<T>(T obj, string table, MySqlConnection connection = null)
        {
            var fields = GetClassFields<T>(x => !x.ShouldOmit(obj));
            string command = $"INSERT INTO `{table}` ({string.Join(", ", fields.CastEnumeration(x => x.SQLName))}) VALUES ({string.Join(", ", fields.CastEnumeration(x => $"@{x.FieldIndex}"))});";
            MySqlCommand sqlCommand = (connection != null ? new MySqlCommand(command, connection) : new MySqlCommand(command));
            foreach (var field in fields)
                sqlCommand.Parameters.AddWithValue($"@{field.FieldIndex}", field.GetValue(obj));
            return sqlCommand;
        }

        public static string BuildInsertCommandContent<T>(T obj, string table, int prefix, out PropertyList properties)
        {
            var fields = GetClassFields<T>(x => !x.ShouldOmit(obj));

            string command = $"INSERT INTO `{table}` ({string.Join(", ", fields.CastEnumeration(x => x.SQLName))}) VALUES ({string.Join(", ", fields.CastEnumeration(x => $"@{prefix}_{x.FieldIndex}"))});";

            properties = new PropertyList();
            foreach (var meta in fields)
                properties.Add($"@{prefix}_{meta.FieldIndex}", meta.GetValue(obj));
            return command;
        }

        public static MySqlCommand BuildInsertUpdateCommand<T>(T obj, string table, MySqlConnection Connection = null)
        {
            var fields = GetClassFields<T>(x => !x.ShouldOmit(obj));

            bool hasUpdatable = fields.Any(x => !x.Meta.OmitOnUpdate);

            string command = $"INSERT INTO `{table}` ({string.Join(", ", fields.CastEnumeration(x => x.SQLName))}) VALUES ({string.Join(", ", fields.CastEnumeration(x => $"@{x.FieldIndex}"))}) {(!hasUpdatable ? "" : $"ON DUPLICATE KEY UPDATE {string.Join(", ", fields.Where(x => !x.Meta.OmitOnUpdate).Select(x => $"`{x.Name}`=@{x.FieldIndex}"))};")};";
            MySqlCommand sqlCommand = (Connection != null ? new MySqlCommand(command, Connection) : new MySqlCommand(command));
            foreach (var meta in fields)
                sqlCommand.Parameters.AddWithValue($"@{meta.FieldIndex}", meta.GetValue(obj));
            return sqlCommand;
        }

        public static string BuildInsertUpdateCommandContent<T>(T obj, string table, int prefix, out PropertyList properties)
        {
            var fields = GetClassFields<T>(x => !x.ShouldOmit(obj));
            bool hasUpdatable = fields.Any(x => !x.Meta.OmitOnUpdate);

            string command = $"INSERT INTO `{table}` ({string.Join(", ", fields.CastEnumeration(x => x.SQLName))}) VALUES ({string.Join(", ", fields.CastEnumeration(x => $"@{x.FieldIndex}"))}) {(!hasUpdatable ? "" : $"ON DUPLICATE KEY UPDATE {string.Join(", ", fields.Where(x => !x.Meta.OmitOnUpdate).Select(x => $"`{x.Name}`=@{x.FieldIndex}"))};")};";
            properties = new PropertyList();
            foreach (var meta in fields)
                properties.Add($"@{prefix}_{meta.FieldIndex}", meta.GetValue(obj));
            return command;
        }

        public static MySqlCommand BuildUpdateCommand<T>(T obj, string table, MySqlConnection connection = null)
        {
            var fields = GetClassFields<T>(x => !x.ShouldOmit(obj) && !x.Meta.OmitOnUpdate);

            var primaryKey = fields.FirstOrDefault(x => x.Meta.IsPrimaryKey);

            if (primaryKey == null)
            {
                throw new NoPrimaryKeyException();
            }

            string Command = $"UPDATE `{table}` SET {string.Join(", ", fields.CastEnumeration(x => $"{x.SQLName}=@{x.FieldIndex}"))} WHERE {primaryKey.SQLName}=@KEY;";
            MySqlCommand sqlCommand = (connection != null ? new MySqlCommand(Command, connection) : new MySqlCommand(Command));
            sqlCommand.Parameters.AddWithValue("@KEY", primaryKey.GetValue(obj));
            foreach (var meta in fields)
                sqlCommand.Parameters.AddWithValue($"@{meta.FieldIndex}", meta.GetValue(obj));
            return sqlCommand;
        }

        public static string BuildUpdateCommandContent<T>(T obj, string table, int prefix, out PropertyList properties)
        {
            var fields = GetClassFields<T>(x => !x.ShouldOmit(obj) && !x.Meta.OmitOnUpdate);

            var primaryKey = fields.FirstOrDefault(x => x.Meta.IsPrimaryKey);

            if (primaryKey == null)
            {
                throw new NoPrimaryKeyException();
            }

            string command = $"UPDATE `{table}` SET {string.Join(", ", fields.CastEnumeration(x => $"{x.SQLName}=@{prefix}_{x.FieldIndex}"))} WHERE {primaryKey.SQLName}=@{prefix}KEY;";
            properties = new PropertyList();
            properties.Add($"@{prefix}KEY", primaryKey.GetValue(obj));

            foreach (var meta in fields)
                properties.Add($"@{prefix}_{meta.FieldIndex}", meta.GetValue(obj));

            return command;
        }

        public static MySqlCommand BuildDeleteCommand<T>(T obj, string table, MySqlConnection sqlConnection = null)
        {
            var fields = GetClassFields<T>(x => !x.ShouldOmit(obj));

            var primaryKey = fields.FirstOrDefault(x => x.Meta.IsPrimaryKey);

            if (primaryKey == null)
            {
                throw new NoPrimaryKeyException();
            }
            string command = $"DELETE FROM `{table}` WHERE {primaryKey.SQLName}=@KEY;";
            MySqlCommand sqlCommand = (sqlConnection != null ? new MySqlCommand(command, sqlConnection) : new MySqlCommand(command));
            sqlCommand.Parameters.AddWithValue("@KEY", primaryKey.GetValue(obj));
            return sqlCommand;
        }

        public static string BuildDeleteCommandContent<T>(T obj, string table, int prefix, out PropertyList properties)
        {
            var fields = GetClassFields<T>(x => !x.ShouldOmit(obj));

            var primaryKey = fields.FirstOrDefault(x => x.Meta.IsPrimaryKey);

            if (primaryKey == null)
            {
                throw new NoPrimaryKeyException();
            }
            string command = $"DELETE FROM `{table}` WHERE {primaryKey.SQLName}=@{prefix}_KEY;";

            properties = new PropertyList();
            properties.Add($"{prefix}_KEY", primaryKey.GetValue(obj));

            return command;
        }

        public MySqlCommand BuildCreateTableCommand<T>(string TableName, MySqlConnection Connection = null)
        {
            List<SQLBuildField> fields = new List<SQLBuildField>();
            string dbEngine = "InnoDB";
            if (Attribute.IsDefined(typeof(T), typeof(SQLDatabaseEngine)))
            {
                Attribute attrib = Attribute.GetCustomAttribute(typeof(T), typeof(SQLDatabaseEngine));
                dbEngine = ((SQLDatabaseEngine)attrib).DatabaseEngine;
            }

            foreach (var field in GetClassFields<T>())
            {
                if (fields.Where(x => string.Equals(x.Name, field.SQLName, StringComparison.InvariantCultureIgnoreCase)).Count() == 0)
                {
                    var bField = new SQLBuildField()
                    {
                        AutoIncrement = field.Meta.AutoIncrement,
                        Indexed = field.AttributeDefined<SQLIndex>(),
                        Name = field.SQLName,
                        Null = field.Meta.DBNull,
                        PrimaryKey = field.Meta.IsPrimaryKey,
                        Unique = field.Meta.Unique,
                        Type = m_TypeHelper.GetSQLTypeIndexed(field.FieldType)
                    };

                    var def = field.GetAttribute<SQLDefault>();
                    if (def != null)
                    {
                        bField.Default = def.DefaultValue;
                    }

                    if (bField.Type == null)
                    {
                        throw new SQLIncompatableTypeException(bField.Name);
                    }

                    fields.Add(bField);
                }
            }

            StringBuilder commandBuilder = new StringBuilder();
            commandBuilder.AppendLine($"CREATE TABLE `{MySqlHelper.EscapeString(TableName)}` (");
            List<string> bodyParams = new List<string>();
            List<object> defaults = new List<object>();
            foreach (SQLBuildField field in fields)
            {
                bodyParams.Add($"    `{field.Name}` {field.SQLRepresentation} {(field.Null ? "NULL" : "NOT NULL")}{(field.Default != null ? $" DEFAULT @DEF{defaults.Count}" : "")}{(field.AutoIncrement ? " AUTO_INCREMENT" : "")}");

                if (field.Default != null) defaults.Add(field.Default);
                if (field.PrimaryKey)
                    bodyParams.Add($"    PRIMARY KEY (`{field.Name}`)");
                if (field.Unique)
                    bodyParams.Add($"    UNIQUE `{field.Name}_Unique` (`{field.Name}`)");
                if (field.Indexed)
                    bodyParams.Add($"    INDEX `{field.Name}_INDEX` (`{field.Name}`)");
            }
            commandBuilder.AppendLine(string.Join(",\n", bodyParams));
            commandBuilder.Append($") ENGINE = {dbEngine};");
            MySqlCommand sqlCommand = (Connection != null ? new MySqlCommand(commandBuilder.ToString(), Connection) : new MySqlCommand(commandBuilder.ToString()));
            foreach (object def in defaults)
            {
                sqlCommand.Parameters.AddWithValue($"@DEF{defaults.IndexOf(def)}", def);
            }
            return sqlCommand;
        }
    }
}