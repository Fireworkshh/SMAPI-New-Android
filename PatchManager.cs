using HarmonyLib;
using StardewValley.Locations;
using StardewValley;
using System;
using System.Reflection;
using StardewValley.Monsters;
using SMAPIStardewValley;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using StardewValley.BellsAndWhistles;
using Microsoft.Xna.Framework;

namespace StardewModdingAPI
{
    public static class PatchManager
    {
        public static void ApplyAllPatches()
        {
            try
            {
              
                
                ApplyPatch();
                ApplyOptionsElementDrawPatch();
               // ApplyResetLocalStatePatch();
               // ApplyLocationsFieldPatch();

                Console.WriteLine("All patches applied successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying patches: {ex.Message}");
            }
        }
        public static void ApplyPatch()
        {
            try
            {

                Harmony harmony = new Harmony("com.example.patch");
                //   Utility.Monitor.Log($"Applying Harmony patch \"{nameof(HarmonyPatch_OptimizeMonsterCode)}\": prefixing SDV method \"Monster.findPlayer\".", LogLevel.Trace);
                harmony.Patch(
                    original: AccessTools.Method(typeof(Monster), "findPlayer", new Type[] { }),
                    prefix: new HarmonyMethod(typeof(PatchManager), nameof(Monster_findPlayer_Prefix))
                );

                //    Utility.Monitor.Log($"Applying Harmony patch \"{nameof(HarmonyPatch_OptimizeMonsterCode)}\": postfixing SDV method \"Monster.findPlayer\".", LogLevel.Trace);
                harmony.Patch(
                    original: AccessTools.Method(typeof(Monster), "findPlayer", new Type[] { }),
                    postfix: new HarmonyMethod(typeof(PatchManager), nameof(Monster_findPlayer_Postfix))
                );
        }
            catch (Exception ex)
        {
                //    Utility.Monitor.LogOnce($"Harmony patch \"{nameof(HarmonyPatch_OptimizeMonsterCode)}\" failed to apply. Monsters might slow the game down or cause errors. Full error message: \n{ex.ToString()}", LogLevel.Error);
            }
        }
        

 
      
        private static bool Monster_findPlayer_Prefix(ref Farmer __result)
        {
            try
            {
                if (!Context.IsMultiplayer) //if this is NOT a multiplayer session
                {
                    __result = Game1.player; //return the current player
                    return false; //skip the original method
    }
                else
                    return true; //call the original method
}
            catch (Exception ex)
            {
                //  Utility.Monitor.LogOnce($"Harmony patch \"{nameof(Monster_findPlayer_Prefix)}\" has encountered an error. Monsters might cause the game to run slower in single-player mode. Full error message: \n{ex.ToString()}", LogLevel.Error);
                return true; //call the original method
            }
        }
        public static void ApplyOptionsElementDrawPatch()
        {
            try
            {
                Harmony harmony = new Harmony("com.example.patch");

                // 为了避免直接覆盖原始 draw 方法，我们使用 Harmony 来前置（Prefix）补丁。
                harmony.Patch(
                    original: AccessTools.Method(typeof(OptionsElement), "draw", new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(IClickableMenu) }),
                    prefix: new HarmonyMethod(typeof(PatchManager), nameof(OptionsElement_draw_Prefix))
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying OptionsElement draw patch: {ex.Message}");
            }
        }

        private static bool OptionsElement_draw_Prefix(SpriteBatch b, int slotX, int slotY, IClickableMenu context)
        {
            // 首先，我们可以安全地从 `context` 强制转换为 `OptionsElement`，并检查是否为空
            OptionsElement optionsElement = context as OptionsElement;

         
            try
            {
                // 在此处，我们通过直接访问 `this` 来操作 OptionsElement 实例
                if (optionsElement.whichOption == -1)
                {
                    // 自定义的绘制逻辑
                    SpriteText.drawString(b, optionsElement.label, slotX + optionsElement.bounds.X, slotY + optionsElement.bounds.Y, 999, -1, 999, 1f, 0.1f, false, -1, "", null, SpriteText.ScrollTextAlignment.Left);
                    return false; // 返回 false 来跳过原始的 `draw` 方法
                }

                // 调用原始的绘制方法，但我们可以修改绘制的一些参数
                Utility.drawTextWithShadow(b, optionsElement.label, Game1.dialogueFont, new Vector2(slotX + optionsElement.bounds.X, slotY + optionsElement.bounds.Y),
                    optionsElement.greyedOut ? Game1.textColor * 0.33f : Game1.textColor, 1f, 0.1f, -1, -1, 1f, 3);

                // 返回 true 以允许继续执行原始的 `draw` 方法
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OptionsElement_draw_Prefix: {ex.Message}");
                return true; // 如果发生错误，继续执行原始的 `draw` 方法
            }
        }
    
    /// <summary>Attempts to avoid a bug where monsters occasionally crash the game due to a null "Monster.Player" during "Monster.behaviorAtGameTick".</summary>
    /// <remarks>
    /// I cannot yet naturally reproduce this bug, but it has been reported by several players.
    /// atravita found that the null errors occurred in "!Player.isRafting" and fixed the issue by transpiling a null check into that code. If errors persist, consider that solution.
    /// This postfix solution should address issues in other code that reference "Monster.Player" as well. Most of the game's code assumes it cannot be null.</remarks>
    private static void Monster_findPlayer_Postfix(ref Farmer __result)
        {
            try
            {
                if (__result == null) //if this method failed to return a farmer (possible due to other mods' patches, multiplayer/threading issues, etc)
                {
                    __result = Game1.player; //assign the local player (should never be null because it causes immediate crashes in most contexts, but may still be possible)

                    if (__result == null) //if the result is somehow still null
                    {
                        //      Utility.Monitor.LogOnce($"Monster.findPlayer and Game1.player both returned null. If errors occur, please share your full log file with this mod's developer.", LogLevel.Debug);
                        return;
                    }
                    else
                    {
                        //        Utility.Monitor.LogOnce($"Monster.findPlayer returned null. Using Game1.player instead.", LogLevel.Trace);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                // Utility.Monitor.LogOnce($"Harmony patch \"{nameof(Monster_findPlayer_Postfix)}\" has encountered an error. Full error message: \n{ex.ToString()}", LogLevel.Error);
                return; //call the original method
            }
        }
    }
}
    
