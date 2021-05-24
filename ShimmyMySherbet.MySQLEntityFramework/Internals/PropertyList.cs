using System;
using System.Collections;
using System.Collections.Generic;

namespace ShimmyMySherbet.MySQL.EF.Internals
{
    public class PropertyList : IEnumerable<BuildProperty>
    {
        internal List<BuildProperty> m_Properties = new List<BuildProperty>();

        public IEnumerator<BuildProperty> GetEnumerator()
        {
            return m_Properties.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_Properties.GetEnumerator();
        }

        public void Add(string key, object value)
        {
            lock (m_Properties)
            {
                m_Properties.Add(new BuildProperty(key, value));
            }
        }
        public void Reset()
        {
            lock(m_Properties)
            {
                m_Properties.Clear();
            }
        }
        public void Merge(PropertyList properties)
        {
            if (properties == this) throw new NotSupportedException("Cannot merge properties into self.");
            lock (m_Properties)
            {
                properties.ReciveMerge(ref m_Properties);
            }
        }

        internal void ReciveMerge(ref List<BuildProperty> properties)
        {
            lock (m_Properties)
            {
                m_Properties.AddRange(properties);
            }
        }
    }
}