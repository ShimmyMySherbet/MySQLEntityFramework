using System;
using System.Linq;
using System.Reflection;

namespace ShimmyMySherbet.MySQL.EF.Internals
{
    public class ClassProperty : ClassFieldBase
    {
        private PropertyInfo m_info;

        public ClassProperty(PropertyInfo info, int index) : base(index)
        {
            m_info = info;
            Reader = SQLConverter.GetTypeReader(ReadType);
        }
        public override TypeReader Reader { get; }

        public override Type FieldType => m_info.PropertyType;

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
                value = SerializationProvider.Serialize(SerializeFormat.Value, value, FieldType);
            }
            return value;
        }

        public override void SetValue(object instance, object obj)
        {
            if (m_info.CanWrite)
            {
                if (SerializeFormat != null)
                {
                    obj = SerializationProvider.Deserialize(SerializeFormat.Value, obj, FieldType);
                }
                m_info.SetValue(instance, obj);
            }
        }
    }
}