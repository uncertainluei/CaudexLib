using BepInEx;
using System.Collections.Generic;

namespace UncertainLuei.CaudexLib.Registers.ModuleSystem
{
    public static class CaudexModuleHelper
    {
        public static AbstractCaudexModule[] GetActiveCaudexModules(this PluginInfo plug)
        {
            if (!CaudexModuleLoader.pluginModules.ContainsKey(plug))
            {
                CaudexLibPlugin.Log.LogError($"Could not get modules for plugin {plug}! Make sure you are running this after all modules are loaded!");
                return [];
            }
            return CaudexModuleLoader.pluginModules[plug].ToArray();
        }

        public static string[] GetActiveCaudexModuleTags(this PluginInfo plug)
        {
            if (!CaudexModuleLoader.pluginModules.ContainsKey(plug))
            {
                CaudexLibPlugin.Log.LogError($"Could not get module save tags for plugin {plug}! Make sure you are running this after all modules are loaded!");
                return [];
            }

            List<string> tags = [];
            foreach (AbstractCaudexModule module in CaudexModuleLoader.pluginModules[plug])
                if (!module.Info.SaveTag.IsNullOrWhiteSpace())
                    tags.Add(module.Info.SaveTag);
            return tags.ToArray();
        }
    }
}
