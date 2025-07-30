using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MTM101BaldAPI;

namespace UncertainLuei.CaudexLib.Util.Extensions
{
    public static class CaudexHarmonyExtensions
    {
        private static readonly List<Action> queuedHarmonyActions = [];

        internal static void RunQueuedActions()
        {
            foreach (Action action in queuedHarmonyActions)
                action();
            queuedHarmonyActions.Clear();
        }

        // Queues PatchAll when Caudex is done loading all plugins
        public static void QueuePatchAll(Harmony _harmony, Assembly assembly = null)
        {
            assembly ??= Assembly.GetCallingAssembly();
            queuedHarmonyActions.Add(() => _harmony.PatchAll(assembly));
        }

        // Queues PatchAll (type) when Caudex is done loading all plugins
        public static void QueuePatchAll(Harmony _harmony, Type type)
            => queuedHarmonyActions.Add(() => _harmony.PatchAll(type));

        // Queues Patch when Caudex is done loading all plugins
        public static void QueuePatch(Harmony _harmony, MethodBase original, HarmonyMethod prefix = null, HarmonyMethod postfix = null, HarmonyMethod transpiler = null, HarmonyMethod finalizer = null, HarmonyMethod ilmanipulator = null)
            => queuedHarmonyActions.Add(() => _harmony.Patch(original, prefix, postfix, transpiler, finalizer, ilmanipulator));

        // Queues PatchAllConditionals when Caudex is done loading all plugins
        public static void PatchAllConditionals(Harmony _harmony, Assembly assembly = null)
        {
            assembly ??= Assembly.GetCallingAssembly();
            queuedHarmonyActions.Add(() => _harmony.PatchAllConditionals(assembly));
        }
    }
}
