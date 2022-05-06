using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using ShimmyMySherbet.MySQL.EF.Models.Exceptions;
using ShimmyMySherbet.MySQL.EF.Models.Internals;
using ShimmyMySherbet.MySQL.EF.Models.TypeModel;

namespace ShimmyMySherbet.MySQL.EF.Internals
{
    public class SQLConverter
    {
        public SQLTypeHelper TypeHelper;

        public List<T> ReadModelsFromReader<T>(DbDataReader Reader, int limit = -1)
        {
            if (typeof(T).IsPrimitive || typeof(T) == typeof(string) || TypeHelper != null && TypeHelper.GetSQLTypeIndexed(typeof(T)) != null)
            {
                return ReadSQLBaseTypesUnsafe<T>(Reader, limit);
            }
            else
            {
                return ReadClasses<T>(Reader, limit);
            }
        }

        public async Task<List<T>> ReadModelsFromReaderAsync<T>(DbDataReader Reader, int limit = -1)
        {
            if (typeof(T).IsPrimitive || typeof(T) == typeof(string) || TypeHelper != null && TypeHelper.GetSQLTypeIndexed(typeof(T)) != null)
            {
                return await ReadSQLBaseTypesUnsafeAsync<T>(Reader, limit);
            }
            else
            {
                return await ReadClassesAsync<T>(Reader, limit);
            }
        }

        public List<T> ReadSQLBaseTypes<T>(MySqlDataReader Reader, SQLTypeHelper Helper, int limit = -1)
        {
            SQLType Type = Helper.GetSQLTypeIndexed(typeof(T));
            if (Type == null) throw new SQLIncompatableTypeException();
            return ReadSQLBaseTypesUnsafe<T>(Reader, limit);
        }

        public List<T> ReadSQLBaseTypesUnsafe<T>(IDataReader Reader, int limit = -1)
        {
            List<T> Entries = new List<T>();
            int CompatableColumn = -1;
            bool IsNumeric = SQLTypeHelper.NumericType(typeof(T));
            for (int i = 0; i < Reader.FieldCount; i++)
            {
                Type SQLType = Reader.GetFieldType(i);
                if (typeof(T).IsAssignableFrom(SQLType))
                {
                    CompatableColumn = i;
                }
                else if (SQLTypeHelper.CanCastEquivilant(SQLType, typeof(T)) || (IsNumeric && SQLTypeHelper.NumericType(SQLType)))
                {
                    CompatableColumn = i;
                }
            }
            if (CompatableColumn == -1)
            {
                if (Reader.FieldCount == 0)
                {
                    CompatableColumn = 0;
                }
                else
                {
                    var names = new List<string>();
                    for (int i = 0; i < Reader.FieldCount; i++)
                        names.Add(Reader.GetName(i));
                    throw new SQLIncompatableTypeException(string.Join(", ", names), typeof(T).Name);
                }
            }

            bool checkLimit = limit != -1;
            int count = 0;

            while (Reader.Read())
            {
                count++;

                try
                {
                    var ob = TryRead(Reader, typeof(T), CompatableColumn, out var r);

                    if (r)
                    {
                        Entries.Add((T)ob);
                    }
                    else
                    {
                        throw new SQLInvalidCastException(Reader.GetName(CompatableColumn), Reader.GetDataTypeName(CompatableColumn), typeof(T).Name);
                    }
                }
                catch (InvalidCastException)
                {
                    throw new SQLInvalidCastException(Reader.GetName(CompatableColumn), Reader.GetDataTypeName(CompatableColumn), typeof(T).Name);
                }
                if (checkLimit && count >= limit) break;
            }
            return Entries;
        }

        public async Task<List<T>> ReadSQLBaseTypesUnsafeAsync<T>(DbDataReader Reader, int limit = -1)
        {
            List<T> Entries = new List<T>();
            int CompatableColumn = -1;
            bool IsNumeric = SQLTypeHelper.NumericType(typeof(T));
            for (int i = 0; i < Reader.FieldCount; i++)
            {
                Type SQLType = Reader.GetFieldType(i);
                if (typeof(T).IsAssignableFrom(SQLType))
                {
                    CompatableColumn = i;
                }
                else if (SQLTypeHelper.CanCastEquivilant(SQLType, typeof(T)) || (IsNumeric && SQLTypeHelper.NumericType(SQLType)))
                {
                    CompatableColumn = i;
                }
            }
            if (CompatableColumn == -1)
            {
                if (Reader.FieldCount == 0)
                {
                    CompatableColumn = 0;
                }
                else
                {
                    var names = new List<string>();
                    for (int i = 0; i < Reader.FieldCount; i++)
                        names.Add(Reader.GetName(i));
                    throw new SQLIncompatableTypeException(string.Join(", ", names), typeof(T).Name);
                }
            }

            bool checkLimit = limit != -1;
            int count = 0;

            while (await Reader.ReadAsync())
            {
                count++;

                try
                {
                    var ob = TryRead(Reader, typeof(T), CompatableColumn, out var read);

                    if (read)
                    {
                        Entries.Add((T)ob);
                    }
                    else
                    {
                        throw new SQLInvalidCastException(Reader.GetName(CompatableColumn), Reader.GetDataTypeName(CompatableColumn), typeof(T).Name);
                    }
                }
                catch (InvalidCastException)
                {
                    throw new SQLInvalidCastException(Reader.GetName(CompatableColumn), Reader.GetDataTypeName(CompatableColumn), typeof(T).Name);
                }
                if (checkLimit && count >= limit) break;
            }
            return Entries;
        }

        public List<T> ReadClasses<T>(IDataReader Reader, int limit = -1)
        {
            Dictionary<string, IClassField> BaseFields = new Dictionary<string, IClassField>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var field in EntityCommandBuilder.GetClassFields<T>())
            {
                if (!BaseFields.ContainsKey(field.SQLName))
                {
                    BaseFields[field.SQLName] = field;
                }
            }
            Dictionary<string, SQLFieldReferance> SQLFields = new Dictionary<string, SQLFieldReferance>(StringComparer.InvariantCultureIgnoreCase);
            for (int i = 0; i < Reader.FieldCount; i++)
            {
                Type SQLType = Reader.GetFieldType(i);
                string Name = Reader.GetName(i);
                SQLFields.Add(Name, new SQLFieldReferance(i, Name, SQLType));
            }
            List<T> Result = new List<T>();
            bool checkLimit = limit != -1;
            int count = 0;

            while (Reader.Read())
            {
                count++;
                T NewObject = Activator.CreateInstance<T>();
                foreach (SQLFieldReferance Referance in SQLFields.Values)
                {
                    if (BaseFields.ContainsKey(Referance.Name))
                    {
                        var field = BaseFields[Referance.Name];

                        if (Reader.IsDBNull(Referance.Index))
                        {
                            field.SetValue(NewObject, GetDefault(field.FieldType));
                        }
                        else
                        {
                            var obj = TryRead(Reader, field.ReadType, Referance.Index, out var read);

                            if (read)
                            {
                                field.SetValue(NewObject, obj);
                            }
                            else
                            {
                                throw new SQLInvalidCastException(Referance.Name, Referance.Type.Name, field.FieldType.Name);
                            }
                        }
                    }
                }
                Result.Add(NewObject);
                if (checkLimit && count >= limit) break;
            }
            return Result;
        }

        private static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        /// <summary>
        /// Attempts to read a type from the reader using it's implemented conversion methods.
        /// If the requested type isn't implemented for the reader, an IConversion cast is attempted instead
        /// </summary>
        /// <param name="read">True if the item was supported and read</param>
        /// <returns>Instance or null</returns>
        private static object TryRead(IDataReader reader, Type type, int index, out bool read)
        {
            if (type == typeof(string))
            {
                read = true;
                return reader.GetString(index);
            }
            else if (type == typeof(bool))
            {
                read = true;
                return reader.GetBoolean(index);
            }
            else if (type == typeof(byte))
            {
                read = true;
                return reader.GetByte(index);
            }
            else if (type == typeof(char))
            {
                read = true;
                return reader.GetChar(index);
            }
            else if (type == typeof(DateTime))
            {
                read = true;
                return reader.GetDateTime(index);
            }
            else if (type == typeof(decimal))
            {
                read = true;
                return reader.GetDecimal(index);
            }
            else if (type == typeof(double))
            {
                read = true;
                return reader.GetDouble(index);
            }
            else if (type == typeof(float))
            {
                read = true;
                return reader.GetFloat(index);
            }
            else if (type == typeof(Guid))
            {
                read = true;
                return reader.GetGuid(index);
            }
            else if (type == typeof(short))
            {
                read = true;
                return reader.GetInt16(index);
            }
            else if (type == typeof(int))
            {
                read = true;
                return reader.GetInt32(index);
            }
            else if (type == typeof(long))
            {
                read = true;
                return reader.GetInt64(index);
            }

            var obj = reader.GetValue(index);

            try
            {
                var ob = Convert.ChangeType(obj, type);
                read = true;
                return ob;
            }
            catch (InvalidCastException)
            {
                read = false;
            }

            return null;
        }

        public async Task<List<T>> ReadClassesAsync<T>(DbDataReader Reader, int limit = -1)
        {
            var BaseFields = new Dictionary<string, IClassField>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var field in EntityCommandBuilder.GetClassFields<T>())
            {
                if (!BaseFields.ContainsKey(field.SQLName))
                {
                    BaseFields[field.SQLName] = field;
                }
            }

            Dictionary<string, SQLFieldReferance> SQLFields = new Dictionary<string, SQLFieldReferance>(StringComparer.InvariantCultureIgnoreCase);
            for (int i = 0; i < Reader.FieldCount; i++)
            {
                Type SQLType = Reader.GetFieldType(i);
                string Name = Reader.GetName(i);
                SQLFields.Add(Name, new SQLFieldReferance(i, Name, SQLType));
            }
            bool checkLimit = limit != -1;
            int count = 0;

            List<T> Result = new List<T>();
            while (await Reader.ReadAsync())
            {
                count++;
                T NewObject = Activator.CreateInstance<T>();
                foreach (SQLFieldReferance Referance in SQLFields.Values)
                {
                    if (BaseFields.ContainsKey(Referance.Name))
                    {
                        var field = BaseFields[Referance.Name];
                        var obj = Reader.GetValue(Referance.Index);
                        if (await Reader.IsDBNullAsync(Referance.Index))
                        {
                            field.SetValue(NewObject, GetDefault(field.FieldType));
                        }
                        else
                        {
                            var ob = TryRead(Reader, field.ReadType, Referance.Index, out var read);
                            if (read)
                            {
                                field.SetValue(NewObject, ob);
                            }
                            else
                            {
                                throw new SQLInvalidCastException(Referance.Name, Referance.Type.Name, field.FieldType.Name);
                            }
                        }
                    }
                }
                Result.Add(NewObject);
                if (checkLimit && count >= limit) break;
            }
            return Result;
        }
    }
}