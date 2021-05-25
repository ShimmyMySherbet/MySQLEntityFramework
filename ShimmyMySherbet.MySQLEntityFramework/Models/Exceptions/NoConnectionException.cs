using System;

namespace ShimmyMySherbet.MySQL.EF.Models.Exceptions
{
    public sealed class NoConnectionException : Exception
    {
        public NoConnectionException(string msg) : base(msg)
        {
        }
    }
}