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
        private SQLTypeHelper TypeHelper = new SQLTypeHelper();

        public static MySqlCommand BuildCommand(string Command, params object[] Arguments)
        {
            MySqlCommand Command_ = new MySqlCommand(Command);
            for (int i = 0; i < Arguments.Length; i++)
            {
                if (Command.Contains($"{{{i}}}")) Command_.Parameters.AddWithValue($"{{{i}}}", Arguments[i]);
                if (Command.Contains($"@{i}")) Command_.Parameters.AddWithValue($"@{i}", Arguments[i]);
            }
            return Command_;
        }

        public static MySqlCommand BuildCommand(MySqlConnection Connection, string Command, params object[] Arguments)
        {
            MySqlCommand Command_ = new MySqlCommand(Command, Connection);
            for (int i = 0; i < Arguments.Length; i++)
            {
                if (Command.Contains($"{{{i}}}")) Command_.Parameters.AddWithValue($"{{{i}}}", Arguments[i]);
                if (Command.Contains($"@{i}")) Command_.Parameters.AddWithValue($"@{i}", Arguments[i]);
            }
            return Command_;
        }

        public static string BuildCommandContent(string Command, int prefix, out PropertyList properties, params object[] Arguments)
        {
            properties = new PropertyList();
            for (int i = Arguments.Length - 1; i > 0; i--)
            {
                if (Command.Contains($"{{{i}}}"))
                {
                    Command = Command.Replace($"{{{i}}}", $"@_{prefix}_{i}");
                    properties.Add($"@_{prefix}_{i}", Arguments[i]);
                }
                if (Command.Contains($"@{i}"))
                {
                    Command = Command.Replace($"@{i}", $"@_{prefix}_{i}");
                    properties.Add($"@_{prefix}_{i}", Arguments[i]);
                }
            }
            return Command;
        }

        public static MySqlCommand BuildInsertCommand<T>(T Obj, string Table, MySqlConnection Connection = null)
        {
            List<SQLMetaField> Metas = new List<SQLMetaField>();
            foreach (FieldInfo Field in typeof(T).GetFields())
            {
                bool Include = true;
                string Name = Field.Name;
                foreach (Attribute Attrib in Attribute.GetCustomAttributes(Field))
                {
                    if (Attrib is SQLOmit || Attrib is SQLIgnore)
                    {
                        Include = false;
                        break;
                    }
                    else if (Attrib is SQLPropertyName)
                    {
                        Name = ((SQLPropertyName)Attrib).Name;
                    }
                }
                if (Include)
                {
                    if (Metas.Where(x => string.Equals(x.Name, Name, StringComparison.InvariantCultureIgnoreCase)).Count() != 0) continue;
                    Metas.Add(new SQLMetaField(Name, Metas.Count, Field));
                }
            }
            string Command = $"INSERT INTO `{Table}` ({string.Join(", ", Metas.CastEnumeration(x => x.Name))}) VALUES ({string.Join(", ", Metas.CastEnumeration(x => $"@{x.Index}"))});";
            MySqlCommand sqlCommand = (Connection != null ? new MySqlCommand(Command, Connection) : new MySqlCommand(Command));
            foreach (SQLMetaField Meta in Metas)
                sqlCommand.Parameters.AddWithValue($"@{Meta.Index}", Meta.Field.GetValue(Obj));
            return sqlCommand;
        }

        public static string BuildInsertCommandContent<T>(T Obj, string Table, int prefix, out PropertyList properties)
        {
            List<SQLMetaField> Metas = new List<SQLMetaField>();
            foreach (FieldInfo Field in typeof(T).GetFields())
            {
                bool Include = true;
                string Name = Field.Name;
                foreach (Attribute Attrib in Attribute.GetCustomAttributes(Field))
                {
                    if (Attrib is SQLOmit || Attrib is SQLIgnore)
                    {
                        Include = false;
                        break;
                    }
                    else if (Attrib is SQLPropertyName)
                    {
                        Name = ((SQLPropertyName)Attrib).Name;
                    }
                }
                if (Include)
                {
                    if (Metas.Where(x => string.Equals(x.Name, Name, StringComparison.InvariantCultureIgnoreCase)).Count() != 0) continue;
                    Metas.Add(new SQLMetaField(Name, Metas.Count, Field));
                }
            }
            string Command = $"INSERT INTO `{Table}` ({string.Join(", ", Metas.CastEnumeration(x => x.Name))}) VALUES ({string.Join(", ", Metas.CastEnumeration(x => $"@{prefix}_{x.Index}"))});";

            properties = new PropertyList();
            foreach (SQLMetaField Meta in Metas)
                properties.Add($"@{prefix}_{Meta.Index}", Meta.Field.GetValue(Obj));
            return Command;
        }

        public static MySqlCommand BuildInsertUpdateCommand<T>(T Obj, string Table, MySqlConnection Connection = null)
        {
            List<SQLMetaField> Metas = new List<SQLMetaField>();
            foreach (FieldInfo Field in typeof(T).GetFields())
            {
                bool Include = true;
                string Name = Field.Name;
                bool omitUpdate = false;
                foreach (Attribute Attrib in Attribute.GetCustomAttributes(Field))
                {
                    if (Attrib is SQLOmit || Attrib is SQLIgnore)
                    {
                        Include = false;
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
                if (Include)
                {
                    if (Metas.Where(x => string.Equals(x.Name, Name, StringComparison.InvariantCultureIgnoreCase)).Count() != 0) continue;
                    Metas.Add(new SQLMetaField(Name, Metas.Count, Field));
                }
            }
            string Command = $"INSERT INTO `{Table}` ({string.Join(", ", Metas.CastEnumeration(x => x.Name))}) VALUES ({string.Join(", ", Metas.CastEnumeration(x => $"@{x.Index}"))}) ON DUPLICATE KEY UPDATE {string.Join(", ", Metas.Where(x => !x.OmitUpdate).Select(x => $"`{x.Name}`=@{x.Index}"))};";
            MySqlCommand sqlCommand = (Connection != null ? new MySqlCommand(Command, Connection) : new MySqlCommand(Command));
            foreach (SQLMetaField Meta in Metas)
                sqlCommand.Parameters.AddWithValue($"@{Meta.Index}", Meta.Field.GetValue(Obj));
            return sqlCommand;
        }

        public static string BuildInsertUpdateCommandContent<T>(T Obj, string Table, int prefix, out PropertyList properties)
        {
            List<SQLMetaField> Metas = new List<SQLMetaField>();
            foreach (FieldInfo Field in typeof(T).GetFields())
            {
                bool Include = true;
                string Name = Field.Name;
                bool omitUpdate = false;
                foreach (Attribute Attrib in Attribute.GetCustomAttributes(Field))
                {
                    if (Attrib is SQLOmit || Attrib is SQLIgnore)
                    {
                        Include = false;
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
                if (Include)
                {
                    if (Metas.Where(x => string.Equals(x.Name, Name, StringComparison.InvariantCultureIgnoreCase)).Count() != 0) continue;
                    Metas.Add(new SQLMetaField(Name, Metas.Count, Field, omitUpdate));
                }
            }
            string Command = $"INSERT INTO `{Table}` ({string.Join(", ", Metas.CastEnumeration(x => x.Name))}) VALUES ({string.Join(", ", Metas.CastEnumeration(x => $"@{x.Index}"))}) ON DUPLICATE KEY UPDATE {string.Join(", ", Metas.Where(x => !x.OmitUpdate).Select(x => $"`{x.Name}`=@{x.Index}"))};";
            properties = new PropertyList();
            foreach (SQLMetaField Meta in Metas)
                properties.Add($"@{prefix}_{Meta.Index}", Meta.Field.GetValue(Obj));
            return Command;
        }

        public static MySqlCommand BuildUpdateCommand<T>(T Obj, string Table, MySqlConnection Connection = null)
        {
            List<SQLMetaField> Metas = new List<SQLMetaField>();
            SQLMetaField PrimaryKey = null;
            foreach (FieldInfo Field in typeof(T).GetFields())
            {
                bool Include = true;
                string Name = Field.Name;
                bool Primary = false;
                bool omitUpdate = false;
                bool isPrimary = Attribute.IsDefined(Field, typeof(SQLPrimaryKey));
                foreach (Attribute Attrib in Attribute.GetCustomAttributes(Field))
                {
                    if ((Attrib is SQLOmit && !isPrimary) || Attrib is SQLIgnore)
                    {
                        Include = false;
                        break;
                    }
                    else if (Attrib is SQLPropertyName)
                    {
                        Name = ((SQLPropertyName)Attrib).Name;
                    }
                    else if (Attrib is SQLPrimaryKey)
                    {
                        Primary = true;
                    } else if (Attrib is SQLOmitUpdate)
                    {
                        omitUpdate = true;
                    }
                }
                if (Include)
                {
                    SQLMetaField Meta = new SQLMetaField(Name, Metas.Count, Field, omitUpdate);
                    if (Primary)
                    {
                        PrimaryKey = Meta;
                        foreach (SQLMetaField SubMeta in Metas.Where(x => string.Equals(x.Name, Meta.Name, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            Metas.Remove(SubMeta);
                            break;
                        }
                    }
                    else
                    {
                        if (Metas.Where(x => string.Equals(x.Name, Name, StringComparison.InvariantCultureIgnoreCase)).Count() != 0) continue;
                        Metas.Add(new SQLMetaField(Name, Metas.Count, Field));
                    }
                }
            }
            Metas.RemoveAll(x => x.OmitUpdate);
            if (PrimaryKey == null) throw new NoPrimaryKeyException();
            string Command = $"UPDATE `{Table}` SET {string.Join(", ", Metas.CastEnumeration(x => $"{x.Name}=@{x.Index}"))} WHERE {PrimaryKey.Name}=@KEY;";
            MySqlCommand sqlCommand = (Connection != null ? new MySqlCommand(Command, Connection) : new MySqlCommand(Command));
            sqlCommand.Parameters.AddWithValue("@KEY", PrimaryKey.Field.GetValue(Obj));
            foreach (SQLMetaField Meta in Metas)
                sqlCommand.Parameters.AddWithValue($"@{Meta.Index}", Meta.Field.GetValue(Obj));
            return sqlCommand;
        }

        public static string BuildUpdateCommandContent<T>(T Obj, string Table, int prefix, out PropertyList properties)
        {
            List<SQLMetaField> Metas = new List<SQLMetaField>();
            SQLMetaField PrimaryKey = null;
            foreach (FieldInfo Field in typeof(T).GetFields())
            {
                bool Include = true;
                string Name = Field.Name;
                bool Primary = false;
                bool omitUpdate = false;
                bool isPrimary = Attribute.IsDefined(Field, typeof(SQLPrimaryKey));
                foreach (Attribute Attrib in Attribute.GetCustomAttributes(Field))
                {
                    if ((Attrib is SQLOmit && !isPrimary) || Attrib is SQLIgnore)
                    {
                        Include = false;
                        break;
                    }
                    else if (Attrib is SQLPropertyName)
                    {
                        Name = ((SQLPropertyName)Attrib).Name;
                    }
                    else if (Attrib is SQLPrimaryKey)
                    {
                        Primary = true;
                    }
                    else if (Attrib is SQLOmitUpdate)
                    {
                        omitUpdate = true;
                    }
                }
                if (Include)
                {
                    SQLMetaField Meta = new SQLMetaField(Name, Metas.Count, Field, omitUpdate);
                    if (Primary)
                    {
                        PrimaryKey = Meta;
                        foreach (SQLMetaField SubMeta in Metas.Where(x => string.Equals(x.Name, Meta.Name, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            Metas.Remove(SubMeta);
                            break;
                        }
                    }
                    else
                    {
                        if (Metas.Where(x => string.Equals(x.Name, Name, StringComparison.InvariantCultureIgnoreCase)).Count() != 0) continue;
                        Metas.Add(new SQLMetaField(Name, Metas.Count, Field));
                    }
                }
            }
            Metas.RemoveAll(x => x.OmitUpdate);
            if (PrimaryKey == null) throw new NoPrimaryKeyException();
            string Command = $"UPDATE `{Table}` SET {string.Join(", ", Metas.CastEnumeration(x => $"{x.Name}=@{x.Index}"))} WHERE {PrimaryKey.Name}=@KEY;";
            properties = new PropertyList();
            properties.Add($"@{prefix}KEY", PrimaryKey.Field.GetValue(Obj));

            foreach (SQLMetaField Meta in Metas)
                properties.Add($"@{prefix}_{Meta.Index}", Meta.Field.GetValue(Obj));

            return Command;
        }

        public static MySqlCommand BuildDeleteCommand<T>(T Obj, string Table, MySqlConnection Connection = null)
        {
            SQLMetaField PrimaryKey = null;
            foreach (FieldInfo Field in typeof(T).GetFields())
            {
                bool Primary = false;
                string Name = Field.Name;
                foreach (Attribute Attrib in Attribute.GetCustomAttributes(Field))
                {
                    if (Attrib is SQLIgnore)
                    {
                        break;
                    }
                    else if (Attrib is SQLPrimaryKey)
                    {
                        Primary = true;
                    }
                    else if (Attrib is SQLPropertyName)
                    {
                        Name = ((SQLPropertyName)Attrib).Name;
                    }
                }
                if (Primary)
                {
                    PrimaryKey = new SQLMetaField(Name, -1, Field);
                    break;
                }
            }
            if (PrimaryKey == null) throw new NoPrimaryKeyException();

            string Command = $"DELETE FROM `{Table}` WHERE {PrimaryKey.Name}=@KEY;";
            MySqlCommand sqlCommand = (Connection != null ? new MySqlCommand(Command, Connection) : new MySqlCommand(Command));
            sqlCommand.Parameters.AddWithValue("@KEY", PrimaryKey.Field.GetValue(Obj));
            return sqlCommand;
        }

        public static string BuildDeleteCommandContent<T>(T Obj, string Table, int prefix, out PropertyList properties)
        {
            SQLMetaField PrimaryKey = null;
            foreach (FieldInfo Field in typeof(T).GetFields())
            {
                bool Primary = false;
                string Name = Field.Name;
                foreach (Attribute Attrib in Attribute.GetCustomAttributes(Field))
                {
                    if (Attrib is SQLIgnore)
                    {
                        break;
                    }
                    else if (Attrib is SQLPrimaryKey)
                    {
                        Primary = true;
                    }
                    else if (Attrib is SQLPropertyName)
                    {
                        Name = ((SQLPropertyName)Attrib).Name;
                    }
                }
                if (Primary)
                {
                    PrimaryKey = new SQLMetaField(Name, -1, Field);
                    break;
                }
            }
            if (PrimaryKey == null) throw new NoPrimaryKeyException();

            string k1 = $"@{prefix}KEY";
            string Command = $"DELETE FROM `{Table}` WHERE {PrimaryKey.Name}={k1};";

            properties = new PropertyList();
            properties.Add(k1, PrimaryKey.Field.GetValue(Obj));

            return Command;
        }

        public MySqlCommand BuildCreateTableCommand<T>(string TableName, MySqlConnection Connection = null)
        {
            List<SQLBuildField> Fields = new List<SQLBuildField>();
            string DBEngine = "InnoDB";
            if (Attribute.IsDefined(typeof(T), typeof(SQLDatabaseEngine)))
            {
                Attribute attrib = Attribute.GetCustomAttribute(typeof(T), typeof(SQLDatabaseEngine));
                DBEngine = ((SQLDatabaseEngine)attrib).DatabaseEngine;
            }
            foreach (FieldInfo Field in typeof(T).GetFields())
            {
                SQLBuildField BuildField = new SQLBuildField()
                {
                    Name = Field.Name
                };
                bool Include = true;
                foreach (Attribute Attrib in Attribute.GetCustomAttributes(Field))
                {
                    switch (Attrib)
                    {
                        case SQLIgnore _:
                            Include = false;
                            break;

                        case SQLAutoIncrement _:
                            BuildField.AutoIncrement = true;
                            break;

                        case SQLDefault DefaultVal:
                            BuildField.Default = DefaultVal.DefaultValue;
                            break;

                        case SQLPrimaryKey _:
                            BuildField.PrimaryKey = true;
                            break;

                        case SQLPropertyName name:
                            BuildField.Name = name.Name;
                            break;

                        case SQLNull _:
                            BuildField.Null = true;
                            break;

                        case SQLUnique _:
                            BuildField.Unique = true;
                            break;

                        case SQLIndex _:
                            BuildField.Indexed = true;
                            break;

                        default:
                            if (Attrib.GetType().BaseType == typeof(SQLType))
                                BuildField.Type = ((SQLType)Attrib);
                            break;
                    }
                }
                if (Include && Fields.Where(x => string.Equals(x.Name, BuildField.Name, StringComparison.InvariantCultureIgnoreCase)).Count() == 0)
                {
                    if (BuildField.Type == null) BuildField.Type = TypeHelper.GetSQLTypeIndexed(Field.FieldType);
                    if (BuildField.Type == null) throw new SQLIncompatableTypeException(Field.Name);
                    Fields.Add(BuildField);
                }
            }

            StringBuilder CommandBuilder = new StringBuilder();
            CommandBuilder.AppendLine($"CREATE TABLE `{MySqlHelper.EscapeString(TableName)}` (");
            List<string> BodyParmas = new List<string>();
            List<object> Defaults = new List<object>();
            bool FL = true;
            foreach (SQLBuildField Field in Fields)
            {
                BodyParmas.Add($"    `{Field.Name}` {Field.Type.SQLRepresentation} {(Field.Null ? "NULL" : "NOT NULL")}{(Field.Default != null ? $" DEFAULT @DEF{Defaults.Count}" : "")}{(Field.AutoIncrement ? " AUTO_INCREMENT" : "")}");

                if (FL) FL = false;
                if (Field.Default != null) Defaults.Add(Field.Default);
                if (Field.PrimaryKey)
                    BodyParmas.Add($"    PRIMARY KEY (`{Field.Name}`)");
                if (Field.Unique)
                    BodyParmas.Add($"    UNIQUE `{Field.Name}_Unique` (`{Field.Name}`)");
                if (Field.Indexed)
                    BodyParmas.Add($"    INDEX `{Field.Name}_INDEX` (`{Field.Name}`)");
            }
            CommandBuilder.AppendLine(string.Join(",\n", BodyParmas));
            CommandBuilder.Append($") ENGINE = {DBEngine};");
            MySqlCommand sqlCommand = (Connection != null ? new MySqlCommand(CommandBuilder.ToString(), Connection) : new MySqlCommand(CommandBuilder.ToString()));
            foreach (object Def in Defaults)
            {
                sqlCommand.Parameters.AddWithValue($"@DEF{Defaults.IndexOf(Def)}", Def);
            }
            return sqlCommand;
        }
    }
}