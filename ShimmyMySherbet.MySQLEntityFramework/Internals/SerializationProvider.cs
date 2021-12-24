using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using ShimmyMySherbet.MySQL.EF.Models;
using ShimmyMySherbet.MySQL.EF.Models.Internals;
using ShimmyMySherbet.MySQL.EF.Models.TypeModel;

namespace ShimmyMySherbet.MySQL.EF.Internals
{
    public static class SerializationProvider
    {
        private static ConcurrentDictionary<Type, XmlSerializer> m_SerializerCache = new ConcurrentDictionary<Type, XmlSerializer>();

        private static XmlSerializer GetOrCreateSerializer(Type type)
        {
            if (m_SerializerCache.TryGetValue(type, out var ser))
            {
                return ser;
            }

            var n = new XmlSerializer(type);
            m_SerializerCache[type] = n;
            return n;
        }

        public static SQLType GetTypeFor(ESerializeFormat format)
        {
            switch (format)
            {
                case ESerializeFormat.JSON:
                case ESerializeFormat.XML:
                    return SQLTypeHelper.GetSQLType(typeof(string));
            }
            return null;
        }

        public static object Serialize(ESerializeFormat format, object instance, Type type)
        {
            switch (format)
            {
                case ESerializeFormat.XML:
                    var serializer = GetOrCreateSerializer(type);
                    var sb = new StringBuilder();
                    using (var xm = XmlWriter.Create(sb))
                    {
                        serializer.Serialize(xm, instance);
                        xm.Flush();
                        return sb.ToString();
                    }
                case ESerializeFormat.JSON:
                    return JsonConvert.SerializeObject(instance);
            }
            return null;
        }

        public static object Deserialize(ESerializeFormat format, object data, Type target)
        {
            switch (format)
            {
                case ESerializeFormat.XML:
                    var serializer = GetOrCreateSerializer(target);
                    using (var re = new StringReader((string)data))
                    {
                        return serializer.Deserialize(re);
                    }
                case ESerializeFormat.JSON:
                    return JsonConvert.DeserializeObject((string)data, target);
            }
            return null;
        }
    }
}