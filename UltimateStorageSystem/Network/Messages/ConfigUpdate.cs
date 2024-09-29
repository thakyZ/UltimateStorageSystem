namespace UltimateStorageSystem.Network.Messages
{
    internal class ConfigUpdate
    {
        public long  FarmerID     { get; init; }
        public bool? UseWhitelist { get; init; }
    }
}
