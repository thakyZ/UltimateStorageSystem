using System.Xml.Serialization;

namespace UltimateStorageSystem.Utilities
{
    internal class ModXmlTypeConstructorAttribute : XmlTypeAttribute
    {
        private static  IManifest? manifest;
        private static  List<Type> typesToRegister = [];
        internal static Type[]     TypesToRegister => [..typesToRegister];

        public ModXmlTypeConstructorAttribute(Type classType) : base(GetXmlTypeName(classType.Name))
        {
            typesToRegister.Add(classType);
        }

        internal static void Init(IManifest _manifest)
        {
            manifest = _manifest;
        }

        private static string GetXmlTypeName(string? className)
        {
            if (manifest is null)
            {
                throw new NullReferenceException("The mod's manifest was not registered to the class ModXmlTypeConstructorAttribute properly.");
            }

            if (className is null)
            {
                throw new NullReferenceException("The Caller Member Name passed to the construtor was not properly passed.");
            }

            return $"Mod_{manifest.UniqueID.Replace('.', '_')}_{className}";
        }
    }
}