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
    public sealed class SQLOmit : Attribute
    {
    }
}
