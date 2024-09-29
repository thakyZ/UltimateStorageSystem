using UltimateStorageSystem.Network.Messages;
using UltimateStorageSystem.Tools;

namespace UltimateStorageSystem.Network
{
    internal partial class MessageManager
    {
        private static void ProcessAddFilter(FilterDataChange? message)
        {
            if (message.TrueIfNullAndPrint() || message.GameLocation.ToGameLocation() is not GameLocation location) return;
            ModEntry.Instance.FarmLinkTerminalData?.AddChestInFilterForLocation(location, new Vector2(message.TileLocationX, message.TileLocationY), true);
        }

        private static void ProcessRemoveFilter(FilterDataChange? message)
        {
            if (message.TrueIfNullAndPrint() || message.GameLocation.ToGameLocation() is not GameLocation location) return;
            ModEntry.Instance.FarmLinkTerminalData?.RemoveChestInFilterForLocation(location, new Vector2(message.TileLocationX, message.TileLocationY), true);
        }

        private static void ProcessConfigUpdate(ConfigUpdate? message)
        {
            if (message.TrueIfNullAndPrint() || !ModEntry.IsMainFarmer(message.FarmerID)) return;
            ModEntry.UpdateHostConfig(useWhitelist: message.UseWhitelist);
        }

        private static void ProcessFilterData(FilterData? message)
        {
            if (message.TrueIfNullAndPrint() || instance.TrueIfNullAndPrint()) return;
            if (message.FarmerID != Game1.player.UniqueMultiplayerID) return;
            ModEntry.Instance.FarmLinkTerminalData?.Update(FarmLinkTerminalData.Deserialize(message.JsonData));
        }

        private static void ProcessRequestData(FilterData? message)
        {
            if (message.TrueIfNullAndPrint()) return;
            long farmerID = message.FarmerID;
            if (message.JsonData is not null) Logger.Warn($"Recieved invalid json data from a {nameof(MessageType.RequestData)} message.");
            SendFilterData(farmerID);
        }
    }
}
