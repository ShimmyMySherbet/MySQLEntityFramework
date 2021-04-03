using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using MySql.Data.MySqlClient;
using ShimmyMySherbet.MySQL.EF.Models;
using ShimmyMySherbet.MySQL.EF.Models.Exceptions;
using ShimmyMySherbet.MySQL.EF.Models.Internals;
using ShimmyMySherbet.MySQL.EF.Models.TypeModel;

namespace ShimmyMySherbet.MySQL.EF.Internals
{
    public class SQLConverter
    {
        public SQLTypeHelper TypeHelper;

        public List<T> ReadModelsFromReader<T>(IDataReader Reader, int limit = -1)
        {
            if (TypeHelper != null && TypeHelper.GetSQLTypeIndexed(typeof(T)) != null)
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
            if (TypeHelper != null && TypeHelper.GetSQLTypeIndexed(typeof(T)) != null)
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
            bool CastRequired = false;
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
                    CastRequired = true;
                }
            }
            if (CompatableColumn == -1) throw new SQLIncompatableTypeException();
            bool checkLimit = limit != -1;
            int count = 0;

            while (Reader.Read())
            {
                count++;
                if (CastRequired)
                {
                    Entries.Add((T)Convert.ChangeType(Reader.GetValue(CompatableColumn), typeof(T)));
                }
                else
                {
                    Entries.Add((T)Reader.GetValue(CompatableColumn));
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
            if (CompatableColumn == -1) throw new SQLIncompatableTypeException();

            bool checkLimit = limit != -1;
            int count = 0;

            while (await Reader.ReadAsync())
            {
                count++;
                Entries.Add(await Reader.GetFieldValueAsync<T>(CompatableColumn));
                if (checkLimit && count >= limit) break;
            }
            return Entries;
        }

        public List<T> ReadClasses<T>(IDataReader Reader, int limit = -1)
        {
            Type TT = typeof(T);
            Dictionary<string, FieldInfo> BaseFields = new Dictionary<string, FieldInfo>(StringComparer.InvariantCultureIgnoreCase);
            foreach (FieldInfo Field in typeof(T).GetFields())
            {
                bool IncludeField = true;
                string Name = Field.Name;
                foreach (Attribute attribute in Attribute.GetCustomAttributes(Field))
                {
                    if (attribute is SQLIgnore)
                    {
                        IncludeField = false;
                        break;
                    }
                    else if (attribute is SQLPropertyName)
                    {
                        Name = ((SQLPropertyName)attribute).Name;
                    }
                }
                if (IncludeField && !BaseFields.ContainsKey(Name))
                    BaseFields.Add(Name, Field);
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
                        FieldInfo field = BaseFields[Referance.Name];
                        field.SetValue(NewObject, Reader.GetValue(Referance.Index));
                    }
                }
                Result.Add(NewObject);
                if (checkLimit && count >= limit) break;
            }
            return Result;
        }



        public async Task<List<T>> ReadClassesAsync<T>(DbDataReader Reader, int limit = -1)
        {
            Type TT = typeof(T);
            Dictionary<string, FieldInfo> BaseFields = new Dictionary<string, FieldInfo>(StringComparer.InvariantCultureIgnoreCase);
            foreach (FieldInfo Field in typeof(T).GetFields())
            {
                bool IncludeField = true;
                string Name = Field.Name;
                foreach (Attribute attribute in Attribute.GetCustomAttributes(Field))
                {
                    if (attribute is SQLIgnore)
                    {
                        IncludeField = false;
                        break;
                    }
                    else if (attribute is SQLPropertyName)
                    {
                        Name = ((SQLPropertyName)attribute).Name;
                    }
                }
                if (IncludeField && !BaseFields.ContainsKey(Name))
                    BaseFields.Add(Name, Field);
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
                        FieldInfo field = BaseFields[Referance.Name];
                        field.SetValue(NewObject, Reader.GetValue(Referance.Index));
                    }
                }
                Result.Add(NewObject);
                if (checkLimit && count >= limit) break;
            }
            return Result;
        }
    }
}