using StardewModdingAPI.Events;
using UltimateStorageSystem.Network.Messages;

namespace UltimateStorageSystem.Network
{
    internal sealed partial class MessageManager : IDisposable
    {
        private static   MessageManager? instance;
        private readonly IModHelper?     helper;
        private          bool            disposedValue;
        private static   string          ModID = string.Empty;

        private MessageManager(IModHelper helper)
        {
            this.helper = helper;
        }

        public static void Init(IModHelper helper, string modID)
        {
            instance = new(helper);
            ModID = modID;
            // Apply events
            helper.Events.Multiplayer.ModMessageReceived += OnModMessageReceieved;
            helper.Events.Multiplayer.PeerConnected += OnPeerConnected;
        }

        private static void OnModMessageReceieved(object? sender, ModMessageReceivedEventArgs e)
        {
            if (!Enum.TryParse(e.Type, out MessageType @enum))
            {
                Logger.CustomTrace("Failed to parse string with value \"{e.Type}\" to enum {nameof()}");
                return;
            }

            switch (@enum)
            {
                case MessageType.RemoveFilter:
                    ProcessRemoveFilter(e.ReadAs<FilterDataChange>());
                    break;
                case MessageType.AddFilter:
                    ProcessAddFilter(e.ReadAs<FilterDataChange>());
                    break;
                case MessageType.ConfigUpdate:
                    ProcessConfigUpdate(e.ReadAs<ConfigUpdate>());
                    break;
                case MessageType.FilterData:
                    ProcessFilterData(e.ReadAs<FilterData>());
                    break;
                case MessageType.RequestData:
                    ProcessRequestData(e.ReadAs<FilterData>());
                    break;
                default:
                    Logger.CustomTrace($"Unknown request data got a {nameof(MessageType.None)} type message.");
                    break;
            }
        }

        private static void OnPeerConnected(object? sender, PeerConnectedEventArgs e)
        {
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                { }

                disposedValue = true;

                if (helper is not null)
                {
                    helper.Events.Multiplayer.ModMessageReceived -= OnModMessageReceieved;
                    helper.Events.Multiplayer.PeerConnected -= OnPeerConnected;
                }
            }
        }

        ~MessageManager()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public static void Unload()
        {
            instance?.Dispose();
        }
    }
}
