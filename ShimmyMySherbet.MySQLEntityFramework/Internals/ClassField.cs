﻿using System;
using System.Linq;
using System.Reflection;

namespace ShimmyMySherbet.MySQL.EF.Internals
{
    public class ClassField : ClassFieldBase
    {
        private FieldInfo m_info;

        public ClassField(FieldInfo info, int fieldIndex) : base(fieldIndex)
        {
            m_info = info;
            Reader = SQLConverter.GetTypeReader(ReadType);
        }
        public override TypeReader Reader { get; }

        public override Type FieldType => m_info.FieldType;

        public override string Name => m_info.Name;

        public override bool AttributeDefined<T>()
        {
            return GetAttribute<T>() != null;
        }

        public override T GetAttribute<T>()
        {
            return m_info.GetCustomAttribute<T>();
        }

        public override Attribute[] GetCustomAttributes()
        {
            return m_info.GetCustomAttributes().ToArray();
        }

        public override object GetValue(object instance)
        {
            var value = m_info.GetValue(instance);
            if (SerializeFormat != null)
            {
                return SerializationProvider.Serialize(SerializeFormat.Value, value, FieldType);
            }
            return value;
        }

        public override void SetValue(object instance, object obj)
        {
            if (SerializeFormat != null)
            {
                obj = SerializationProvider.Deserialize(SerializeFormat.Value, obj, FieldType);
            }

            m_info.SetValue(instance, obj);
        }
    }
}