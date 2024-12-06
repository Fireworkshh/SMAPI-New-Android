using HarmonyLib;
using StardewValley.Locations;
using StardewValley;
using System;
using System.Reflection;

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
