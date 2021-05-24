namespace ShimmyMySherbet.MySQL.EF.Internals
{
    public class PrefixAssigner
    {
        private int m_PrefixLevel = -1;
        private object lockObj = new object();

        public int AssignPrefix()
        {
            lock (lockObj)
            {
                m_PrefixLevel++;
                return m_PrefixLevel;
            }
        }
    }
}