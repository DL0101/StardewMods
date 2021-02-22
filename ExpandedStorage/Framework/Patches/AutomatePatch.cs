﻿using System;
using System.Linq;
using Harmony;
using ImJustMatt.ExpandedStorage.Framework.Integrations;
using ImJustMatt.Common.PatternPatches;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

// ReSharper disable InvertIf
// ReSharper disable InconsistentNaming

namespace ImJustMatt.ExpandedStorage.Framework.Patches
{
    internal class AutomatePatch : Patch<ModConfig>
    {
        private static IReflectionHelper _reflection;
        private readonly bool _isAutomateLoaded;
        private readonly Type _type;

        internal AutomatePatch(IMonitor monitor, ModConfig config, IReflectionHelper reflection, bool isAutomateLoaded)
            : base(monitor, config)
        {
            _reflection = reflection;
            _isAutomateLoaded = isAutomateLoaded;

            if (!isAutomateLoaded)
                return;

            var automateAssembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.FullName.StartsWith("Automate,"));
            _type = automateAssembly.GetType("Pathoschild.Stardew.Automate.Framework.Storage.ChestContainer");
        }

        protected internal override void Apply(HarmonyInstance harmony)
        {
            if (_isAutomateLoaded)
            {
                Monitor.Log("Patching Automate for Restricted Storage");
                var methodInfo = AccessTools.GetDeclaredMethods(_type)
                    .Find(m => m.Name.Equals("Store", StringComparison.OrdinalIgnoreCase));
                harmony.Patch(methodInfo, new HarmonyMethod(GetType(), nameof(Store_Prefix)));
            }
        }

        public static bool Store_Prefix(object __instance, ITrackedStack stack)
        {
            var reflectedChest = _reflection.GetField<Chest>(__instance, "Chest");
            var reflectedSample = _reflection.GetProperty<Item>(stack, "Sample");
            var config = ExpandedStorage.GetConfig(reflectedChest.GetValue());
            return config == null || config.Filter(reflectedSample.GetValue());
        }
    }
}