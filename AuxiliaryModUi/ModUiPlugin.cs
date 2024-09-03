using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.OptionsAPI;
using MTM101BaldAPI.Reflection;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LuisRandomness.BBPAuxiliaryModUi
{
    [BepInPlugin(ModGuid, "Auxiliary Mod UI", ModVersion)]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi", BepInDependency.DependencyFlags.HardDependency)]
    public class ModUiPlugin : BaseUnityPlugin
    {
        public const string ModGuid = "io.github.luisrandomness.auxiliarymodui";
        public const string ModVersion = "2024.1.0.0";

        internal static List<ModEntry> modEntries = new List<ModEntry>();

        internal static ModUiPlugin instance;
        internal static AssetManager assetMan = new AssetManager();

        // CONFIGURATION

        private void Awake()
        {
            instance = this;

            InitConfigValues();

            LoadingEvents.RegisterOnAssetsLoaded(OnLoadPost, true);

            new Harmony(ModGuid).PatchAllConditionals();
        }

        private void GrabAllModEntries()
        {
            if (modEntries.Count > 0)
                return;

            foreach (PluginInfo info in Chainloader.PluginInfos.Values)
            {
                ModEntry entry = new ModEntry(info);
                Debug.Log($"{entry.guid} v{entry.version}");
                modEntries.Add(entry);
            }
        }

        void InitConfigValues()
        {
        }

        void OnLoadPost()
        {
            // Guarantees it'll run when all mods are loaded
            GrabAllModEntries();

            assetMan.Add("ModsButton", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "MenuButton", "Mods_Unlit.png"), 100f));
            assetMan.Add("ModsButtonHighlight", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "MenuButton", "Mods_Lit.png"), 100f));

            assetMan.Add("Bg", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "ModUi", "ModsBgRef.png"), 100f));

            // Misc. buttons
            assetMan.Add("Btn_AssetsFolder", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "ModUi", "Buttons", "AssetsFolder.png"), 100f));
            assetMan.Add("Btn_Config", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "ModUi", "Buttons", "Config.png"), 100f));
            assetMan.Add("Btn_Update", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "ModUi", "Buttons", "Update.png"), 100f));
            assetMan.Add("Btn_UpdateUnavailable", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "ModUi", "Buttons", "UpdateUnavailable.png"), 100f));
            assetMan.Add("Btn_Website", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "ModUi", "Buttons", "Website.png"), 100f));
            assetMan.Add("Btn_Issues", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "ModUi", "Buttons", "Issues.png"), 100f));
            assetMan.Add("Btn_License", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "ModUi", "Buttons", "License.png"), 100f));

            OptionsMenu options = Resources.FindObjectsOfTypeAll<OptionsMenu>()?.FirstOrDefault();
            options = Instantiate(options);
            options.name = "ModsMenu";

            Transform optTransform = options.transform;
            GameObject optMenu = options.gameObject;

            Destroy(options);
            ModsListMenu menu = optMenu.AddComponent<ModsListMenu>();
            menu.ReflectionSetVariable("destroyOnLoad", true);

            foreach (Transform child in optTransform)
            {
                if (child.name == "Bottom")
                    break;
                if (child.name == "TooltipBase")
                    continue;

                if (child.name != "Base")
                {
                    Destroy(child.gameObject);
                    continue;
                }

                foreach (Transform child2 in child)
                {
                    if (child2.name == "BackButton")
                        continue;
                    if (child2.name != "BG")
                    {
                        Destroy(child2.gameObject);
                        continue;
                    }

                    child2.GetComponent<Image>().sprite = assetMan.Get<Sprite>("Bg");
                }

                TMP_Text txt = UIHelpers.CreateText<TextMeshProUGUI>(BaldiFonts.BoldComicSans24, "Mod Name", child, new Vector3(112f, 160f));
                txt.name = "Info_Name";
                txt.rectTransform.sizeDelta = new Vector2(240f,36f);
                txt.enableAutoSizing = true;
                txt.fontSizeMax = 24f;
                txt.fontSizeMin = 18f;
                txt.overflowMode = TextOverflowModes.Ellipsis;
                txt.alignment = TextAlignmentOptions.Left;
                txt.color = Color.black;
                menu.infName = txt;

                txt = UIHelpers.CreateText<TextMeshProUGUI>(BaldiFonts.ComicSans12, "v0.0.0.0, No License", child, new Vector3(112f, 140f));
                txt.name = "Info_Version";
                txt.rectTransform.sizeDelta = new Vector2(240f,24f);
                txt.alignment = TextAlignmentOptions.Left;
                txt.color = Color.black;
                menu.infVer = txt;

                txt = Instantiate(txt, child);
                txt.name = "Info_Guid";
                txt.text = "Info_Guid";
                txt.rectTransform.anchoredPosition = new Vector2(112f, 124f);
                txt.color = Color.gray;
                menu.infGuid = txt;

                txt = Instantiate(txt, child);
                txt.name = "Info_Credits";
                txt.text = "Info_Credits";
                txt.rectTransform.sizeDelta = new Vector2(240f,56f);
                txt.rectTransform.anchoredPosition = new Vector2(112f, 48f);
                txt.alignment = TextAlignmentOptions.TopLeft;
                txt.color = Color.black;
                menu.infCredit = txt;

                txt = Instantiate(txt, child);
                txt.name = "Info_About";

                // PLACEHOLDER TEXT, JUST SO I CAN TEST THIS
                txt.text = "Info_About";
                
                txt.overflowMode = TextOverflowModes.Truncate;
                txt.rectTransform.sizeDelta = new Vector2(240f, 192f);
                txt.rectTransform.anchoredPosition = new Vector2(112f, -80f);
                menu.infAbout = txt;
            }

            DontDestroyOnLoad(optMenu);

            assetMan.Add("ModsMenu", menu);
        }
    }

    [HarmonyPatch(typeof(MainMenu))]
    [HarmonyPatch("Start")]
    class MainMenuButton
    {
        static void Postfix(MainMenu __instance)
        {
            StandardMenuButton oB = __instance.transform.Find("Options").GetComponent<StandardMenuButton>();

            StandardMenuButton mB = Object.Instantiate(oB, __instance.transform);
            mB.name = "Mods";
            mB.unhighlightedSprite = ModUiPlugin.assetMan.Get<Sprite>("ModsButton");
            mB.highlightedSprite = ModUiPlugin.assetMan.Get<Sprite>("ModsButtonHighlight");

            mB.image.sprite = mB.unhighlightedSprite;
            mB.image.SetNativeSize();
            RectTransform rect = (RectTransform)mB.transform;
            rect.anchoredPosition = new Vector2(112f, -96f);
            rect.SetSiblingIndex(oB.transform.GetSiblingIndex() + 1);

            mB.OnPress = new UnityEngine.Events.UnityEvent();
            mB.OnPress.AddListener(() => { __instance.gameObject.SetActive(false); ModsListMenu.Open(__instance.gameObject); });

        }
    }
}