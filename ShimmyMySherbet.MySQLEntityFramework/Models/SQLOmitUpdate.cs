using System;

namespace ShimmyMySherbet.MySQL.EF.Models
{
    /// <summary>
    /// Omits the field when updating
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]

    public sealed class SQLOmitUpdate : Attribute
    {
    }
}