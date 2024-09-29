using System.Xml.Serialization;

namespace UltimateStorageSystem.Utilities
{
    internal class ModXmlTypeConstructorAttribute : XmlTypeAttribute
    {
        private static          IManifest? _manifest;
        private static readonly List<Type> typesToRegister = [];
        internal static         Type[]     TypesToRegister => [..typesToRegister];

        public ModXmlTypeConstructorAttribute(Type classType) : base(GetXmlTypeName(classType.Name))
        {
            typesToRegister.Add(classType);
        }

        internal static void Init(IManifest manifest)
        {
            _manifest = manifest;
        }

        private static string GetXmlTypeName(string? className)
        {
            if (_manifest is null)
            {
                throw new NullReferenceException("The mod's manifest was not registered to the class ModXmlTypeConstructorAttribute properly.");
            }

            if (className is null)
            {
                throw new NullReferenceException("The Caller Member Name passed to the construtor was not properly passed.");
            }

            return $"Mod_{_manifest.UniqueID.Replace('.', '_')}_{className}";
        }
    }
}