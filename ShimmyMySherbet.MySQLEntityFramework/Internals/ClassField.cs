using ShimmyMySherbet.MySQL.EF.Models.Internals;
using System;
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
        }

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
            return m_info.GetValue(instance);
        }

        public override void SetValue(object instance, object obj)
        {
            m_info.SetValue(instance, obj);
        }
    }
}