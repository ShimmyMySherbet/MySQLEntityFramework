using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using ShimmyMySherbet.MySQL.EF.Models.Internals;

namespace ShimmyMySherbet.MySQL.EF.Models.TypeModel
{
    public class SQLTypeHelper
    {
        private Dictionary<Type, SQLType> NetTypeIndex = new Dictionary<Type, SQLType>();

        public SQLTypeHelper()
        {
            foreach (Type Type in Assembly.GetExecutingAssembly().GetTypes().Where(x => typeof(SQLType).IsAssignableFrom(x.BaseType)))
            {
                if (Attribute.IsDefined(Type, typeof(SQLNetType)))
                {
                    Type NetType = ((SQLNetType)Attribute.GetCustomAttribute(Type, typeof(SQLNetType))).Type;
                    if (NetType != null && !NetTypeIndex.ContainsKey(NetType))
                    {
                        SQLType SQLType = (SQLType)Activator.CreateInstance(Type);
                        NetTypeIndex.Add(NetType, SQLType);
                    }
                }
            }
        }

        public SQLType GetSQLTypeIndexed(Type T)
        {
            if (NetTypeIndex.ContainsKey(T))
            {
                return NetTypeIndex[T];
            }
            else
            {
                return null;
            }
        }

        public static SQLType GetSQLType(Type T)
        {
            foreach (Type Type in Assembly.GetExecutingAssembly().GetTypes().Where(x => typeof(SQLType).IsAssignableFrom(x.BaseType)))
            {
                foreach (Attribute attrib in Attribute.GetCustomAttributes(Type))
                {
                    if (attrib is SQLNetType)
                    {
                        SQLNetType NType = (SQLNetType)attrib;
                        if (NType.Type == null) break;
                        if (NType.Type == T)
                        {
                            return Activator.CreateInstance(Type) as SQLType;
                        }
                    }
                }
            }
            return null;
        }

        private static Type[] Int16Equivilants = { typeof(byte), typeof(sbyte) };
        private static Type[] UInt16Equivilants = { typeof(byte) };
        private static Type[] Int32Equivilants = { typeof(byte), typeof(sbyte), typeof(Int16), typeof(UInt16) };
        private static Type[] UInt32Equivilants = { typeof(byte), typeof(UInt16) };
        private static Type[] Int64Equivilants = { typeof(byte), typeof(sbyte), typeof(Int16), typeof(UInt16), typeof(UInt32), typeof(UInt32) };
        private static Type[] UInt64Equivilants = { typeof(byte), typeof(UInt16), typeof(UInt32) };

        public static bool CanCastEquivilant(Type BaseType, Type TargetType)
        {
            if (TargetType == typeof(Int16))
            {
                return Int16Equivilants.Contains(BaseType);
            } else if (TargetType == typeof(UInt16))
            {
                return UInt16Equivilants.Contains(BaseType);
            } else if (TargetType == typeof(Int32))
            {
                return Int32Equivilants.Contains(BaseType);
            } else if (TargetType == typeof(UInt32))
            {
                return UInt32Equivilants.Contains(BaseType);
            } else if (TargetType == typeof(Int64))
            {
                return Int64Equivilants.Contains(BaseType);
            } else if (TargetType == typeof(UInt64))
            {
                return UInt64Equivilants.Contains(BaseType);
            } else
            {
                return false;
            }
        }
    }
}