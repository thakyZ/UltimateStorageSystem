using GenericModConfigMenu;

namespace UltimateStorageSystem.Integrations.GenericModConfigMenu
{
    internal static class GenericModConfigMenuIntegration
    {
        /// <summary>
        /// Add a config UI to Generic Mod Config Menu if it's installed.
        /// </summary>
        /// <param name="manifest">The mod manifest.</param>
        /// <param name="modRegistry">The mod registry from which to get the API.</param>
        /// <param name="monitor">The monitor with which to log errors.</param>
        /// <param name="getConfig">Get the current mod configuration.</param>
        /// <param name="reset">Reset the config to its default values.</param>
        /// <param name="save">Save the current config to the <c>config.json</c> file.</param>
        /// <param name="titleScreenOnly">Whether the options can only be edited from the title screen.</param>
        public static void Register(IManifest manifest, IModRegistry modRegistry, IMonitor monitor, Func<ModConfig> getConfig, Action reset, Action save, bool titleScreenOnly = false)
        {
            // Get API
            IGenericModConfigMenuApi? api = IntegrationHelper.GetGenericModConfigMenu(modRegistry, monitor);

            if (api is null)
                return;

            // Register config UI
            api.Register(manifest, reset, save, titleScreenOnly);

            // Hotkey options
            api.AddSectionTitle(mod: manifest,
                                text: I18n.Config_Hotkeys_Title);
            // Add the config option to set the hotkey to open the FarmLink terminal..
            api.AddKeybind(mod: manifest,
                           name: I18n.Config_OpenFarmLinkTerminalHotkey_Name,
                           tooltip: I18n.Config_OpenFarmLinkTerminalHotkey_Desc,
                           getValue: () => getConfig().OpenFarmLinkTerminalHotkey,
                           setValue: value => getConfig().OpenFarmLinkTerminalHotkey = value);
        }
    }
}
