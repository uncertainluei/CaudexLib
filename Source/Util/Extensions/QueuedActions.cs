using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MTM101BaldAPI;

namespace UncertainLuei.CaudexLib.Util.Extensions
{
    public static class QueuedActions
    {
        private static readonly List<Action> queuedActions = [];

        internal static void RunQueuedActions()
        {
            foreach (Action action in queuedActions)
                action.Invoke();
            queuedActions.Clear();
        }

        public static void QueueAction(Action action)
            => queuedActions.Add(action);

        // Queues PatchAll when Caudex is done loading all plugins
        public static void QueuePatchAll(this Harmony _harmony, Assembly assembly = null)
        {
            assembly ??= Assembly.GetCallingAssembly();
            queuedActions.Add(() => _harmony.PatchAll(assembly));
        }

        // Queues PatchAll (type) when Caudex is done loading all plugins
        public static void QueuePatchAll(this Harmony _harmony, Type type)
            => queuedActions.Add(() => _harmony.PatchAll(type));

        // Queues Patch when Caudex is done loading all plugins
        public static void QueuePatch(this Harmony _harmony, MethodBase original, HarmonyMethod prefix = null, HarmonyMethod postfix = null, HarmonyMethod transpiler = null, HarmonyMethod finalizer = null, HarmonyMethod ilmanipulator = null)
            => queuedActions.Add(() => _harmony.Patch(original, prefix, postfix, transpiler, finalizer, ilmanipulator));

        // Queues PatchAllConditionals when Caudex is done loading all plugins
        public static void QueuePatchAllConditionals(this Harmony _harmony, Assembly assembly = null)
        {
            assembly ??= Assembly.GetCallingAssembly();
            queuedActions.Add(() => _harmony.PatchAllConditionals(assembly));
        }
    }
}
