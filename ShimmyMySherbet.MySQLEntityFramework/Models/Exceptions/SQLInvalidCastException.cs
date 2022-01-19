using System;

namespace ShimmyMySherbet.MySQL.EF.Models.Exceptions
{
    public class SQLInvalidCastException : Exception
    {
        public SQLInvalidCastException(string fieldname, string from, string to) : base($"Cannot convert SQL Type {from} to .NET type {to} in column '{fieldname}'")
        {
        }
    }
}