using System;

namespace ShimmyMySherbet.MySQL.EF.Models
{
    public class SQLCharSetAttribute : Attribute
    {
        public SQLCharSet CharSet { get; }
        public string CharSetName { get; }

        public SQLCharSetAttribute(SQLCharSet charset = SQLCharSet.utf8mb4)
        {
            CharSet = charset;
            if (charset == SQLCharSet.ServerDefault)
            {
                CharSetName = string.Empty;
            }
            else
            {
                CharSetName = charset.ToString();
            }
        }

        public SQLCharSetAttribute(string charset)
        {
            CharSet = SQLCharSet.Other;
            CharSetName = charset;
        }
    }
}