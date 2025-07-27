using System;
using BepInEx.Bootstrap;
using MTM101BaldAPI.Registers;

namespace UncertainLuei.CaudexLib.Registers.ModuleSystem
{
    public abstract class CaudexModuleInitAttribute : Attribute
    {
    }

    // Regular Caudex init module
    public class CaudexModule(string name) : CaudexModuleInitAttribute
    {
        public CaudexModule(string guid, string name) : this(name)
        {
            PluginGuid = guid;
        }

        public string Name { get; } = name;
        public string PluginGuid { get; }
    }

    public class CaudexModuleConfig(string section, string key, string desc, bool defaultVal = true) : CaudexModuleInitAttribute
    {
        public string Section { get; } = section;
        public string Key { get; } = key;
        public string Description { get; } = desc;
        public bool DefaultValue { get; } = defaultVal;
    }

    public class CaudexModuleSaveTag(string tag) : CaudexModuleInitAttribute
    {
        public string Value { get; } = tag;
    }

    // Loading Events
    public class CaudexLoadEvent(LoadingEventOrder order) : Attribute
    {
        private readonly LoadingEventOrder orderToExecute = order;
        public virtual bool ShouldRun(LoadingEventOrder order) => orderToExecute == order;
    }
    public class CaudexLoadEventMod(string modGuid, LoadingEventOrder order) : CaudexLoadEvent(order)
    {
        private readonly string modGuid = modGuid;
        public override bool ShouldRun(LoadingEventOrder order) => base.ShouldRun(order) && Chainloader.PluginInfos.ContainsKey(modGuid);
    }
    public class CaudexLoadEventNoMod(string modGuid, LoadingEventOrder order) : CaudexLoadEvent(order)
    {
        private readonly string modGuid = modGuid;
        public override bool ShouldRun(LoadingEventOrder order) => base.ShouldRun(order) && !Chainloader.PluginInfos.ContainsKey(modGuid);
    }

    public class CaudexGenModEvent(GenerationModType modType) : Attribute
    {
        private readonly GenerationModType modTypeToExecute = modType;
        public virtual bool ShouldRun(GenerationModType modType) => modTypeToExecute == modType;
    }
}
