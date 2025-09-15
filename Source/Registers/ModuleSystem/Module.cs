using System;
using System.Collections.Generic;
using System.Reflection;

using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;

using MTM101BaldAPI;
using MTM101BaldAPI.Registers;

namespace UncertainLuei.CaudexLib.Registers.ModuleSystem
{
    public abstract class AbstractCaudexModule
    {
        protected PluginInfo Plugin => Info.Plugin;
        public CaudexModuleInfo Info { get; }

        protected AbstractCaudexModule()
        {
            Type type = GetType();
            object[] customAttributes = type.GetCustomAttributes(typeof(CaudexModuleInitAttribute), true);

            CaudexModule properties = null;
            CaudexModuleConfig configProperties = null;
            CaudexModuleSaveTag saveProperties = null;
            foreach (var attrib in customAttributes)
            {
                if (properties == null && attrib is CaudexModule modAttrib)
                    properties = modAttrib;
                if (configProperties == null && attrib is CaudexModuleConfig configAttrib)
                    configProperties = configAttrib;
                if (saveProperties == null && attrib is CaudexModuleSaveTag saveAttrib)
                    saveProperties = saveAttrib;
            }
            if (properties == null)
                throw new InvalidOperationException("Cannot instantiate Caudex module " + GetType().FullName + " because the CaudexModule attribute is missing!");

            PluginInfo info;
            if (properties.PluginGuid.IsNullOrWhiteSpace())
                info = CaudexModuleLoader.pluginsFromAssembly[type.Assembly];
            else if (!Chainloader.PluginInfos.TryGetValue(properties.PluginGuid, out info))
                throw new InvalidOperationException("Cannot instantiate Caudex module " + GetType().FullName + " because it is referencing a plugin that is not available!");
            else if (info.Instance.GetType().Assembly != type.Assembly)
                throw new InvalidOperationException("Cannot instantiate Caudex module " + GetType().FullName + " because it is referencing a plugin outside its assembly!");

            ConfigEntry<bool> configEntry = null;
            if (configProperties != null)
            {
                ConfigDefinition def = new(configProperties.Section, configProperties.Key);
                if (!info.Instance.Config.ContainsKey(def))
                {
                    configEntry = info.Instance.Config.Bind(
                    configProperties.Section,
                    configProperties.Key,
                    configProperties.DefaultValue,
                    configProperties.Description);
                }
                else if (info.Instance.Config[def] is ConfigEntry<bool> boolEntry)
                    configEntry = boolEntry;
                else
                    CaudexLibPlugin.Log.LogError($"Caudex module {GetType().FullName} was assigned to a non-boolean config entry!");
            }

            Info = new()
            {
                Name = properties.Name,
                SaveTag = saveProperties?.Value,
                Plugin = info,
                ConfigEntry = configEntry
            };
            Loaded();
        }

        // Runs when a module gets created (ideal for running save data and the like)
        protected virtual void Loaded()
        {
        }

        protected virtual void Initialized()
        {
        }

        internal void TryInitialize()
        {
            if (!Enabled) return;

            if (!CaudexModuleLoader.pluginModules.ContainsKey(Info.Plugin))
                CaudexModuleLoader.pluginModules.Add(Info.Plugin, []);
            CaudexModuleLoader.pluginModules[Info.Plugin].Add(this);

            MethodInfo[] methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            ParameterInfo[] parameters;
            bool isSceneObject;
            foreach (MethodInfo method in methods)
            {
                parameters = method.GetParameters();
                if (parameters.Length == 0)
                {
                    object[] loadAttribs = method.GetCustomAttributes(typeof(CaudexLoadEvent), true);
                    foreach (object attr in loadAttribs)
                        TryAddLoadMethod((CaudexLoadEvent)attr, method);

                    continue;
                }
                if (parameters.Length != 3) continue;
                if (parameters[0].IsOut ||
                    parameters[1].IsOut ||
                    parameters[2].IsOut ||
                    parameters[0].ParameterType != typeof(string) ||
                    parameters[1].ParameterType != typeof(int))
                    continue;

                isSceneObject = true;
                if (parameters[2].ParameterType == typeof(CustomLevelObject) ||
                    parameters[2].ParameterType == typeof(LevelObject))
                    isSceneObject = false;
                else if (parameters[2].ParameterType != typeof(SceneObject))
                    continue;

                object[] genModAttribs = method.GetCustomAttributes(typeof(CaudexGenModEvent), true);
                foreach (object attr in genModAttribs)
                    TryAddGenModMethod((CaudexGenModEvent)attr, method, isSceneObject);
            }

            Initialized();
        }

        private void TryAddLoadMethod(CaudexLoadEvent loadEvent, MethodInfo method)
        {
            foreach (LoadingEventOrder order in CaudexModuleLoader.loadEventOrders)
            {
                if (!loadEvent.ShouldRun(order)) continue;
                
                if (!loadMethods.ContainsKey(order))
                    loadMethods.Add(order, []);
                // Prevent additional triggers of the same method in the load event
                if (!loadMethods[order].Contains(method))
                    loadMethods[order].Add(method);
                return;
            }
        }

        private void TryAddGenModMethod(CaudexGenModEvent genModEvent, MethodInfo method, bool isSceneObject)
        {
            foreach (GenerationModType modType in CaudexModuleLoader.genModTypes)
            {
                if (!genModEvent.ShouldRun(modType)) continue;

                // CustomLevelObject variant
                if (!isSceneObject)
                {
                    if (!genLvlModMethods.ContainsKey(modType))
                        genLvlModMethods.Add(modType, []);
                    // Prevent additional triggers of the same method in the load event
                    if (!genLvlModMethods[modType].Contains(method))
                        genLvlModMethods[modType].Add(method);
                    return;
                }

                // SceneObject variant
                if (!genModMethods.ContainsKey(modType))
                    genModMethods.Add(modType, []);
                if (!genModMethods[modType].Contains(method))
                    genModMethods[modType].Add(method);
                return;
            }
        }

        internal Dictionary<LoadingEventOrder, List<MethodInfo>> loadMethods = [];
        internal Dictionary<GenerationModType, List<MethodInfo>> genModMethods = [];
        internal Dictionary<GenerationModType, List<MethodInfo>> genLvlModMethods = [];

        public virtual bool Enabled => Info.ConfigEntry == null || Info.ConfigEntry.Value;
    }

    public class CaudexModuleInfo
    {
        internal CaudexModuleInfo()
        {
        }

        public string Name { get; internal set; }
        public string SaveTag { get; internal set; }

        public PluginInfo Plugin { get; internal set; }
        public ConfigEntry<bool> ConfigEntry { get; internal set; }
    }
}