using UltimateStorageSystem.Network.Messages;

namespace UltimateStorageSystem.Network
{
    internal partial class MessageManager
    {
        public static bool SendAddFilter(Farmer? farmer, GameLocation location, Point tileLocation)
        {
            if (farmer.TrueIfNullAndPrint() || instance?.helper is null)
                return false;

            var message = new FilterDataChange
                          {
                              FarmerID      = farmer.UniqueMultiplayerID,
                              GameLocation  = location.NameOrUniqueName,
                              TileLocationX = tileLocation.X,
                              TileLocationY = tileLocation.Y,
                          };
            instance.helper.Multiplayer.SendMessage(message, nameof(MessageType.AddFilter), [ModID], []);
            return true;
        }

        public static bool SendRemoveFilter(Farmer? farmer, GameLocation location, Point tileLocation)
        {
            if (farmer.TrueIfNullAndPrint() || instance?.helper is null)
                return false;
            var message = new FilterDataChange
                          {
                              FarmerID      = farmer.UniqueMultiplayerID,
                              GameLocation  = location.NameOrUniqueName,
                              TileLocationX = tileLocation.X,
                              TileLocationY = tileLocation.Y,
                          };
            instance.helper.Multiplayer.SendMessage(message, nameof(MessageType.RemoveFilter), [ModID], []);
            return true;
        }

        public static void SendConfigUpdate(Farmer? farmer, ModConfig config, long? playerID = null)
        {
            if (farmer.TrueIfNullAndPrint() || !farmer.IsMainPlayer || instance?.helper is null)
                return;
            var message = new ConfigUpdate
                          {
                              FarmerID     = farmer.UniqueMultiplayerID,
                              UseWhitelist = config.UseWhiteList,
                          };
            instance.helper.Multiplayer.SendMessage(message, nameof(MessageType.ConfigUpdate), [ModID], playerID is not null ? [playerID.Value] : null);
        }

        /// <summary>
        /// Send
        /// </summary>
        public static void SendDataRequest()
        {
            if (instance?.helper is null || Game1.player.IsMainPlayer || ModEntry.GetMainFarmer() is not Farmer mainFarmer)
                return;
            var message = new FilterData
                          {
                              FarmerID     = Game1.player.UniqueMultiplayerID,
                          };
            instance.helper.Multiplayer.SendMessage(message, nameof(MessageType.ConfigUpdate), [ModID], [mainFarmer.UniqueMultiplayerID]);
        }

        /// <summary>
        /// Send a packet to the farmer who requested the data.
        /// </summary>
        /// <param name="farmerID">The farmer's Unique Multiplayer ID to send the data to.</param>
        private static void SendFilterData(long farmerID)
        {
            if (instance?.helper is null)
                return;
            var message = new FilterData
                          {
                              FarmerID = farmerID,
                              JsonData = ModEntry.Instance.FarmLinkTerminalData?.Serialize() ?? "[]",
                          };
            instance.helper.Multiplayer.SendMessage(message, nameof(MessageType.ConfigUpdate), [ModID], [farmerID]);
        }
    }
}
