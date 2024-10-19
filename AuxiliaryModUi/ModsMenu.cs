using BepInEx;
using BepInEx.Bootstrap;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace UncertainLuei.BaldiPlus.ModsUi
{
    public class ModsListMenu : Singleton<ModsListMenu>
    {
        private GameObject lastMenu;

        public TMP_Text infName;
        public TMP_Text infVer;
        public TMP_Text infGuid;
        public TMP_Text infCredit;
        public TMP_Text infAbout;

        private byte currentPage = 0;
        private ModEntry currentEntry;

        public override void AwakeFunction()
        {
            StandardMenuButton backButton = transform.Find("Base/BackButton").GetComponent<StandardMenuButton>();

            // Override back button's current events with our desired functionality
            backButton.OnPress = new UnityEvent();
            backButton.OnPress.AddListener(Close);
        }

        private void UpdateModPage(ModEntry entry)
        {
            if (currentEntry == entry) return;

            currentEntry = entry;

            infName.text = currentEntry.name;
            infVer.text = $"v{currentEntry.version}, {currentEntry.license}";
            infGuid.text = currentEntry.guid;
            
            infCredit.text = $"<b>Author(s):</b> {currentEntry.authors}" + (entry.credits.IsNullOrWhiteSpace() ? "" : $"\n<b>Credits:</b> {currentEntry.credits}");
            infAbout.text = currentEntry.description;
        }

        public static void Open(GameObject canvas)
        {
            canvas.SetActive(false);

            if (Instance)
            {
                Instance.lastMenu = canvas;
                Instance.gameObject.SetActive(true);
                Instance.UpdateModPage(ModUiPlugin.modEntries[Mathf.FloorToInt(UnityEngine.Random.value * ModUiPlugin.modEntries.Count)]);

                return;
            }

            if (!ModUiPlugin.assetMan.ContainsKey("ModsMenu"))
                MTM101BaldiDevAPI.CauseCrash(ModUiPlugin.instance.Info, new NullReferenceException("Could not find ModsMenu blueprint!"));

            ModsListMenu newMenu = ModUiPlugin.assetMan.Get<ModsListMenu>("ModsMenu");
            newMenu = Instantiate(newMenu);
            newMenu.lastMenu = canvas;

            newMenu.UpdateModPage(ModUiPlugin.modEntries[Mathf.FloorToInt(UnityEngine.Random.value * ModUiPlugin.modEntries.Count)]);
        }

        private void Close()
        {
            if (lastMenu)
                lastMenu.SetActive(true);

            gameObject.SetActive(false);
        }
    }

    public class ModEntry
    {
        public ModEntry(PluginInfo info)
        {
            name = info.Metadata.Name;
            guid = info.Metadata.GUID;
            version = info.Metadata.Version.ToString();

            assetsPath = AssetLoader.GetModPath(info.Instance);
        }

        public string name;
        public string guid;
        public string version;

        public string license = "License unset";
        public string description = "No description set.";

        public string authors = "";
        public string credits = "";

        private string assetsPath;

        public void OpenAssetsPath()
        {
            if (!assetsPath.IsNullOrWhiteSpace())
                Application.OpenURL(assetsPath);
        }
    }
}
