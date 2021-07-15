using ShimmyMySherbet.MySQL.EF.Models.Internals;
using System;

namespace ShimmyMySherbet.MySQL.EF.Internals
{
    public abstract class ClassFieldBase : IClassField
    {
        public abstract string Name { get;  }

        public string SQLName
        {
            get
            {
                var ovr = Meta?.Name;

                if (ovr != null)
                {
                    return ovr;
                }

                return Name;
            }
        }

        public abstract Type FieldType { get; }

        public ClassFieldBase(int index)
        {
            FieldIndex = index;
        }

        public int FieldIndex
        {
            get;
            protected set;
        }

        private SQLMetaField m_meta;

        public SQLMetaField Meta
        {
            get
            {
                if (m_meta == null)
                {
                    m_meta = SQlMetaBuilder.GetMeta(this, FieldIndex);
                }
                return m_meta;
            }
        }

        private SQLType m_OverrideType = null;
        private bool m_HasOverride = false;

        public SQLType OverrideType
        {
            get
            {
                if (!m_HasOverride)
                {
                    m_OverrideType = GetAttribute<SQLType>();
                }
                return m_OverrideType;
            }
        }

        public abstract bool AttributeDefined<T>() where T : Attribute;

        public abstract T GetAttribute<T>() where T : Attribute;

        public abstract Attribute[] GetCustomAttributes();

        public abstract object GetValue(object instance);

        public abstract void SetValue(object instance, object obj);

        public bool ShouldOmit(object instance)
        {
            if (Meta.Omit) return true;
            if (Meta.OmitOnNull && GetValue(instance) == null)
            {
                return true;
            }
            return false;
        }
    }
}