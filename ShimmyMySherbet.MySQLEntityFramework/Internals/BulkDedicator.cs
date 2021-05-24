using System.Collections.Generic;
using System.Text;

namespace ShimmyMySherbet.MySQL.EF.Internals
{
    public class BulkDedicator
    {
        public int Threads { get; private set; }
        private int m_index = 0;
        private object m_lock = new object();

        private List<StringBuilder> m_Commands = new List<StringBuilder>();
        private List<PropertyList> m_CommandProperties = new List<PropertyList>();
        private List<PrefixAssigner> m_Assigners = new List<PrefixAssigner>();

        public BulkDedicator(int threads)
        {
            Threads = threads;
        }

        public void GetService(out StringBuilder stringBuilder, out PropertyList properties, out int prefix)
        {
            int i = SelectHandler();

            lock (m_Commands)
            {
                stringBuilder = m_Commands[i];
            }

            lock (m_CommandProperties)
            {
                properties = m_CommandProperties[i];
            }

            lock (m_Assigners)
            {
                prefix = m_Assigners[i].AssignPrefix();
            }
        }

        private int SelectHandler()
        {
            if (Threads <= 1)
                return 0;
            lock (m_lock)
            {
                var r = m_index;

                m_index++;
                if (m_index >= Threads)
                {
                    m_index = 0;
                }
                return r;
            }
        }
    }
}