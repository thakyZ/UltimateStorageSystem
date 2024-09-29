using GenericModConfigMenu;
using SpaceCore;

namespace UltimateStorageSystem.Integrations
{
    /// <summary>Simplifies validated access to mod APIs.</summary>
    internal static class IntegrationHelper
    {
        /// <summary>Get a mod API if it's installed and valid.</summary>
        /// <param name="label">A human-readable name for the mod.</param>
        /// <param name="modId">The mod's unique ID.</param>
        /// <param name="minVersion">The minimum version of the mod that's supported.</param>
        /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <returns>Returns the mod's API interface if valid, else null.</returns>
        private static TInterface? GetValidatedApi<TInterface>(string label, string modId, string minVersion, IModRegistry modRegistry, IMonitor monitor) where TInterface : class
        {
            // Check mod installed
            IManifest? mod = modRegistry.Get(modId)?.Manifest;

            if (mod is null) return null;

            // Check mod version
            if (mod.Version.IsOlderThan(minVersion))
            {
                monitor.Log($"Detected {label} {mod.Version}, but need {minVersion} or later. Disabled integration with that mod.", LogLevel.Warn);
                return null;
            }

            // Get API
            TInterface? api = modRegistry.GetApi<TInterface>(modId);

            if (api is null)
            {
                monitor.Log($"Detected {label}, but couldn't fetch its API. Disabled integration with that mod.", LogLevel.Warn);
                return null;
            }

            return api;
        }

        /// <summary>
        /// Get Generic Mod Config Menu's API if it's installed and valid.
        /// </summary>
        /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <returns>Returns the API interface if valid, else null.</returns>
        public static IGenericModConfigMenuApi? GetGenericModConfigMenu(IModRegistry modRegistry, IMonitor monitor)
        {
            return GetValidatedApi<IGenericModConfigMenuApi>(
            label: "Generic Mod Config Menu",
            modId: "spacechase0.GenericModConfigMenu",
            // minVersion: "{{GenericModConfigMenu.MinimumVersion}}",
            minVersion: "1.6.0",
            modRegistry: modRegistry,
            monitor: monitor
            );
        }

        /// <summary>
        /// Get SpaceCore's API if it's installed and valid.
        /// </summary>
        /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <returns>Returns the API interface if valid, else null.</returns>
        public static ISpaceCoreApi? GetSpaceCore(IModRegistry modRegistry, IMonitor monitor)
        {
            return GetValidatedApi<ISpaceCoreApi>(
            label: "SpaceCore",
            modId: "spacechase0.SpaceCore",
            // minVersion: "{{SpaceCore.MinimumVersion}}",
            minVersion: "1.25.2",
            modRegistry: modRegistry,
            monitor: monitor
            );
        }
    }
}
