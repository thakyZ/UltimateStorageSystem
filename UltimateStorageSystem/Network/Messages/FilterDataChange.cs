namespace UltimateStorageSystem.Network.Messages
{
    internal class FilterDataChange
    {
        public long   FarmerID      { get; set; }
        public string GameLocation  { get; init; } = string.Empty;
        public int    TileLocationX { get; init; }
        public int    TileLocationY { get; init; }
    }
}
