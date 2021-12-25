using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using ShimmyMySherbet.MySQL.EF.Models;

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

        public static IClassField DetermineIDField(this List<IClassField> fields)
        {
            var matches = new List<IClassField>();
            foreach (var f in fields)
            {
                if (f.AttributeDefined<SQLPrimaryKey>() && f.AttributeDefined<SQLAutoIncrement>() && f.FieldType.IsNumeric())
                {
                    matches.Add(f);
                }
            }

            if (matches.Count == 1)
            {
                return matches[0];
            }

            return null; // No matching field, or ambiguity in multiple numeric composite auto-increment keys
        }

        private static readonly HashSet<Type> m_NumericMap = new HashSet<Type>(new[] { typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) });

        public static bool IsNumeric(this Type t) => t.IsPrimitive && m_NumericMap.Contains(t);

        public static bool IsZero(this object obj)
        {
            if (obj is sbyte sb)
                return sb == 0;
            if (obj is byte b)
                return b == 0;
            if (obj is short s)
                return s == 0;
            if (obj is ushort us)
                return us == 0;
            if (obj is int i)
                return i == 0;
            if (obj is uint ui)
                return ui == 0;
            if (obj is long l)
                return l == 0;
            if (obj is ulong ul)
                return ul == 0;
            if (obj is float f)
                return f == 0;
            if (obj is double d)
                return d == 0;
            if (obj is decimal dc)
                return dc == 0;
            return false;
        }

        public static bool TryConvertNumeric(this long inp, Type target, out object inst)
        {
            if (target == typeof(sbyte))
            {
                inst = (sbyte)inp;
                return true;
            }
            if (target == typeof(byte))
            {
                inst = (byte)inp;
                return true;
            }
            if (target == typeof(byte))
            {
                inst = (byte)inp;
                return true;
            }
            if (target == typeof(short))
            {
                inst = (short)inp;
                return true;
            }
            if (target == typeof(ushort))
            {
                inst = (ushort)inp;
                return true;
            }
            if (target == typeof(int))
            {
                inst = (int)inp;
                return true;
            }
            if (target == typeof(uint))
            {
                inst = (uint)inp;
                return true;
            }
            if (target == typeof(long))
            {
                inst = inp;
                return true;
            }
            if (target == typeof(ulong))
            {
                inst = (ulong)inp;
                return true;
            }
            if (target == typeof(float))
            {
                inst = (float)inp;
                return true;
            }
            if (target == typeof(double))
            {
                inst = (double)inp;
                return true;
            }
            inst = null;
            return false;
        }
    }
}