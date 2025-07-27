using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UncertainLuei.CaudexLib.Registers
{
    public static class SplashLogos
    {
        public static void AddLogo(Texture2D tex, Vector2 size, float dur = 2.5f)
        {
            logos.Add(new(tex, size, dur));
        }
        public static void AddLogo(Texture2D tex, float dur = 2.5f)
        {
            Vector2 size = new(tex.width, tex.height);
            if (size.x <= 1920)
                size *= 1920 / tex.width;
            else
                size *= 1920.0f / tex.width;
            AddLogo(tex, size, dur);
        }

        private static readonly List<SplashLogo> logos = [];
        private struct SplashLogo(Texture2D tex, Vector2 size, float dur)
        {
            [Range(0.5f,2.5f)]
            internal float duration = dur;

            internal Texture2D image = tex;
            internal Vector2 size = size;
        }

        internal static void SceneLoading(Scene scene, LoadSceneMode mode)
        {
            if (scene == null ||
                scene.name != "Logo" ||
                mode != LoadSceneMode.Single) return;

            SceneTimer timer = GameObject.FindObjectOfType<SceneTimer>();
            timer?.StartLogoDisplay();
        }

        internal static void StartLogoDisplay(this SceneTimer timer)
        {
            if (logos.Count > 0)
                timer.StartCoroutine(DisplayLogos(timer));
        }

        private static IEnumerator DisplayLogos(SceneTimer timer)
        {
            RawImage img = timer.GetComponentInChildren<RawImage>();
            RectTransform imgTrans = img.rectTransform;

            timer.enabled = false;
            float time = 2.5f;

            for (; time > 0f; time -= Time.unscaledDeltaTime)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                    timer.Stop();

                yield return null;
            }
            foreach (SplashLogo logo in logos)
            {
                img.texture = logo.image;
                imgTrans.sizeDelta = logo.size;
                for (time = logo.duration; time > 0f; time -= Time.unscaledDeltaTime)
                {
                    if (Input.GetKeyDown(KeyCode.Escape))
                        timer.Stop();

                    yield return null;
                }
            }
            timer.Stop();
        }

        private static void Stop(this SceneTimer timer)
        {
            timer.StopAllCoroutines();
            timer.time = 0f;
            timer.Update();
        }
    }
}
