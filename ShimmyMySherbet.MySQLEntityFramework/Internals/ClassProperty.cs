using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Internals
{
    public class ClassProperty : ClassFieldBase
    {
        private PropertyInfo m_info;

        public ClassProperty(PropertyInfo info, int index) : base(index)
        {
            m_info = info;
        }

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
            return m_info.GetValue(instance);

        }

        public override void SetValue(object instance, object obj)
        {
            if (m_info.CanWrite)
            {
                m_info.SetValue(instance, obj);
            }
        }
    }
}
