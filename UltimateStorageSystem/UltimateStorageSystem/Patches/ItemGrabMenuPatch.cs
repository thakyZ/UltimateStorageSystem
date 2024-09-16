using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Objects;
using HarmonyLib;
using StardewValley.Menus;
using UltimateStorageSystem.Tools;

namespace UltimateStorageSystem.Patches
{
    [SuppressMessage("CodeQuality", "IDE0079"), SuppressMessage("Roslynator", "RCS1102"), SuppressMessage("Roslynator", "RCS1163"), SuppressMessage("Roslynator", "RCS1213")]
    [HarmonyPatch(typeof(ItemGrabMenu), nameof(ItemGrabMenu.draw), typeof(SpriteBatch))]
    public class ItemGrabMenuPatchDraw
    {
        static          FieldInfo                  f_junimoNoteIcon = AccessTools.Field(typeof(ItemGrabMenu),              nameof(ItemGrabMenu.junimoNoteIcon));
        static          MethodInfo                 m_draw           = AccessTools.Method(typeof(ClickableTextureComponent), nameof(ClickableTextureComponent.draw));
        static          MethodInfo                 m_PatchedMethod  = SymbolExtensions.GetMethodInfo((ItemGrabMenu i, SpriteBatch b) => ItemGrabMenuPatch.PatchedDrawMethod(i, b));
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> _instructions)
        {
            var                   found1       = false; /* found `clickableTextureComponent4` */
            var                   found2       = false; /* found `clickableTextureComponent4.draw(b);` */
            var                   patched      = false;
            List<CodeInstruction> instructions = [.._instructions];
            for (int i = 0; i < instructions.Count; i++)
            {
                var instruction = instructions[0];
                if (found2 && !patched)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, m_PatchedMethod);
                    patched     = true;
                }
                if (instruction.LoadsField(f_junimoNoteIcon))
                {
                    found1     = true;
                }
                if (found1 && instruction.Calls(m_draw))
                {
                    found2     = true;
                }
                yield return instruction;
            }
            if (!found1)
                Logger.WarnOnce("Cannot find <Ldfld clickableTextureComponent4> in ItemGrabMenu.draw");
            if (!found2)
                Logger.WarnOnce("Cannot find <Callvirt clickableTextureComponent4.draw> in ItemGrabMenu.draw");

        }
    }

    //     ClickableTextureComponent clickableTextureComponent4 = this.junimoNoteIcon;
    // (1587,4)-(1587,28) D:\GitlabRunner\builds\Gq5qA5P4\0\ConcernedApe\stardewvalley\Farmer\Farmer\Menus\ItemGrabMenu.cs
    /* 0x00278FF8 02           */ // IL_0680: ldarg.0
    /* 0x00278FF9 7B17200004   */ // IL_0681: ldfld     class StardewValley.Menus.ClickableTextureComponent StardewValley.Menus.ItemGrabMenu::junimoNoteIcon
    //     if (clickableTextureComponent4 != null)
    /* 0x00278FFE 25           */ // IL_0686: dup
    /* 0x00278FFF 2D03         */ // IL_0687: brtrue.s  IL_068C
    /* 0x00279001 26           */ // IL_0689: pop
    /* 0x00279002 2B06         */ // IL_068A: br.s      IL_0692
    //         clickableTextureComponent4.draw(b);
    /* 0x00279004 03           */ // IL_068C: ldarg.1
    /* 0x00279005 6FD7300006   */ // IL_068D: callvirt  instance void StardewValley.Menus.ClickableTextureComponent::draw(class [MonoGame.Framework]Microsoft.Xna.Framework.Graphics.SpriteBatch)

    [SuppressMessage("CodeQuality", "IDE0079"), SuppressMessage("Roslynator", "RCS1102"), SuppressMessage("Roslynator", "RCS1163"), SuppressMessage("Roslynator", "RCS1213")]
    [HarmonyPatch(typeof(ItemGrabMenu), [typeof(IList<Item>), typeof(bool), typeof(bool), typeof(InventoryMenu.highlightThisItem), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(string), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(Item), typeof(int), typeof(object), typeof(ItemExitBehavior), typeof(bool)])]
    public class ItemGrabMenuPatchCtor
    {
        static FieldInfo  f_junimoNoteIcon           = AccessTools.Field(typeof(ItemGrabMenu),              nameof(ItemGrabMenu.junimoNoteIcon));
        static MethodInfo m_ShouldShowJunimoNoteIcon = AccessTools.Method(typeof(InventoryPage), nameof(InventoryPage.ShouldShowJunimoNoteIcon));
        static MethodInfo m_PatchedMethod            = SymbolExtensions.GetMethodInfo((ItemGrabMenu __instance, Item? sourceItem) => ItemGrabMenuPatch.PatchedCctorMethod(__instance, sourceItem));
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> _instructions)
        {
            var                   found1       = false; /* found IL_01AF: call */
            var                   found2       = false; /* found IL_023F: stfld */
            var                   patched      = false;
            List<CodeInstruction> instructions = [.._instructions];
            for (int i = 0; i < instructions.Count; i++)
            {
                var instruction = instructions[0];
                if (found2 && !patched)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 14);
                    yield return new CodeInstruction(OpCodes.Call,     m_PatchedMethod);
                    patched     = true;
                }
                if (instruction.Calls(m_ShouldShowJunimoNoteIcon))
                {
                    found1     = true;
                }
                if (found1 && instruction.StoresField(f_junimoNoteIcon))
                {
                    found2     = true;
                }
                yield return instruction;
            }
            if (!found1)
                Logger.WarnOnce("Cannot find <Call bool StardewValley.Menus.InventoryPage::ShouldShowJunimoNoteIcon()> in ItemGrabMenu.cctor");
            if (!found2)
                Logger.WarnOnce("Cannot find <Stfld StardewValley.Menus.ItemGrabMenu::junimoNoteIcon> in ItemGrabMenu.cctor");

        }
    }

    /*
     * ldarg_0    ItemGrabMenu this
     * ldarg_1    IList<Item> inventory
     * ldarg_2    bool reverseGrab
     * ldarg_3    bool showReceivingMenu
     * ldarg_S  4 InventoryMenu.highlightThisItem highlightFunction
     * ldarg_S  5 ItemGrabMenu.behaviorOnItemSelect behaviorOnItemSelectFunction
     * ldarg_S  6 string message
     * ldarg_S  7 ItemGrabMenubehaviorOnItemSelect? behaviorOnItemGrab = null
     * ldarg_S  8 bool snapToBottom = false
     * ldarg_S  9 bool canBeExitedWithKey = false
     * ldarg_S 10 bool playRightClickSound = true
     * ldarg_S 11 bool allowRightClick = true
     * ldarg_S 12 bool showOrganizeButton = false
     * ldarg_S 13 int source = 0
     * ldarg_S 14 Item? sourceItem = null
     * ldarg_S 15 int whichSpecialButton  -1
     * ldarg_S 16 object? context = null
     * ldarg_S 17 ItemExitBehavior heldItemExitBehavior = ItemExitBehavior.ReturnToPlayer
     * ldarg_S 18 bool allowExitWithHeldItem = false
     */

    //             if (InventoryPage.ShouldShowJunimoNoteIcon())
    /* (203,5)-(203,50) D:\GitlabRunner\builds\Gq5qA5P4\0\ConcernedApe\stardewvalley\Farmer\Farmer\Menus\ItemGrabMenu.cs */
    /* 0x0027602F 2882320006   */ // IL_01AF: call      bool StardewValley.Menus.InventoryPage::ShouldShowJunimoNoteIcon()
    /* 0x00276034 398B000000   */ // IL_01B4: brfalse   IL_0244
    //                 this.junimoNoteIcon = new ClickableTextureComponent("", new Rectangle(this.xPositionOnScreen + this.width, this.yPositionOnScreen + this.height / 3 - 64 + -216, 64, 64), "", Game1.content.LoadString("Strings\\UI:GameMenu_JunimoNote_Hover"), Game1.mouseCursors, new Rectangle(331, 374, 15, 14), 4f, false)
    //                 {
    //                     myID = 898,
    //                     leftNeighborID = 11,
    //                     downNeighborID = 106
    //                 };
    /* (205,6)-(210,8) D:\GitlabRunner\builds\Gq5qA5P4\0\ConcernedApe\stardewvalley\Farmer\Farmer\Menus\ItemGrabMenu.cs */
    /* 0x00276039 02           */ // IL_01B9: ldarg.0
    /* 0x0027603A 7274090070   */ // IL_01BA: ldstr     ""
    /* 0x0027603F 02           */ // IL_01BF: ldarg.0
    /* 0x00276040 7BA51F0004   */ // IL_01C0: ldfld     int32 StardewValley.Menus.IClickableMenu::xPositionOnScreen
    /* 0x00276045 02           */ // IL_01C5: ldarg.0
    /* 0x00276046 7BA31F0004   */ // IL_01C6: ldfld     int32 StardewValley.Menus.IClickableMenu::width
    /* 0x0027604B 58           */ // IL_01CB: add
    /* 0x0027604C 02           */ // IL_01CC: ldarg.0
    /* 0x0027604D 7BA61F0004   */ // IL_01CD: ldfld     int32 StardewValley.Menus.IClickableMenu::yPositionOnScreen
    /* 0x00276052 02           */ // IL_01D2: ldarg.0
    /* 0x00276053 7BA41F0004   */ // IL_01D3: ldfld     int32 StardewValley.Menus.IClickableMenu::height
    /* 0x00276058 19           */ // IL_01D8: ldc.i4.3
    /* 0x00276059 5B           */ // IL_01D9: div
    /* 0x0027605A 58           */ // IL_01DA: add
    /* 0x0027605B 1F40         */ // IL_01DB: ldc.i4.s  64
    /* 0x0027605D 59           */ // IL_01DD: sub
    /* 0x0027605E 2028FFFFFF   */ // IL_01DE: ldc.i4    -216
    /* 0x00276063 58           */ // IL_01E3: add
    /* 0x00276064 1F40         */ // IL_01E4: ldc.i4.s  64
    /* 0x00276066 1F40         */ // IL_01E6: ldc.i4.s  64
    /* 0x00276068 737F01000A   */ // IL_01E8: newobj    instance void [MonoGame.Framework]Microsoft.Xna.Framework.Rectangle::.ctor(int32, int32, int32, int32)
    /* 0x0027606D 7274090070   */
    // IL_01ED: ldstr     ""
    /* 0x00276072 7EC7060004   */ // IL_01F2: ldsfld    class StardewValley.LocalizedContentManager StardewValley.Game1::content
    /* 0x00276077 72FD1D0670   */ // IL_01F7: ldstr     "Strings\\UI:GameMenu_JunimoNote_Hover"
    /* 0x0027607C 6F430D0006   */ // IL_01FC: callvirt  instance string StardewValley.LocalizedContentManager::LoadString(string)
    /* 0x00276081 7EFB060004   */
    // IL_0201: ldsfld    class [MonoGame.Framework]Microsoft.Xna.Framework.Graphics.Texture2D StardewValley.Game1::mouseCursors
    /* 0x00276086 204B010000   */ // IL_0206: ldc.i4    331
    /* 0x0027608B 2076010000   */ // IL_020B: ldc.i4    374
    /* 0x00276090 1F0F         */ // IL_0210: ldc.i4.s  15
    /* 0x00276092 1F0E         */ // IL_0212: ldc.i4.s  14
    /* 0x00276094 737F01000A   */ // IL_0214: newobj    instance void [MonoGame.Framework]Microsoft.Xna.Framework.Rectangle::.ctor(int32, int32, int32, int32)
    /* 0x00276099 2200008040   */
    // IL_0219: ldc.r4    4
    /* 0x0027609E 16           */ // IL_021E: ldc.i4.0
    /* 0x0027609F 73D1300006   */ // IL_021F: newobj    instance void StardewValley.Menus.ClickableTextureComponent::.ctor(string, valuetype [MonoGame.Framework]Microsoft.Xna.Framework.Rectangle, string, string, class [MonoGame.Framework]Microsoft.Xna.Framework.Graphics.Texture2D, valuetype [MonoGame.Framework]Microsoft.Xna.Framework.Rectangle, float32, bool)
    /* 0x002760A4 25           */ // IL_0224: dup
    /* 0x002760A5 2082030000   */ // IL_0225: ldc.i4    898
    /* 0x002760AA 7D4D1E0004   */ // IL_022A: stfld     int32 StardewValley.Menus.ClickableComponent::myID
    /* 0x002760AF 25           */ // IL_022F: dup
    /* 0x002760B0 1F0B         */ // IL_0230: ldc.i4.s  11
    /* 0x002760B2 7D4E1E0004   */ // IL_0232: stfld     int32 StardewValley.Menus.ClickableComponent::leftNeighborID
    /* 0x002760B7 25           */ // IL_0237: dup
    /* 0x002760B8 1F6A         */ // IL_0238: ldc.i4.s  106
    /* 0x002760BA 7D511E0004   */ // IL_023A: stfld     int32 StardewValley.Menus.ClickableComponent::downNeighborID
    /* 0x002760BF 7D17200004   */ // IL_023F: stfld     class StardewValley.Menus.ClickableTextureComponent StardewValley.Menus.ItemGrabMenu::junimoNoteIcon


    [SuppressMessage("CodeQuality", "IDE0079"), SuppressMessage("Roslynator", "RCS1102"), SuppressMessage("Roslynator", "RCS1163"), SuppressMessage("Roslynator", "RCS1213")]
    [HarmonyPatch(typeof(ItemGrabMenu), nameof(ItemGrabMenu.receiveLeftClick))]
    public class ItemGrabMenuPatchMethodTwo
    {
        static FieldInfo f_sourceItem = AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.sourceItem));

        public static void Postfix(ItemGrabMenu __instance, int x, int y, bool playSound /* = true */)
        {
            if (f_sourceItem.TryGetValue(__instance, out Item? item, out Exception? sItemException) && item is Chest chest && ItemGrabMenuPatch.ChestBlacklistButton?.containsPoint(x, y) == true)
            {
                if (ItemGrabMenuPatch.ToggledOn(__instance))
                {
                    FarmlinkChestManager.RemoveChestInBlacklistForLocation(chest.Location, chest.TileLocation);
                }
                else
                {
                    FarmlinkChestManager.AddChestInBlacklistForLocation(chest.Location, chest.TileLocation);
                }
            }
            else if (sItemException is not null)
            {
                Logger.Exception(sItemException);
            }
        }
    }

    internal static class ItemGrabMenuPatch
    {
        private  static          ClickableTextureComponent? chestBlacklistButton;
        internal static          ClickableTextureComponent? ChestBlacklistButton => chestBlacklistButton;
        private  static          FieldInfo                  f_sourceItem        = AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.sourceItem));
        private  static          FieldInfo                  f_xPositionOnScreen = AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.xPositionOnScreen));
        private  static          FieldInfo                  f_width             = AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.width));
        private  static          FieldInfo                  f_yPositionOnScreen = AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.yPositionOnScreen));
        private  static          FieldInfo                  f_height            = AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.height));
        private  static readonly Rectangle                  ToggledOnOffset     = new Rectangle();
        private  static readonly Rectangle                  ToggledOffOffset    = new Rectangle();

        private static Rectangle GetToggledOnOffOffet(ItemGrabMenu __instance)
        {
            return ToggledOn(__instance) ? ToggledOnOffset : ToggledOffOffset;
        }

        internal static bool ToggledOn(ItemGrabMenu __instance)
        {
            if (f_sourceItem.TryGetValue(__instance, out Item? item, out Exception? sItemException) && item is Chest chest)
            {
                return FarmlinkChestManager.HasChestInBlacklistForLocation(chest.Location, chest.TileLocation);
            }
            else if (sItemException is not null)
            {
                Logger.Exception(sItemException);
            }

            return false;
        }

        internal static bool ShouldShowChestBlacklistIcon()
        {
            return ModEntry.IsFarmLinkTerminalPlaced();
        }

        internal static void PatchedDrawMethod(ItemGrabMenu __instance, SpriteBatch b)
        {
            if (__instance.sourceItem is not Chest chest || !chest.playerChest.Get())
                return;

            chestBlacklistButton?.draw(b);
        }

        [SuppressMessage("CodeQuality", "IDE0079"), SuppressMessage("Roslynator", "RCS1102"), SuppressMessage("Roslynator", "RCS1163"), SuppressMessage("Roslynator", "RCS1213")]
        internal static void PatchedCctorMethod(ItemGrabMenu __instance, Item? sourceItem)
        {
            if (sourceItem is not Chest chest || !chest.playerChest.Get())
                return;

            Exception? xPosException = null, widthException = null, yPosException = null, heightException = null;

            if (ShouldShowChestBlacklistIcon() &&
                f_xPositionOnScreen.TryGetValue(__instance, out int xPositionOnScreen, out xPosException) &&
                f_width.TryGetValue(__instance, out int width, out widthException)                        &&
                f_yPositionOnScreen.TryGetValue(__instance, out int yPositionOnScreen, out yPosException) &&
                f_height.TryGetValue(__instance, out int height, out heightException))
            {
                chestBlacklistButton ??= new ClickableTextureComponent(
                name: nameof(chestBlacklistButton),
                bounds: new Rectangle(xPositionOnScreen + width, yPositionOnScreen + (height / 3) - 64 + -216, 64, 64).MoveHorizontal(64 + 24),
                label: "",
                hoverText: I18n.Label_ChestBlacklistButton_Desc(),
                texture: Game1.mouseCursors,
                sourceRect: GetToggledOnOffOffet(__instance),
                scale: 4f,
                drawShadow: false)
                {
                    myID           = 899,
                    leftNeighborID = 898,
                    downNeighborID = 106
                };
            }
            else
            {
                if (xPosException is not null)
                    Logger.Exception(xPosException);
                if (widthException is not null)
                    Logger.Exception(widthException);
                if (yPosException is not null)
                    Logger.Exception(yPosException);
                if (heightException is not null)
                    Logger.Exception(heightException);
            }
        }
    }
}
