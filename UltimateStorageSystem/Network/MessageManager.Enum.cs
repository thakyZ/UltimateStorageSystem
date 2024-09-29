namespace UltimateStorageSystem.Network
{
    internal partial class MessageManager
    {
        internal enum MessageType
        {
            None         /* = 0 */,
            FilterData   /* = 1 */,
            AddFilter    /* = 2 */,
            RemoveFilter /* = 3 */,
            ConfigUpdate /* = 4 */,
            RequestData  /* = 5 */,
        }
    }
}
