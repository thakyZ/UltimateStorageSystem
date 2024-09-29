﻿using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Monsters;

#region Api File
namespace SpaceCore
{
    /// <summary>
    /// The API which lets other mods interace with ScapeCore's functionality.
    /// </summary>
    public interface ISpaceCoreApi
    {
        /// <summary>
        /// Returns a list of all currently loaded skill's string IDs
        /// </summary>
        /// <returns>An array of skill IDs</returns>
        string[] GetCustomSkills();

        /// <summary>
        /// Gets the Base level of the custom skill for the farmer.
        /// </summary>
        /// <param name="farmer"> The farmer who you want to get the skill level for.</param>
        /// <param name="skill"> The string ID of the skill you want the level of.</param>
        /// <returns>Int</returns>
        int GetLevelForCustomSkill(Farmer farmer, string skill);

        /// <summary>
        /// Gets the Buff level of the custom skill for the farmer.
        /// </summary>
        /// <param name="farmer"> The farmer who you want to get the skill level for.</param>
        /// <param name="skill"> The string ID of the skill you want the level of.</param>
        /// <returns>Int</returns>
        int GetBuffLevelForCustomSkill(Farmer farmer, string skill);

        /// <summary>
        /// Gets the Base + Buff level of the custom skill for the farmer.
        /// </summary>
        /// <param name="farmer"> The farmer who you want to get the skill level for.</param>
        /// <param name="skill"> The string ID of the skill you want the level of.</param>
        /// <returns>Int</returns>
        int GetTotalLevelForCustomSkill(Farmer farmer, string skill);

        /// <summary>
        /// Gets the total exp the skill has
        /// </summary>
        /// <param name="farmer"> The farmer who you want to get the skill level for.</param>
        /// <param name="skill"> The string ID of the skill you want the level of.</param>
        /// <returns>Int</returns>
        int GetExperienceForCustomSkill(Farmer farmer, string skill);

        /// <summary>
        /// Get a list of all the skills with the ID, current xp value in them, and current level
        /// </summary>
        /// <param name="farmer">The farmer you want the list for</param>
        /// <returns>List&lt;Tuple&lt;Skill, XP value, Level&gt;&gt;</returns>
        List<Tuple<string, int, int>> GetExperienceAndLevelsForCustomSkill(Farmer farmer);

        /// <summary>
        /// Add EXP to a custom skill
        /// </summary>
        /// <param name="farmer"> The farmer who you want to give exp to</param>
        /// <param name="skill"> The string ID of the custom skill</param>
        /// <param name="amt"> The int Amount you want to give</param>
        void AddExperienceForCustomSkill(Farmer farmer, string skill, int amt);

        /// <summary>
        /// Gets the display name of the skill
        /// </summary>
        /// <param name="skill"> The ID of the skill you want to get</param>
        /// <returns>Texture2D</returns>
        string GetDisplayNameOfCustomSkill(string skill);

        /// <summary>
        /// Gets the 10x10 icon of the skill that shows up on the skill page
        /// </summary>
        /// <param name="skill"> The ID of the skill you want to get</param>
        /// <returns>Texture2D</returns>
        Texture2D GetSkillPageIconForCustomSkill(string skill);

        /// <summary>
        /// Gets the 16x16 icon of the skill that shows up in the level up menu.
        /// </summary>
        /// <param name="skill"> The ID of the skill you want to get</param>
        /// <returns>Texture2D</returns>
        Texture2D GetSkillIconForCustomSkill(string skill);

        /// <summary>
        /// Get the profession ID for the custom skill
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="profession"></param>
        /// <returns>int profession ID</returns>
        int GetProfessionId(string skill, string profession);

        /// <summary>
        /// Must have [XmlType("Mods_SOMETHINGHERE")] attribute (required to start with "Mods_")
        /// </summary>
        void RegisterSerializerType(Type type);

        void RegisterCustomProperty(Type declaringType, string name, Type propType, MethodInfo getter, MethodInfo setter);

        void RegisterSpawnableMonster(string id, Func<Vector2, Dictionary<string, object>, Monster> monsterSpawner);

        List<int> GetLocalIndexForMethod(MethodBase meth, string local);

        public event EventHandler<Action<string, Action>> AdvancedInteractionStarted;

        // public List<string> GetVirtualCurrencyList();
        // public bool IsVirtualCurrencyTeamWide(string currency);
        // public int GetVirtualCurrencyAmount(Farmer who, string currency);
        // public void AddToVirtualCurrency(Farmer who, string currency, int amount); // supports negative numbers

        public void RegisterEquipmentSlot(IManifest modManifest, string globalId, Func<Item, bool> slotValidator, Func<string> slotDisplayName, Texture2D bgTex, Rectangle? bgRect = null);
        public Item GetItemInEquipmentSlot(Farmer farmer, string globalId);
        public void SetItemInEquipmentSlot(Farmer farmer, string globalId, Item item);
        public bool CanItemGoInEquipmentSlot(string globalId, Item item);
    }
}
#endregion