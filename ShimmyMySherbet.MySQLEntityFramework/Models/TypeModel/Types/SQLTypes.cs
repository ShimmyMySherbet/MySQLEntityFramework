using System;
using ShimmyMySherbet.MySQL.EF.Models.Internals;

namespace ShimmyMySherbet.MySQL.EF.Models.TypeModel.Types
{
    [SQLTypeName("INT"), SQLSigned(true), SQLNetType(typeof(int))]
    public sealed class SQLTypeInt : SQLType
    {
    }

    [SQLTypeName("INT"), SQLSigned(false), SQLNetType(typeof(uint))]
    public sealed class SQLTypeUInt : SQLType
    {
    }

    [SQLTypeName("SMALLINT"), SQLSigned(true), SQLNetType(typeof(short))]
    public sealed class SQLTypeShort : SQLType
    {
    }

    [SQLTypeName("SMALLINT"), SQLSigned(false), SQLNetType(typeof(ushort))]
    public sealed class SQLTypeUShort : SQLType
    {
    }

    [SQLTypeName("TINYINT"), SQLSigned(true), SQLNetType(typeof(byte))]
    public sealed class SQLTypeByte : SQLType
    {
    }

    [SQLTypeName("MEDIUMINT"), SQLNetType(null)]
    public sealed class SQLTypeMediumInt : SQLType
    {
    }

    [SQLTypeName("BIGINT"), SQLSigned(true), SQLNetType(typeof(long))]
    public sealed class SQLTypeLong : SQLType
    {
    }

    [SQLTypeName("BIGINT"), SQLSigned(false), SQLNetType(typeof(ulong))]
    public sealed class SQLTypeULong : SQLType
    {
    }

    [SQLTypeName("FLOAT"), SQLNetType(typeof(float))]
    public sealed class SQLTypeFloat : SQLType
    {
    }

    [SQLTypeName("DOUBLE"), SQLNetType(typeof(double))]
    public sealed class SQLTypeDouble : SQLType
    {
    }

    [SQLTypeName("DECIMAL"), SQLNetType(typeof(decimal))]
    public sealed class SQLTypeDecimal : SQLType
    {
    }

    [SQLTypeName("DATE"), SQLNetType(null), SQLNoSign]
    public sealed class SQLTypeDate : SQLType
    {
    }

    [SQLTypeName("DATETIME"), SQLNetType(typeof(DateTime)), SQLNoSign]
    public sealed class SQLTypeDateTime : SQLType
    {
    }

    [SQLTypeName("TIMESTAMP"), SQLNetType(null), SQLNoSign]
    public sealed class SQLTypeTimestamp : SQLType
    {
    }

    [SQLTypeName("TIME"), SQLNetType(null), SQLNoSign]
    public sealed class SQLTypeTime : SQLType
    {
    }

    [SQLTypeName("BOOLEAN"), SQLNetType(typeof(bool)), SQLNoSign]
    public sealed class SQLTypeBool : SQLType
    {
    }

    [SQLTypeName("CHAR"), SQLNetType(typeof(char)), SQLNoSign]
    public sealed class SQLTypeChar : SQLType
    {
    }

    [SQLTypeName("VARCHAR"), SQLNetType(null), SQLLength(255), SQLNoSign]
    public sealed class SQLTypeVarChar : SQLType
    {
    }

    [SQLTypeName("TINYTEXT"), SQLNetType(null), SQLLength(255), SQLNoSign]
    public sealed class SQLTypeTinyText : SQLType
    {
    }

    [SQLTypeName("TEXT"), SQLNetType(typeof(string)), SQLNoSign]
    public sealed class SQLTypeText : SQLType
    {
    }

    [SQLTypeName("BLOB"), SQLNetType(typeof(byte[])), SQLNoSign]
    public sealed class SQLTypeBlob : SQLType
    {
    }

    [SQLTypeName("MEDIUMTEXT"), SQLNetType(null), SQLNoSign]
    public sealed class SQLTypeMediumText : SQLType
    {
    }

    [SQLTypeName("MEDIUMBLOB"), SQLNetType(null), SQLNoSign]
    public sealed class SQLTypeMediumBlob : SQLType
    {
    }

    [SQLTypeName("LONGTEXT"), SQLNetType(null), SQLNoSign]
    public sealed class SQLTypeLongText : SQLType
    {
    }

    [SQLTypeName("LONGBLOB"), SQLNetType(null), SQLNoSign]
    public sealed class SQLTyeLongBlob : SQLType
    {
    }
}