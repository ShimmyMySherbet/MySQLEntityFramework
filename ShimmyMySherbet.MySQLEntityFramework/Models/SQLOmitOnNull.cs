using System;

namespace ShimmyMySherbet.MySQL.EF.Models
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SQLOmitOnNull : Attribute
    {
    }
}