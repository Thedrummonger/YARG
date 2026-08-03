using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YARG.Assets.Script.YargAP;
using YARG.Core.Input;
using YARG.Menu.History;
using YARG.Menu.ListMenu;
using YARG.Menu.Navigation;
using YARG.Menu.Persistent;
using YARG.Menu.Settings;
using YARG.Settings;
using YARG.Settings.Customization;
using YARG.Song;

namespace YARG.Menu.ArchipelagoMenu
{
    public class ArchipelagoMenu : MonoSingleton<ArchipelagoMenu>
    {
        public TMP_InputField ServerAddress;
        public TMP_InputField Port;
        public TMP_InputField Slotname;
        public TMP_InputField Password;

        public Toggle PrintChat;
        public Toggle PrintUnrelatedItems;

        public TMP_Dropdown DeathLinkMode;

        private void OnEnable()
        {
            // Set navigation scheme
            Navigator.Instance.PushScheme(new NavigationScheme(new()
            {
                new NavigationScheme.Entry(MenuAction.Red, "Menu.Common.Back", () => MenuManager.Instance.PopMenu())

            }, true));

        }

        public void ConnectButton() => APConnectionHelper.DoConnect($"{ServerAddress.text}:{Port.text}", Slotname.text, Password.text, "");
        public void DisconnectButton() => APConnectionHelper.DoDisconnect();

        public void SetToggles()
        {
            APEvents.PrintChatMessages = PrintChat.isOn;
            APEvents.PrintUnrelatedItems = PrintUnrelatedItems.isOn;
            APEvents.DeathLinkType = DeathLinkMode.value >= APData.DeathLinkValues.Length ? APEvents.DeathLinkYAML : APData.DeathLinkValues[DeathLinkMode.value];
            APEvents.UpdateDeathLinkTag();

            Debug.Log($"{APEvents.PrintChatMessages} | {APEvents.PrintUnrelatedItems} | {APEvents.DeathLinkType}");
        }

        private void OnDisable()
        {
            Navigator.Instance.PopScheme();

            // Save on close
            SettingsManager.SaveSettings();
            CustomContentManager.SaveAll();

            //This is a bit of a hack to update the CurrentNavigationGroup again.
            //ideally the settings menu should work just like every other menu so this isn't needed
            MenuManager.Instance.ReactivateCurrentMenu();
        }
    }
}