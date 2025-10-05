using System.Collections;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UncertainLuei.CaudexLib.Patches
{
    [HarmonyPatch(typeof(ModLoadingScreenManager))]
    internal static class ApiLoadingScreenPatches
    {
        private static GameObject _blackScreen;

        [HarmonyPatch(typeof(MenuInitializer), "Start"), HarmonyPostfix]
        private static void OnMenuLoad()
        {
            if (MTM101BaldiDevAPI.CalledInitialize) return;
            MTM101BaldiDevAPI.CalledInitialize = true;

            Canvas blackCanvas = UIHelpers.CreateBlankUIScreen("BlackScreen", false, true);
            Image black = new GameObject("Black", typeof(Image)).GetComponent<Image>();
            black.color = CaudexLibPlugin.darkModeLoadingScreen.Value ? Color.black : Color.white;
            black.transform.SetParent(blackCanvas.transform, false);
            black.rectTransform.anchoredPosition = Vector2.zero;
            black.rectTransform.anchorMin = Vector2.zero;
            black.rectTransform.anchorMax = Vector2.one;
            black.rectTransform.pivot = Vector2.one / 2f;

            _blackScreen = blackCanvas.gameObject;
        }

        [HarmonyPatch("Start"), HarmonyPostfix]
        private static void Start(ModLoadingScreenManager __instance)
        {
            if (CaudexLibPlugin.darkModeLoadingScreen.Value)
            {
                TMP_Text[] texts = __instance.transform.GetComponentsInChildren<TextMeshProUGUI>();
                foreach (TMP_Text txt in texts)
                    txt.color = Color.white;

                __instance.GetComponent<Image>().color = Color.black;
            }
            GameObject.DestroyImmediate(_blackScreen);
        }

        [HarmonyPatch("LoadingEnded"), HarmonyPrefix]
        private static bool OnComplete(ModLoadingScreenManager __instance)
        {
            if (!CaudexLibPlugin.changeWarningScreenOrder.Value)
                return true;

            GlobalCam.Instance.Transition(UiTransition.Dither, 1 / 30f);

            foreach (Transform child in __instance.transform)
                GameObject.Destroy(child.gameObject);
            __instance.StartCoroutine(WaitForTransition());
            return false;
        }

        private static IEnumerator WaitForTransition()
        {
            yield return new WaitWhile(() => GlobalCam.Instance.TransitionActive);
            AdditiveSceneManager.Instance.LoadScene("Warnings");
        }
    }
}
