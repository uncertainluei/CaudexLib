using BepInEx;

using MTM101BaldAPI;
using MTM101BaldAPI.Registers;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;

namespace UncertainLuei.CaudexLib.Registers.ModuleSystem
{
    internal static class CaudexModuleLoader
    {
        internal static Dictionary<Assembly, PluginInfo> pluginsFromAssembly = [];
        internal static Dictionary<PluginInfo, List<AbstractCaudexModule>> pluginModules = [];

        internal static readonly LoadingEventOrder[] loadEventOrders = (LoadingEventOrder[])Enum.GetValues(typeof(LoadingEventOrder));
        internal static readonly GenerationModType[] genModTypes = (GenerationModType[])Enum.GetValues(typeof(GenerationModType));

        internal static void GetModulesFromPlugin(PluginInfo plug)
        {
            Assembly assembly = plug.Instance.GetType().Assembly;

            /* Allows Caudex Lib's module system to grab the first loaded plugin in the assembly
             * if not manually provided.
             */
            if (!pluginsFromAssembly.ContainsKey(assembly))
            {
                pluginsFromAssembly.Add(assembly, plug);
                GetModulesFromAssembly(assembly);
            }
            if (!pluginModules.ContainsKey(plug))
                return;

            foreach (LoadingEventOrder order in loadEventOrders)
            {
                AbstractCaudexModule[] modules = pluginModules[plug].Where(x => x.loadMethods.ContainsKey(order)).ToArray();
                if (modules.Length == 0) continue;
                LoadingEvents.RegisterOnAssetsLoaded(plug, LoadModules(plug, modules, order), order);
            }
            foreach (GenerationModType genModType in genModTypes)
                GeneratorManagement.Register(plug.Instance, genModType, (title,no,scene) => LoadGeneratorMods(title, no, scene, plug, genModType));
        }

        private static void GetModulesFromAssembly(Assembly assembly)
        {
            Type moduleType = typeof(AbstractCaudexModule);

            Type[] types = assembly.GetExportedTypes();
            foreach (Type type in types)
            {
                if (!type.IsSubclassOf(moduleType)) continue;
                if (type.GetCustomAttributes(typeof(CaudexModule),true).Length == 0) continue;
                AbstractCaudexModule newModule = (AbstractCaudexModule)Activator.CreateInstance(type);
                newModule.TryInitialize();
            }
        }

        private static IEnumerator LoadModules(PluginInfo plug, AbstractCaudexModule[] modules, LoadingEventOrder order)
        {
            Debug.Log(modules.Length);
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
