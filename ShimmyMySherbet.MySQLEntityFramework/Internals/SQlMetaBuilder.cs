using ShimmyMySherbet.MySQL.EF.Models;
using ShimmyMySherbet.MySQL.EF.Models.Internals;
using System;

namespace ShimmyMySherbet.MySQL.EF.Internals
{
    public static class SQlMetaBuilder
    {
        public static SQLMetaField GetMeta(IClassField field, int index)
        {
            var nameF = field.GetAttribute<SQLPropertyName>();
            var meta = new SQLMetaField()
            {
                DBNull = field.AttributeDefined<SQLNull>(),
                Field = field,
                FieldIndex = index,
                IsForeignKey = field.AttributeDefined<SQLForeignKey>(),
                IsPrimaryKey = field.AttributeDefined<SQLPrimaryKey>(),
                Omit = field.AttributeDefined<SQLOmit>(),
                OmitOnNull = field.AttributeDefined<SQLOmitOnNull>(),
                OmitOnUpdate = field.AttributeDefined<SQLOmitUpdate>(),
                OmitOnInsert = field.AttributeDefined<SQLOmitInsert>(),
                Unique = field.AttributeDefined<SQLUnique>(),
                Name = nameF != null ? nameF.Name : field.Name,
                Ignore = field.AttributeDefined<SQLIgnore>(),
                AutoIncrement = field.AttributeDefined<SQLAutoIncrement>()
            };




            /*
             * TO IMPLEMENT:
             *
             * SQLForeignKey
             *
             */

            return meta;
        }
    }
}