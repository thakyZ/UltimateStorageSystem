using SpaceCore;

namespace UltimateStorageSystem.Integrations.SpaceCore
{
    /// <summary>
    /// Creates
    /// </summary>
    internal sealed class SpaceCoreIntegration
    {
        private static   SpaceCoreIntegration? _instance;
        private readonly ISpaceCoreApi?        api;
        public static    bool                  Initialized => _instance?.api is not null;

        private SpaceCoreIntegration(IModRegistry modRegistry, IMonitor monitor)
        {
            // Get API
            this.api = IntegrationHelper.GetSpaceCore(modRegistry, monitor);
        }

        public static void Init(IModRegistry modRegistry, IMonitor monitor)
        {
            _instance = new(modRegistry, monitor);
        }

        private void RegisterSerializerTypesImpl()
        {
            if (api is null)
                return;

            foreach (Type type in ModXmlTypeConstructorAttribute.TypesToRegister)
            {
                api.RegisterSerializerType(type);
            }
        }

        public static void RegisterSerializerTypes()
        {
            if (Initialized)
            {
                _instance?.RegisterSerializerTypesImpl();
            }
            else
            {
                Logger.WarnOnce($"Tried calling the {nameof(RegisterSerializerTypes)} method of {nameof(SpaceCoreIntegration)} while it was not {nameof(Initialized).ToLower()}");
            }
        }
    }
}
