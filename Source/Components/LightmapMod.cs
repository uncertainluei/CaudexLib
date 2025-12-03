using UnityEngine;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace UncertainLuei.CaudexLib.Components
{
    public delegate void LightmapModAction(ref Color color);

    public struct LightmapMod(LightmapModAction action, sbyte priority = 0)
    {
        private readonly LightmapModAction action = action; // Determines the action of the lightmap modifier
        public sbyte priority = priority; // Holds the priority of the lightmap modifier, determining the execution order

        public readonly void Invoke(ref Color color) => action(ref color);

        // Additional constructors for common actions
        public static LightmapMod Multiplier(Color multiplier, sbyte priority = 0)
            => new((ref Color color) => color *= multiplier, priority);
        public static LightmapMod Multiplier(float multiplier, sbyte priority = 0)
            => new((ref Color color) => color *= multiplier, priority);
    }

    public class LightmapModHolder : MonoBehaviour
    {
        internal static readonly Dictionary<EnvironmentController, LightmapModHolder> _instances = [];
        private static EnvironmentController _ec;
        private static LightmapModHolder _instance;
        public static LightmapModHolder GetInstance(EnvironmentController ec)
        {
            if (_ec == ec && _instance)
                return _instance;

            _ec = ec;
            if (_instances.ContainsKey(ec))
                _instance = _instances[ec];
            else
            {
                _instance = ec.GetComponent<LightmapModHolder>();
                _instance.Environment = ec;
                _instances.Add(ec, _instance);
            }
            return _instance;
        }

        public EnvironmentController Environment { get; internal set; }
        private readonly List<LightmapMod> modifiers = [];

        public void Add(LightmapMod modifier, bool update = true)
        {
            int i, c;
            for (i = 0, c = modifiers.Count; i < c; i++)
            {
                if (modifiers[i].priority > modifier.priority)
                    break;
            }
            modifiers.Insert(i, modifier);

            // Set update to false if you are adding MULTIPLE of these
            if (update)
                ForceUpdateLightmap();
        }

        public void Remove(LightmapMod modifier, bool update = true)
        {
            if (!modifiers.Contains(modifier)) return;
            modifiers.Remove(modifier);

            // Set update to false if you are removing MULTIPLE of these
            if (update)
                ForceUpdateLightmap();
        }

        // Updates the ENTIRE lightmap.
        public void ForceUpdateLightmap()
        {
            for (int i = 0; i < Environment.levelSize.x; i++)
                for (int j = 0; j < Environment.levelSize.z; j++)
                    if (Environment.lightMap[i,j] != null)
                        Environment.QueueLightControllerForUpdate(Environment.lightMap[i,j]);
        }

        internal void Invoke(ref Color color)
        {
            foreach (var modifier in modifiers)
                modifier.Invoke(ref color);
        }

        private void OnDestroy()
        {
            if (Environment && _instances.ContainsKey(Environment))
                _instances.Remove(Environment);
        }
    }
}