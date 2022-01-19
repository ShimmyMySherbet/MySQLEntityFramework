using ShimmyMySherbet.MySQL.EF.Models;
using ShimmyMySherbet.MySQL.EF.Models.Internals;
using System;

namespace ShimmyMySherbet.MySQL.EF.Internals
{
    public interface IClassField
    {
        string Name { get; }
        string SQLName { get; }
        Type FieldType { get; }

        Type ReadType { get; }

        SQLType OverrideType { get; }
        int FieldIndex { get; }
        SQLMetaField Meta { get; }

        Attribute[] GetCustomAttributes();

        bool AttributeDefined<T>() where T : Attribute;

        T GetAttribute<T>() where T : Attribute;

        void SetValue(object instance, object obj);

        object GetValue(object instance);

        bool ShouldOmit(object instance);
    }
}