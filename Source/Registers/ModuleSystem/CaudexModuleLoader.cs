using BepInEx;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.Registers;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UncertainLuei.CaudexLib.Registers.ModuleSystem
{
    public static class CaudexModuleLoader
    {
        internal static Dictionary<Assembly, BaseUnityPlugin> pluginsFromAssembly = [];
        internal static Dictionary<PluginInfo, BaseUnityPlugin> pluginsFromInfo = [];

        internal static Dictionary<PluginInfo, List<AbstractCaudexModule>> pluginModules = [];

        internal static readonly LoadingEventOrder[] loadEventOrders = (LoadingEventOrder[])Enum.GetValues(typeof(LoadingEventOrder));
        internal static readonly GenerationModType[] genModTypes = (GenerationModType[])Enum.GetValues(typeof(GenerationModType));

        public static void LoadAllModules(BaseUnityPlugin plug, Assembly assembly = null)
        {
            if (plug == null)
                throw new ArgumentNullException("plug");

            assembly ??= plug.GetType().Assembly;

            // Grabs the declared PluginInfo                
            if (!pluginsFromAssembly.ContainsKey(assembly))
            {
                pluginsFromAssembly.Add(assembly, plug);
                pluginsFromInfo.Add(plug.Info, plug);
                GetModulesFromAssembly(assembly, plug.Info);
            }
            if (!pluginModules.ContainsKey(plug.Info))
                return;

            foreach (LoadingEventOrder order in loadEventOrders)
            {
                AbstractCaudexModule[] modules = pluginModules[plug.Info].Where(x => x.loadMethods.ContainsKey(order)).ToArray();
                if (modules.Length == 0) continue;
                LoadingEvents.RegisterOnAssetsLoaded(plug.Info, LoadModules(plug.Info, modules, order), order);
            }
            foreach (GenerationModType genModType in genModTypes)
                GeneratorManagement.Register(plug, genModType, (title,no,scene) => LoadGeneratorMods(title, no, scene, plug.Info, genModType));
        }

        private static readonly Type _moduleType = typeof(AbstractCaudexModule);

        private static void GetModulesFromAssembly(Assembly assembly, PluginInfo plug)
        {
            Type[] types;
            object[] attributes;

            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where((Type type) => type != null).ToArray();
            }
            foreach (Type type in types)
            {
                if (type == null) continue;
                if (!type.IsSubclassOf(_moduleType)) continue;

                attributes = type.GetCustomAttributes(typeof(CaudexModule), true);
                if (attributes.Length == 0) continue;
                if (!((CaudexModule)attributes[0]).IsFromPlugin(plug, assembly)) continue;

                AbstractCaudexModule newModule = (AbstractCaudexModule)Activator.CreateInstance(type);
                newModule.TryInitialize();
            }
        }

        private static bool IsFromPlugin(this CaudexModule attribute, PluginInfo info, Assembly assembly)
        {
            string guid = attribute.PluginGuid;
            if (guid.IsNullOrWhiteSpace())
                guid = pluginsFromAssembly[assembly].Info.Metadata.GUID;

            return guid == info.Metadata.GUID;
        }

        private static IEnumerator LoadModules(PluginInfo plug, AbstractCaudexModule[] modules, LoadingEventOrder order)
        {
            yield return modules.Length;
            foreach (AbstractCaudexModule module in modules)
            {
                yield return $"Loading Caudex module \"{module.Info.Name}\"";
                foreach (MethodInfo info in module.loadMethods[order])
                {
                    try
                    {
                        info.Invoke(module, null);
                    }
                    catch (Exception e)
                    {
                        MTM101BaldiDevAPI.CauseCrash(plug, e.InnerException);
                    }
                }
            }
        }

        private static void LoadGeneratorMods(string title, int no, SceneObject scene, PluginInfo plug, GenerationModType genMod)
        {
            object[] args = [title, no, null];
            CustomLevelObject[] lvls = scene.GetCustomLevelObjects();

            foreach (AbstractCaudexModule module in pluginModules[plug])
            {
                if (module.genModMethods.ContainsKey(genMod))
                {
                    args[2] = scene;
                    foreach (MethodInfo sceneMethod in module.genModMethods[genMod])
                    {
                        try
                        {
                            sceneMethod.Invoke(module, args);
                        }
                        catch (Exception e)
                        {
                            MTM101BaldiDevAPI.CauseCrash(plug, e.InnerException);
                        }
                    }
                }
                if (module.genLvlModMethods.ContainsKey(genMod))
                {
                    foreach (MethodInfo lvlMethod in module.genLvlModMethods[genMod])
                    {
                        try
                        {
                            foreach (CustomLevelObject lvl in lvls)
                            {
                                args[2] = lvl;
                                lvlMethod.Invoke(module, args);
                            }
                        }
                        catch (Exception e)
                        {
                            MTM101BaldiDevAPI.CauseCrash(plug, e.InnerException);
                        }
                    }
                }
            }
        }
    }
}
