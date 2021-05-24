using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Internals
{
    public struct BuildProperty
    {
        public string Key;
        public object Value;

        public BuildProperty(string key, object value)
        {
            Key = key;
            Value = value;
        }
    }
}
