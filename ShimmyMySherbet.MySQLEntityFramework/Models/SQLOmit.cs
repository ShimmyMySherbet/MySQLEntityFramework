using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimmyMySherbet.MySQL.EF.Models
{
    /// <summary>
    /// Omits the field on insert and update
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class SQLOmit : Attribute
    {
    }
}
