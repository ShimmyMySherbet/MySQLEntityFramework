namespace ShimmyMySherbet.MySQL.EF.Models
{
    public enum SQLCharSet
    {
        /// <summary>
        /// Uses the default charset of the MySQL server
        /// </summary>
        ServerDefault,

        /// <summary>
        /// Unicode compliant, cannot store emojis.
        /// </summary>
        utf8,

        utf16,
        utf16le,
        utf32,

        /// <summary>
        /// Unicode compliant, can also store emojis.
        /// </summary>
        utf8mb4,

        binary,
        latin1,
        latin2,
        latin5,
        latin7,
        ascii,
        hebrew,
        greek,

        /// <summary>
        /// Reserved for specifying charsets by name that are not present in this enum
        /// </summary>
        Other
    }
}