using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using YARG.Helpers;
using YARG.Menu.Persistent;
using YARG.Song;

namespace YARG.Assets.Script.YargAP
{
    public class ArchipelagoConnectionDialog : MonoBehaviour
    {
        public static ArchipelagoConnectionDialog Instance { get; private set; }

        [Header("State")]
        public bool Show = false;

        [Header("Defaults")]
        [SerializeField] private string address = "archipelago.gg:38281";
        [SerializeField] private string slotName = "";
        [SerializeField] private string password = "";
        [SerializeField] private string connectionSuffix = "";

        private int DeathLinkOverride = APData.DeathLinkValues.Length;
        private int EnergyLinkOverride = APData.EnergyLinkValues.Length;

        private bool     showDeathlinkDropdown = false;

        private Rect _windowRect = new Rect(20, 20, 430, 310);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            var Cache = APConnectionHelper.LoadConnectionCache();
            if (Cache is not null)
            {
                address = $"{Cache.IP}:{Cache.Port}";
                slotName = Cache.SlotName;
                password = Cache.Password;
                connectionSuffix = Cache.ConnectionSuffix;
            }
        }

        private void OnGUI()
        {
            if (!Show) return;

            _windowRect = GUI.Window(0xA1C4, _windowRect, DrawWindow, "Archipelago Connection");
        }
        private void DrawWindow(int id)
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.Width(180));
            GUILayout.Label("Address");
            using (new GUIEnabledScope(!APEvents.IsConnected))
                address = GUILayout.TextField(address);

            GUILayout.Label("Slot Name");
            using (new GUIEnabledScope(!APEvents.IsConnected))
                slotName = GUILayout.TextField(slotName);

            GUILayout.Label("Password");
            using (new GUIEnabledScope(!APEvents.IsConnected))
                password = GUILayout.PasswordField(password, '*');

            GUILayout.Label("Game ID");
            using (new GUIEnabledScope(!APEvents.IsConnected))
                connectionSuffix = GUILayout.TextField(connectionSuffix);

            GUILayout.Space(10);
            string buttonText = APEvents.IsConnected ? "Disconnect" : "Connect";
            if (GUILayout.Button(buttonText, GUILayout.Height(28)))
            {
                GUI.FocusControl(null);
                GUIUtility.keyboardControl = 0;
                if (APEvents.IsConnected)
                    DoDisconnect();
                else
                    DoConnect();
            }
            GUILayout.Space(6);
            if (GUILayout.Button("Close", GUILayout.Height(28)))
                Show = false;
            GUILayout.EndVertical();

            GUILayout.Space(20);

            GUILayout.BeginVertical(GUILayout.Width(190));

            GUILayout.Label($"Deathlink YAML: {(APEvents.IsConnected ? APEvents.DeathLinkYAML.GetDescription() : "N/A")}");

            GUILayout.Label("Deathlink Override");

            using (new GUIEnabledScope(APEvents.IsConnected && APEvents.DeathLinkService != null))
            {
                var Display = DeathLinkOverride == APData.DeathLinkValues.Length ? "Use Yaml" : APEvents.DeathLinkType.GetDescription();
                if (GUILayout.Button(Display, GUILayout.Height(20)))
                {
                    DeathLinkOverride++;
                    if (DeathLinkOverride > APData.DeathLinkValues.Length) DeathLinkOverride = 0;
                    if (DeathLinkOverride == APData.DeathLinkValues.Length)
                        APEvents.DeathLinkType = APEvents.DeathLinkYAML;
                    else
                        APEvents.DeathLinkType = APData.DeathLinkValues[DeathLinkOverride];
                    APEvents.UpdateDeathLinkTag();
                    Debug.Log($"{DeathLinkOverride} | {APEvents.DeathLinkType} | {APEvents.DeathLinkYAML}");
                }
            }

            GUILayout.Label($"Energylink YAML: {(APEvents.IsConnected ? APEvents.EnergyLinkYAML.GetDescription() : "N/A")}");

            GUILayout.Label("Energylink Override");
            using (new GUIEnabledScope(APEvents.IsConnected))
            {
                var Display = EnergyLinkOverride == APData.EnergyLinkValues.Length ? "Use Yaml" : APEvents.EnergyLinkType.GetDescription();
                if (GUILayout.Button(Display, GUILayout.Height(20)))
                {
                    EnergyLinkOverride++;
                    if (EnergyLinkOverride > APData.EnergyLinkValues.Length) EnergyLinkOverride = 0;
                    if (EnergyLinkOverride == APData.EnergyLinkValues.Length)
                        APEvents.EnergyLinkType = APEvents.EnergyLinkYAML;
                    else
                        APEvents.EnergyLinkType = APData.EnergyLinkValues[EnergyLinkOverride];
                    Debug.Log($"{EnergyLinkOverride} | {APEvents.EnergyLinkType} | {APEvents.EnergyLinkYAML}");
                }
            }

            GUILayout.Label("Chat settings");

            if (GUILayout.Button($"Print Chat Messages: {APEvents.PrintChatMessages}"))
                APEvents.PrintChatMessages = !APEvents.PrintChatMessages;

            if (GUILayout.Button($"Print Unrelated Items: {APEvents.PrintUnrelatedItems}"))
                APEvents.PrintUnrelatedItems = !APEvents.PrintUnrelatedItems;

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        private void DoConnect() => APConnectionHelper.DoConnect(address, slotName, password, connectionSuffix);

        private void DoDisconnect() => APConnectionHelper.DoDisconnect();

        private readonly struct GUIEnabledScope : System.IDisposable
        {
            private readonly bool _prev;
            public GUIEnabledScope(bool enabled)
            {
                _prev = GUI.enabled;
                GUI.enabled = enabled;
            }
            public void Dispose() => GUI.enabled = _prev;
        }
    }
}
