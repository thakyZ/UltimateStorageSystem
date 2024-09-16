using System.Xml.Serialization;
using UltimateStorageSystem.Overrides;

namespace UltimateStorageSystem
{
    [XmlInclude(typeof(CustomWorkbench))]
    public class ModConfig
    {
        public SButton OpenFarmLinkTerminalHotkey { get; set; } = SButton.None;  // Standardwert ist ein leerer String
    }
}
