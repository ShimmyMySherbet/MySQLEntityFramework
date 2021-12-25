using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Models
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SQLIngoreProperties : Attribute
    {
    }
}
