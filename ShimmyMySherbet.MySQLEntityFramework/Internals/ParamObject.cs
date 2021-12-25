namespace ShimmyMySherbet.MySQL.EF.Internals
{
    public struct ParamObject
    {
        public string Key;
        public object Value;

        public ParamObject(string key, object value)
        {
            Key = key;
            Value = value;
        }
    }
}