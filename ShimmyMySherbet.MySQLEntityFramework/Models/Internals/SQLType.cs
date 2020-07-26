using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShimmyMySherbet.MySQL.EF.Models.TypeModel;

namespace ShimmyMySherbet.MySQL.EF.Models.Internals
{
    public abstract class SQLType : Attribute
    {
        public bool Signed
        {
            get
            {
                bool Signed = false;
                foreach(Attribute attrib in Attribute.GetCustomAttributes(this.GetType()))
                {
                    if (attrib is SQLSigned)
                    {
                        Signed = ((SQLSigned)attrib).Signed;
                    }
                }
                return Signed;
            }
        }
        public int Length
        {
            get
            {
                int Length = -1;
                foreach (Attribute attrib in Attribute.GetCustomAttributes(this.GetType()))
                {
                    if (attrib is SQLLength)
                    {
                        Length = ((SQLLength)attrib).Length;
                    }
                }
                return Length;
            }
        }

        public string TypeName
        {
            get
            {
                string TypeName = "";
                foreach (Attribute attrib in Attribute.GetCustomAttributes(this.GetType()))
                {
                    if (attrib is SQLTypeName)
                    {
                        TypeName = ((SQLTypeName)attrib).Name;
                    }
                }
                return TypeName;
            }
        }

        public bool NoSign
        {
            get
            {
                bool NoSign = false;

                foreach (Attribute attrib in Attribute.GetCustomAttributes(this.GetType()))
                {
                    if (attrib is SQLNoSign)
                    {
                        NoSign = true;
                    }
                }
                return NoSign;
            }
        }


        public virtual string SQLRepresentation
        {
            get
            {
                string Ustat = "";


                if (!Signed && !NoSign)
                {
                    Ustat = " UNSIGNED";
                }

                if (Length == -1)
                {
                    return $"{TypeName}{Ustat}";
                }
                else
                {
                    return $"{TypeName}({Length}){Ustat}";
                }
            }
        }
    }
}
