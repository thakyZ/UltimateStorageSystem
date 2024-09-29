using System.Text.Json;
using System.Text.Json.Serialization;
using UltimateStorageSystem.Network;

namespace UltimateStorageSystem.Tools
{
    [Serializable]
    internal class ChestFilterEntry
    {
        public string LocationName  { get; }
        public int    TilePositionX { get; }
        public int    TilePositionY { get; }

        [JsonIgnore]
        public GameLocation Location       => Game1.locations.First(x => x.NameOrUniqueName.Equals(LocationName, StringComparison.Ordinal));

        [JsonIgnore]
        public Point        TilePosition   => new Point(TilePositionX, TilePositionY);

        [JsonIgnore]
        public Vector2      TilePositionV2 => new Vector2(TilePositionX, TilePositionY);

        public ChestFilterEntry()
        {
            this.LocationName  = Game1.locations.First(x => x.IsFarm).NameOrUniqueName;
            this.TilePositionX = 0;
            this.TilePositionY = 0;
        }

        public ChestFilterEntry(string location, Point tilePosition)
        {
            this.LocationName  = location;
            this.TilePositionX = tilePosition.X;
            this.TilePositionY = tilePosition.Y;
        }

        public ChestFilterEntry(string location, int x, int y)
        {
            this.LocationName  = location;
            this.TilePositionX = x;
            this.TilePositionY = y;
        }

        public ChestFilterEntry(GameLocation location, int x, int y)
        {
            this.LocationName  = location.NameOrUniqueName;
            this.TilePositionX = x;
            this.TilePositionY = y;
        }

        public ChestFilterEntry(GameLocation location, Point tilePosition)
        {
            this.LocationName  = location.NameOrUniqueName;
            this.TilePositionX = tilePosition.X;
            this.TilePositionY = tilePosition.Y;
        }

        public ChestFilterEntry(GameLocation location, Vector2 tilePosition)
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

        /// <inheritdoc cref="object.Equals(object?)" />
        public bool Equals(ChestFilterEntry? chestBlacklistEntry)
        {
            if (chestBlacklistEntry is null)
                return false;

            return this.LocationName.Equals(chestBlacklistEntry.Location.NameOrUniqueName)
                && (this.TilePosition.Equals(chestBlacklistEntry.TilePosition)
                 || (this.TilePositionX.Equals(chestBlacklistEntry.TilePosition.X)
                  && this.TilePositionY.Equals(chestBlacklistEntry.TilePosition.Y)));
        }

        /// <inheritdoc cref="object.Equals(object?)" />
        public bool Equals(GameLocation? location, Point? tilePosition)
        {
            if (location is null || tilePosition is null)
                return false;

            return this.LocationName.Equals(location.NameOrUniqueName)
                && (this.TilePosition.Equals(tilePosition.Value)
                 || (this.TilePositionX.Equals(tilePosition.Value.X)
                  && this.TilePositionY.Equals(tilePosition.Value.Y)));
        }

        /// <inheritdoc cref="object.Equals(object?)" />
        public bool Equals(GameLocation? location, Vector2? tilePosition)
        {
            if (location is null || tilePosition is null)
                return false;

            return this.Equals(location, tilePosition.Value.ToPoint());
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is ChestFilterEntry chestBlacklistEntry)
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

    [Serializable]
    internal sealed class FarmLinkTerminalData
    {
        public HashSet<ChestFilterEntry>? chestFilter = [];

        public HashSet<ChestFilterEntry> ChestFilter
        {
            get => chestFilter ??= [];
            set => chestFilter = value;
        }

        private static JsonSerializerOptions SerializerSettings => new JsonSerializerOptions(JsonSerializerDefaults.General)
        {
            IgnoreReadOnlyFields     = true,
            IgnoreReadOnlyProperties = true,
            DictionaryKeyPolicy      = JsonNamingPolicy.CamelCase,
            AllowTrailingCommas      = true,
        };

        public FarmLinkTerminalData(HashSet<ChestFilterEntry>? chestFilter)
        {
            this.ChestFilter = chestFilter ?? [];
        }

        internal bool HasChestInFilterForLocation(GameLocation location, Vector2 tileLocation)
        {
            return this.ChestFilter.Any(x => x.Equals(location: location, tilePosition: tileLocation));
        }

        internal bool AddChestInFilterForLocation(GameLocation location, Vector2 tileLocation, bool fromNetwork = false)
        {
            var passed = this.ChestFilter.Add(new(location, tileLocation.ToPoint()));
            if (!fromNetwork)
                passed |= MessageManager.SendAddFilter(Game1.player, location, tileLocation.ToPoint());
            return passed;
        }

        internal (bool, int) RemoveChestInFilterForLocation(GameLocation location, Vector2 tileLocation, bool fromNetwork = false)
        {
            var  removed = this.ChestFilter.RemoveWhere(x => x.Equals(location: location, tilePosition: tileLocation));
            bool passed  = removed > 0;
            if (!fromNetwork)
                passed |= MessageManager.SendRemoveFilter(Game1.player, location, tileLocation.ToPoint());
            return (passed, removed);
        }

        internal bool Update(string? jsonString)
        {
            if (jsonString is null) return false;
            try
            {
                var hashSet = Deserialize(jsonString);
                if (hashSet.TrueIfNullAndPrint()) return false;
                this.ChestFilter.Merge(hashSet);
                return true;
            }
            catch (Exception exception)
            {
                Logger.Exception(exception);
            }
            return false;
        }

        internal bool Update(HashSet<ChestFilterEntry>? hashSet)
        {
            if (hashSet is null) return false;
            try
            {
                this.ChestFilter.Merge(hashSet);
                return true;
            }
            catch (Exception exception)
            {
                Logger.Exception(exception);
            }
            return false;
        }

        internal static HashSet<ChestFilterEntry>? Deserialize(string? jsonString)
        {
            if (jsonString is null) return null;
            try
            {
                var hashSet = JsonSerializer.Deserialize<HashSet<ChestFilterEntry>>(jsonString, SerializerSettings);
                if (hashSet.TrueIfNullAndPrint()) return null;
                return hashSet;
            }
            catch (Exception exception)
            {
                Logger.Exception(exception);
            }
            return null;
        }

        internal string Serialize()
        {
            return JsonSerializer.Serialize(this.ChestFilter, SerializerSettings);
        }
    }
}
