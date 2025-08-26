using BepInEx.Bootstrap;
using HarmonyLib;

using MTM101BaldAPI;
using MTM101BaldAPI.Patches;

using ThinkerAPI;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace UncertainLuei.CaudexLib.Patches
{
    /* This patches the Dev API's warning screen patch as the first warning
     * screen is saved to the WarningScreenContainer, leading to the
     * warning screen showing a copy of the vanilla "WARNING!" text after
     * the original.
     */
    [HarmonyPatch(typeof(WarningScreenStartPatch), "Prefix")]
    internal static class DevApiWarningPatch
    {
        private static bool Prefix(WarningScreen __0, ref bool __result)
        {
            __result = false;
            if (ModLoadingScreenManager.doneLoading)
                return true;

            __0.enabled = false;
            __0.textBox.color = Color.clear;
            __0.audSource.Stop();

            if (Chainloader.PluginInfos.ContainsKey("thinkerAPI"))
            {
                new GameObject("ThinkerAPI Warning Loader", typeof(ThinkerApiWarningLoader))
                    .GetComponent<ThinkerApiWarningLoader>().targetScene = __0.scene;
            }
            else
                SceneManager.LoadScene(__0.scene);
            return false;
        }

        private sealed class ThinkerApiWarningLoader : MonoBehaviour
        {
            internal string targetScene;

            private void Update()
            {
                if (thinkerAPI.warningScreenBlockers <= 0 && !thinkerAPI.givemeaheadstart)
                    SceneManager.LoadScene(targetScene);
            }
        }
    }
}
