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

        private string[] deathLinkOptions = { "use yaml", "disabled", "instant", "one hit" };
        private bool showDeathlinkDropdown = false;

        private Rect _windowRect = new Rect(20, 20, 400, 260);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
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
            GUILayout.EndVertical();

            GUILayout.Space(20);

            GUILayout.BeginVertical(GUILayout.Width(180));

            GUILayout.Label("Deathlink Status");

            string statusText = APEvents.IsConnected ? (APEvents.DeathLinkService != null ? "-Enabled by YAML" : "-Disabled by YAML") : "-Not Connected";
            GUILayout.Label(statusText);

            GUILayout.Label("Deathlink Setting Override");

            using (new GUIEnabledScope(APEvents.IsConnected && APEvents.DeathLinkService != null))
            {
                if (GUILayout.Button(deathLinkOptions[APEvents.DeathLinkOverride], GUILayout.Height(20)))
                {
                    var NewVal = APEvents.DeathLinkOverride + 1;
                    if (NewVal >= deathLinkOptions.Length)
                        NewVal = 0;
                    APEvents.DeathLinkOverride = NewVal;
                }
            }

            GUILayout.Label("Chat settings");

            if (GUILayout.Button($"Print Chat Messages: {APEvents.PrintChatMessages}"))
                APEvents.PrintChatMessages = !APEvents.PrintChatMessages;

            if (GUILayout.Button($"Print Unrelated Items: {APEvents.PrintUnrelatedItems}"))
                APEvents.PrintUnrelatedItems = !APEvents.PrintUnrelatedItems;

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            if (GUILayout.Button("Close"))
                Show = false;

            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        private void DoConnect() => APConnectionHelper.DoConnect(address, slotName, password);

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
