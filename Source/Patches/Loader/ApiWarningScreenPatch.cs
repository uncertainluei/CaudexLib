using System.Collections;
using BepInEx.Bootstrap;
using HarmonyLib;

using MTM101BaldAPI;
using MTM101BaldAPI.Patches;

using ThinkerAPI;

using UnityEngine;

namespace UncertainLuei.CaudexLib.Patches
{
    /* This patches the Dev API's warning screen patch as the first warning
     * screen is saved to the WarningScreenContainer, leading to the
     * warning screen showing a copy of the vanilla "WARNING!" text after
     * the original.
     */
    [ConditionalPatchConfig(CaudexLibPlugin.ModGuid, "LoadingScreen", "ChangeWarningScreenOrder")]
    [HarmonyPatch(typeof(WarningScreenStartPatch), "Prefix")]
    internal static class ApiWarningPatch
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
                __0.StartCoroutine(LoadSceneDelayedThinkerApi(__0.scene));
            else
                __0.StartCoroutine(LoadSceneDelayed(__0.scene));
            return false;
        }

        private static IEnumerator LoadSceneDelayed(string target)
        {
            yield return new WaitWhile(() => AdditiveSceneManager.Instance.Busy);
            AdditiveSceneManager.Instance.LoadScene(target);
        }

        private static IEnumerator LoadSceneDelayedThinkerApi(string target)
        {
            yield return new WaitWhile(() => AdditiveSceneManager.Instance.Busy);
            yield return new WaitWhile(() => thinkerAPI.warningScreenBlockers > 0 || thinkerAPI.givemeaheadstart);
            AdditiveSceneManager.Instance.LoadScene(target);
        }
    }
}
