using HarmonyLib;
using StardewValley.Locations;
using StardewValley;
using System;
using System.Reflection;
using StardewValley.Monsters;

namespace StardewModdingAPI
{
    public static class PatchManager
    {
        public static void ApplyAllPatches()
        {
            try
            {
                ApplyTypeDefinitionPatches();
                
                ApplyResetLocalStatePatch();
                ApplyLocationsFieldPatch();

                Console.WriteLine("All patches applied successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying patches: {ex.Message}");
            }
        }

        [HarmonyPatch]
        public class HarmonyPatch_OptimizeMonsterCode
        {
            // 目标是清空 ApplyPatch 方法的执行，所以我们使用 Prefix 来提前终止方法
            [HarmonyPrefix]
            [HarmonyPatch("FarmTypeManager.ModEntry+HarmonyPatch_OptimizeMonsterCode", "ApplyPatch")]
            public static bool Prefix()
            {
                // 返回 false 来阻止原方法执行
                Console.WriteLine("已阻止 Harmony 补丁 'HarmonyPatch_OptimizeMonsterCode.ApplyPatch' 的执行");
                return false; // false 表示阻止原方法执行
            }
        }
        private static void ApplyTypeDefinitionPatches()
        {
            Harmony harmony = new Harmony("com.example.patch");
            MethodInfo original = AccessTools.Method(typeof(ItemRegistry), "AddTypeDefinition");
            HarmonyMethod prefix = new HarmonyMethod(typeof(GameMainActivity), "AddTypeDefinition_Prefix");
            harmony.Patch(original, prefix);
            Console.WriteLine("Prefix applied to AddTypeDefinition.");
        }
        
        private static void ApplyResetLocalStatePatch()
        {
            Harmony harmony = new Harmony("com.example.patch");
            MethodInfo original = AccessTools.Method(typeof(DecoratableLocation), "resetLocalState");
            HarmonyMethod prefix = new HarmonyMethod(typeof(GameMainActivity), "ResetLocalState_Prefix");
            harmony.Patch(original, prefix);
            Console.WriteLine("Patch applied to resetLocalState.");
        }

        private static void ApplyLocationsFieldPatch()
        {
            Harmony harmony = new Harmony("com.example.patch");
            MethodInfo getterMethod = AccessTools.PropertyGetter(typeof(Game1), "_locations");
            if (getterMethod != null)
            {
                HarmonyMethod prefix = new HarmonyMethod(typeof(GameMainActivity), "ModifyLocationsField");
                harmony.Patch(getterMethod, prefix);
                Console.WriteLine("Patch applied to modify _locations field.");
            }
            else
            {
                Console.WriteLine("_locations getter method not found in Game1 class.");
            }
        }
    }
}
