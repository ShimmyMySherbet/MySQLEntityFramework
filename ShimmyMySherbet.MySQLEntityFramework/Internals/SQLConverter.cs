using System;
using System.Collections.Generic;
using System.Reflection;
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

        public List<T> ReadModelsFromReader<T>(MySqlDataReader Reader)
        {
            if (TypeHelper != null && TypeHelper.GetSQLTypeIndexed(typeof(T)) != null)
            {
                return ReadSQLBaseTypesUnsafe<T>(Reader);
            }
            else
            {
                return ReadClasses<T>(Reader);
            }
        }

        public List<T> ReadSQLBaseTypes<T>(MySqlDataReader Reader, SQLTypeHelper Helper)
        {
            SQLType Type = Helper.GetSQLTypeIndexed(typeof(T));
            if (Type == null) throw new SQLIncompatableTypeException();
            return ReadSQLBaseTypesUnsafe<T>(Reader);
        }

        public List<T> ReadSQLBaseTypesUnsafe<T>(MySqlDataReader Reader)
        {
            List<T> Entries = new List<T>();
            int CompatableColumn = -1;
            bool CastRequired = false;
            for (int i = 0; i < Reader.FieldCount; i++)
            {
                Type SQLType = Reader.GetFieldType(i);
                if (typeof(T).IsAssignableFrom(SQLType))
                {
                    CompatableColumn = i;
                }
                else if (SQLTypeHelper.CanCastEquivilant(SQLType, typeof(T)))
                {
                    CompatableColumn = i;
                    CastRequired = true;
                }
            }
            if (CompatableColumn == -1) throw new SQLIncompatableTypeException();
            while (Reader.Read())
            {
                if (CastRequired)
                {
                    Entries.Add((T)Convert.ChangeType(Reader.GetValue(CompatableColumn), typeof(T)));
                }
                else
                {
                    Entries.Add((T)Reader.GetValue(CompatableColumn));
                }
            }
            return Entries;
        }

        public List<T> ReadClasses<T>(MySqlDataReader Reader)
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
            while (Reader.Read())
            {
                long startt = DateTime.Now.Ticks;
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
            }
            return Result;
        }
    }
}