using MySql.Data.MySqlClient;
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
            List<SQLMetaField> sqlMetas = new List<SQLMetaField>();
            foreach (FieldInfo field in typeof(T).GetFields())
            {
                bool include = true;
                string name = field.Name;
                foreach (Attribute attrib in Attribute.GetCustomAttributes(field))
                {
                    if (attrib is SQLOmit || attrib is SQLIgnore)
                    {
                        include = false;
                        break;
                    }
                    else if (attrib is SQLPropertyName)
                    {
                        name = ((SQLPropertyName)attrib).Name;
                    }
                }
                if (include)
                {
                    if (sqlMetas.Where(x => string.Equals(x.Name, name, StringComparison.InvariantCultureIgnoreCase)).Count() != 0) continue;
                    sqlMetas.Add(new SQLMetaField(name, sqlMetas.Count, field));
                }
            }
            string command = $"INSERT INTO `{table}` ({string.Join(", ", sqlMetas.CastEnumeration(x => x.Name))}) VALUES ({string.Join(", ", sqlMetas.CastEnumeration(x => $"@{x.Index}"))});";
            MySqlCommand sqlCommand = (connection != null ? new MySqlCommand(command, connection) : new MySqlCommand(command));
            foreach (SQLMetaField meta in sqlMetas)
                sqlCommand.Parameters.AddWithValue($"@{meta.Index}", meta.Field.GetValue(obj));
            return sqlCommand;
        }

        public static string BuildInsertCommandContent<T>(T obj, string table, int prefix, out PropertyList properties)
        {
            List<SQLMetaField> sqlMetas = new List<SQLMetaField>();
            foreach (FieldInfo field in typeof(T).GetFields())
            {
                bool include = true;
                string name = field.Name;
                foreach (Attribute attrib in Attribute.GetCustomAttributes(field))
                {
                    if (attrib is SQLOmit || attrib is SQLIgnore)
                    {
                        include = false;
                        break;
                    }
                    else if (attrib is SQLPropertyName)
                    {
                        name = ((SQLPropertyName)attrib).Name;
                    }
                }
                if (include)
                {
                    if (sqlMetas.Where(x => string.Equals(x.Name, name, StringComparison.InvariantCultureIgnoreCase)).Count() != 0) continue;
                    sqlMetas.Add(new SQLMetaField(name, sqlMetas.Count, field));
                }
            }
            string command = $"INSERT INTO `{table}` ({string.Join(", ", sqlMetas.CastEnumeration(x => x.Name))}) VALUES ({string.Join(", ", sqlMetas.CastEnumeration(x => $"@{prefix}_{x.Index}"))});";

            properties = new PropertyList();
            foreach (SQLMetaField meta in sqlMetas)
                properties.Add($"@{prefix}_{meta.Index}", meta.Field.GetValue(obj));
            return command;
        }

        public static MySqlCommand BuildInsertUpdateCommand<T>(T obj, string table, MySqlConnection Connection = null)
        {
            List<SQLMetaField> sqlMetas = new List<SQLMetaField>();
            foreach (FieldInfo field in typeof(T).GetFields())
            {
                bool include = true;
                string Name = field.Name;
                bool omitUpdate = false;
                foreach (Attribute attrib in Attribute.GetCustomAttributes(field))
                {
                    if (attrib is SQLOmit || attrib is SQLIgnore)
                    {
                        include = false;
                        break;
                    }
                    else if (attrib is SQLPropertyName)
                    {
                        Name = ((SQLPropertyName)attrib).Name;
                    }
                    else if (attrib is SQLOmitUpdate)
                    {
                        omitUpdate = true;
                    }
                }
                if (include)
                {
                    if (sqlMetas.Where(x => string.Equals(x.Name, Name, StringComparison.InvariantCultureIgnoreCase)).Count() != 0) continue;
                    sqlMetas.Add(new SQLMetaField(Name, sqlMetas.Count, field, omitUpdate));
                }
            }
            string command = $"INSERT INTO `{table}` ({string.Join(", ", sqlMetas.CastEnumeration(x => x.Name))}) VALUES ({string.Join(", ", sqlMetas.CastEnumeration(x => $"@{x.Index}"))}) ON DUPLICATE KEY UPDATE {string.Join(", ", sqlMetas.Where(x => !x.OmitUpdate).Select(x => $"`{x.Name}`=@{x.Index}"))};";
            MySqlCommand sqlCommand = (Connection != null ? new MySqlCommand(command, Connection) : new MySqlCommand(command));
            foreach (SQLMetaField meta in sqlMetas)
                sqlCommand.Parameters.AddWithValue($"@{meta.Index}", meta.Field.GetValue(obj));
            return sqlCommand;
        }

        public static string BuildInsertUpdateCommandContent<T>(T obj, string table, int prefix, out PropertyList properties)
        {
            List<SQLMetaField> sqlMetas = new List<SQLMetaField>();
            foreach (FieldInfo field in typeof(T).GetFields())
            {
                bool include = true;
                string name = field.Name;
                bool omitUpdate = false;
                foreach (Attribute attrib in Attribute.GetCustomAttributes(field))
                {
                    if (attrib is SQLOmit || attrib is SQLIgnore)
                    {
                        include = false;
                        break;
                    }
                    else if (attrib is SQLPropertyName)
                    {
                        name = ((SQLPropertyName)attrib).Name;
                    }
                    else if (attrib is SQLOmitUpdate)
                    {
                        omitUpdate = true;
                    }
                }
                if (include)
                {
                    if (sqlMetas.Where(x => string.Equals(x.Name, name, StringComparison.InvariantCultureIgnoreCase)).Count() != 0) continue;
                    sqlMetas.Add(new SQLMetaField(name, sqlMetas.Count, field, omitUpdate));
                }
            }
            string command = $"INSERT INTO `{table}` ({string.Join(", ", sqlMetas.CastEnumeration(x => x.Name))}) VALUES ({string.Join(", ", sqlMetas.CastEnumeration(x => $"@{x.Index}"))}) ON DUPLICATE KEY UPDATE {string.Join(", ", sqlMetas.Where(x => !x.OmitUpdate).Select(x => $"`{x.Name}`=@{x.Index}"))};";
            properties = new PropertyList();
            foreach (SQLMetaField meta in sqlMetas)
                properties.Add($"@{prefix}_{meta.Index}", meta.Field.GetValue(obj));
            return command;
        }

        public static MySqlCommand BuildUpdateCommand<T>(T obj, string table, MySqlConnection connection = null)
        {
            List<SQLMetaField> sqlMetas = new List<SQLMetaField>();
            SQLMetaField primaryKey = null;
            foreach (FieldInfo field in typeof(T).GetFields())
            {
                bool include = true;
                string Name = field.Name;
                bool omitUpdate = false;
                bool isPrimary = Attribute.IsDefined(field, typeof(SQLPrimaryKey));
                foreach (Attribute Attrib in Attribute.GetCustomAttributes(field))
                {
                    if ((Attrib is SQLOmit && !isPrimary) || Attrib is SQLIgnore)
                    {
                        include = false;
                        break;
                    }
                    else if (Attrib is SQLPropertyName)
                    {
                        Name = ((SQLPropertyName)Attrib).Name;
                    }
                    else if (Attrib is SQLOmitUpdate)
                    {
                        omitUpdate = true;
                    }
                }
                if (include)
                {
                    SQLMetaField meta = new SQLMetaField(Name, sqlMetas.Count, field, omitUpdate);
                    if (isPrimary)
                    {
                        primaryKey = meta;
                        foreach (SQLMetaField SubMeta in sqlMetas.Where(x => string.Equals(x.Name, meta.Name, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            sqlMetas.Remove(SubMeta);
                            break;
                        }
                    }
                    else
                    {
                        if (sqlMetas.Where(x => string.Equals(x.Name, Name, StringComparison.InvariantCultureIgnoreCase)).Count() != 0) continue;
                        sqlMetas.Add(new SQLMetaField(Name, sqlMetas.Count, field));
                    }
                }
            }
            sqlMetas.RemoveAll(x => x.OmitUpdate);
            if (primaryKey == null) throw new NoPrimaryKeyException();
            string Command = $"UPDATE `{table}` SET {string.Join(", ", sqlMetas.CastEnumeration(x => $"{x.Name}=@{x.Index}"))} WHERE {primaryKey.Name}=@KEY;";
            MySqlCommand sqlCommand = (connection != null ? new MySqlCommand(Command, connection) : new MySqlCommand(Command));
            sqlCommand.Parameters.AddWithValue("@KEY", primaryKey.Field.GetValue(obj));
            foreach (SQLMetaField meta in sqlMetas)
                sqlCommand.Parameters.AddWithValue($"@{meta.Index}", meta.Field.GetValue(obj));
            return sqlCommand;
        }

        public static string BuildUpdateCommandContent<T>(T obj, string table, int prefix, out PropertyList properties)
        {
            List<SQLMetaField> sqlMetas = new List<SQLMetaField>();
            SQLMetaField primaryKey = null;
            foreach (FieldInfo field in typeof(T).GetFields())
            {
                bool include = true;
                string name = field.Name;
                bool omitUpdate = false;
                bool isPrimary = Attribute.IsDefined(field, typeof(SQLPrimaryKey));
                foreach (Attribute attrib in Attribute.GetCustomAttributes(field))
                {
                    if ((attrib is SQLOmit && !isPrimary) || attrib is SQLIgnore)
                    {
                        include = false;
                        break;
                    }
                    else if (attrib is SQLPropertyName)
                    {
                        name = ((SQLPropertyName)attrib).Name;
                    }
                    else if (attrib is SQLOmitUpdate)
                    {
                        omitUpdate = true;
                    }
                }
                if (include)
                {
                    SQLMetaField meta = new SQLMetaField(name, sqlMetas.Count, field, omitUpdate);
                    if (isPrimary)
                    {
                        primaryKey = meta;
                        foreach (SQLMetaField SubMeta in sqlMetas.Where(x => string.Equals(x.Name, meta.Name, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            sqlMetas.Remove(SubMeta);
                            break;
                        }
                    }
                    else
                    {
                        if (sqlMetas.Where(x => string.Equals(x.Name, name, StringComparison.InvariantCultureIgnoreCase)).Count() != 0) continue;
                        sqlMetas.Add(new SQLMetaField(name, sqlMetas.Count, field));
                    }
                }
            }
            sqlMetas.RemoveAll(x => x.OmitUpdate);
            if (primaryKey == null) throw new NoPrimaryKeyException();
            string command = $"UPDATE `{table}` SET {string.Join(", ", sqlMetas.CastEnumeration(x => $"{x.Name}=@{x.Index}"))} WHERE {primaryKey.Name}=@KEY;";
            properties = new PropertyList();
            properties.Add($"@{prefix}KEY", primaryKey.Field.GetValue(obj));

            foreach (SQLMetaField meta in sqlMetas)
                properties.Add($"@{prefix}_{meta.Index}", meta.Field.GetValue(obj));

            return command;
        }

        public static MySqlCommand BuildDeleteCommand<T>(T obj, string table, MySqlConnection sqlConnection = null)
        {
            SQLMetaField primaryKey = null;
            foreach (FieldInfo field in typeof(T).GetFields())
            {
                bool isPrimary = false;
                string name = field.Name;
                foreach (Attribute attrib in Attribute.GetCustomAttributes(field))
                {
                    if (attrib is SQLIgnore)
                    {
                        break;
                    }
                    else if (attrib is SQLPrimaryKey)
                    {
                        isPrimary = true;
                    }
                    else if (attrib is SQLPropertyName)
                    {
                        name = ((SQLPropertyName)attrib).Name;
                    }
                }
                if (isPrimary)
                {
                    primaryKey = new SQLMetaField(name, -1, field);
                    break;
                }
            }
            if (primaryKey == null) throw new NoPrimaryKeyException();

            string command = $"DELETE FROM `{table}` WHERE {primaryKey.Name}=@KEY;";
            MySqlCommand sqlCommand = (sqlConnection != null ? new MySqlCommand(command, sqlConnection) : new MySqlCommand(command));
            sqlCommand.Parameters.AddWithValue("@KEY", primaryKey.Field.GetValue(obj));
            return sqlCommand;
        }

        public static string BuildDeleteCommandContent<T>(T obj, string table, int prefix, out PropertyList properties)
        {
            SQLMetaField primaryKey = null;
            foreach (FieldInfo field in typeof(T).GetFields())
            {
                bool isPrimary = false;
                string name = field.Name;
                foreach (Attribute attrib in Attribute.GetCustomAttributes(field))
                {
                    if (attrib is SQLIgnore)
                    {
                        break;
                    }
                    else if (attrib is SQLPrimaryKey)
                    {
                        isPrimary = true;
                    }
                    else if (attrib is SQLPropertyName)
                    {
                        name = ((SQLPropertyName)attrib).Name;
                    }
                }
                if (isPrimary)
                {
                    primaryKey = new SQLMetaField(name, -1, field);
                    break;
                }
            }
            if (primaryKey == null) throw new NoPrimaryKeyException();

            string k1 = $"@{prefix}KEY";
            string command = $"DELETE FROM `{table}` WHERE {primaryKey.Name}={k1};";

            properties = new PropertyList();
            properties.Add(k1, primaryKey.Field.GetValue(obj));

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
            foreach (FieldInfo field in typeof(T).GetFields())
            {
                SQLBuildField buildField = new SQLBuildField()
                {
                    Name = field.Name
                };
                bool include = true;
                foreach (Attribute attrib in Attribute.GetCustomAttributes(field))
                {
                    switch (attrib)
                    {
                        case SQLIgnore _:
                            include = false;
                            break;

                        case SQLAutoIncrement _:
                            buildField.AutoIncrement = true;
                            break;

                        case SQLDefault DefaultVal:
                            buildField.Default = DefaultVal.DefaultValue;
                            break;

                        case SQLPrimaryKey _:
                            buildField.PrimaryKey = true;
                            break;

                        case SQLPropertyName name:
                            buildField.Name = name.Name;
                            break;

                        case SQLNull _:
                            buildField.Null = true;
                            break;

                        case SQLUnique _:
                            buildField.Unique = true;
                            break;

                        case SQLIndex _:
                            buildField.Indexed = true;
                            break;

                        default:
                            if (attrib.GetType().BaseType == typeof(SQLType))
                                buildField.Type = ((SQLType)attrib);
                            break;
                    }
                }
                if (include && fields.Where(x => string.Equals(x.Name, buildField.Name, StringComparison.InvariantCultureIgnoreCase)).Count() == 0)
                {
                    if (buildField.Type == null) buildField.Type = m_TypeHelper.GetSQLTypeIndexed(field.FieldType);
                    if (buildField.Type == null) throw new SQLIncompatableTypeException(field.Name);
                    fields.Add(buildField);
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