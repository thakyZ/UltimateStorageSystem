namespace UltimateStorageSystem.Tools
{
    internal class ChestBlacklistEntry
    {
        public string       LocationName   { get; set; }
        public int          TilePositionX  { get; set; }
        public int          TilePositionY  { get; set; }
        public GameLocation Location       => Game1.locations.First(x => x.NameOrUniqueName.Equals(LocationName, StringComparison.Ordinal));
        public Point        TilePosition   => new Point(TilePositionX, TilePositionY);
        public Vector2      TilePositionV2 => new Vector2(TilePositionX, TilePositionY);

        public ChestBlacklistEntry()
        {
            this.LocationName  = Game1.locations.First(x => x.IsFarm).NameOrUniqueName;
            this.TilePositionX = 0;
            this.TilePositionY = 0;
        }
        public ChestBlacklistEntry(string location, Point tilePosition)
        {
            this.LocationName  = location;
            this.TilePositionX = tilePosition.X;
            this.TilePositionY = tilePosition.Y;
        }
        public ChestBlacklistEntry(string location, int x, int y)
        {
            this.LocationName  = location;
            this.TilePositionX = x;
            this.TilePositionY = y;
        }
        public ChestBlacklistEntry(GameLocation location, int x, int y)
        {
            this.LocationName  = location.NameOrUniqueName;
            this.TilePositionX = x;
            this.TilePositionY = y;
        }
        public ChestBlacklistEntry(GameLocation location, Point tilePosition)
        {
            this.LocationName  = location.NameOrUniqueName;
            this.TilePositionX = tilePosition.X;
            this.TilePositionY = tilePosition.Y;
        }
        public ChestBlacklistEntry(GameLocation location, Vector2 tilePosition)
        {
            this.LocationName  = location.NameOrUniqueName;
            this.TilePositionX = (int)tilePosition.X;
            this.TilePositionY = (int)tilePosition.Y;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{this.Location.NameOrUniqueName}:{this.TilePosition.X}:{this.TilePosition.Y}";
        }

        /// <inheritdoc cref="object.Equals" />
        public bool Equals(ChestBlacklistEntry? chestBlacklistEntry)
        {
            if (chestBlacklistEntry is null)
                return false;

            return this.LocationName.Equals(chestBlacklistEntry.Location.NameOrUniqueName)
                   && (this.TilePosition.Equals(chestBlacklistEntry.TilePosition)
                       || (this.TilePositionX.Equals(chestBlacklistEntry.TilePosition.X)
                           && this.TilePositionY.Equals(chestBlacklistEntry.TilePosition.Y)));
        }

        /// <inheritdoc cref="object.Equals" />
        public bool Equals(GameLocation? location, Point? tilePosition)
        {
            if (location is null || tilePosition is null)
                return false;

            return this.LocationName.Equals(location.NameOrUniqueName)
                   && (this.TilePosition.Equals(tilePosition.Value)
                       || (this.TilePositionX.Equals(tilePosition.Value.X)
                           && this.TilePositionY.Equals(tilePosition.Value.Y)));
        }

        /// <inheritdoc cref="object.Equals" />
        public bool Equals(GameLocation? location, Vector2? tilePosition)
        {
            if (location is null || tilePosition is null)
                return false;

            return this.Equals(location, tilePosition.Value.ToPoint());
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is ChestBlacklistEntry chestBlacklistEntry)
                return this.Equals(chestBlacklistEntry: chestBlacklistEntry);

            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(
                this.LocationName.GetHashCode(),
                this.TilePositionX.GetHashCode(),
                this.TilePositionY.GetHashCode()
            );
        }
    }

    internal sealed class FarmLinkTerminalData
    {
        public List<ChestBlacklistEntry> ChestBlacklist { get; set; }

        public FarmLinkTerminalData()
        {
            this.ChestBlacklist = [];
        }

        public FarmLinkTerminalData(List<ChestBlacklistEntry> chestBlacklist)
        {
            this.ChestBlacklist = chestBlacklist;
        }
        internal bool HasChestInBlacklistForLocation(GameLocation location, Vector2 tileLocation)
        {
            return this.ChestBlacklist.Exists((ChestBlacklistEntry x) => x.Equals(location: location, tilePosition: tileLocation));
        }

        internal void AddChestInBlacklistForLocation(GameLocation location, Vector2 tileLocation)
        {
            this.ChestBlacklist.Add(new(location, tileLocation.ToPoint()));
        }

        internal void RemoveChestInBlacklistForLocation(GameLocation location, Vector2 tileLocation)
        {
            this.ChestBlacklist.RemoveWhere((ChestBlacklistEntry x) => x.Equals(location: location, tilePosition: tileLocation));
        }
    }
}
