using System;

namespace ShimmyMySherbet.MySQL.EF.Models.Exceptions
{
    public class SQLInvalidLengthException : Exception
    {
        public SQLInvalidLengthException(string message) : base(message)
        {
        }
    }
}