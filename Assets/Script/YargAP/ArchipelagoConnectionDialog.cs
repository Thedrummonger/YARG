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
            using (new GUIEnabledScope(!APEvents._isConnected))
                address = GUILayout.TextField(address);

            GUILayout.Label("Slot Name");
            using (new GUIEnabledScope(!APEvents._isConnected))
                slotName = GUILayout.TextField(slotName);

            GUILayout.Label("Password");
            using (new GUIEnabledScope(!APEvents._isConnected))
                password = GUILayout.PasswordField(password, '*');

            GUILayout.Space(10);
            string buttonText = APEvents._isConnected ? "Disconnect" : "Connect";
            if (GUILayout.Button(buttonText, GUILayout.Height(28)))
            {
                GUI.FocusControl(null);
                GUIUtility.keyboardControl = 0;
                if (APEvents._isConnected)
                    DoDisconnect();
                else
                    DoConnect();
            }
            GUILayout.EndVertical();

            GUILayout.Space(20);

            GUILayout.BeginVertical(GUILayout.Width(180));

            GUILayout.Label("Deathlink Status");

            string statusText = APEvents._isConnected ? (APEvents.deathLinkService != null ? "-Enabled by YAML" : "-Disabled by YAML") : "-Not Connected";
            GUILayout.Label(statusText);

            GUILayout.Label("Deathlink Setting Override");

            using (new GUIEnabledScope(APEvents._isConnected && APEvents.deathLinkService != null))
            {
                if (GUILayout.Button(deathLinkOptions[APEvents.deathLinkOverride], GUILayout.Height(20)))
                {
                    var NewVal = APEvents.deathLinkOverride + 1;
                    if (NewVal >= deathLinkOptions.Length)
                        NewVal = 0;
                    APEvents.deathLinkOverride = NewVal;
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

        private void DoConnect()
        {
            APEvents.session = ArchipelagoSessionFactory.CreateSession(address);
            var Result = APEvents.session.TryConnectAndLogin("YARG", slotName, Archipelago.MultiClient.Net.Enums.ItemsHandlingFlags.AllItems, password: password);
            if (Result is LoginFailure failure)
            {
                ToastManager.ToastError("Failed to connect to Archipelago server: " + string.Join(Environment.NewLine, failure.Errors));
                APEvents.session = null;
                return;
            }

            var SlotData = APEvents.session.DataStorage.GetSlotData();

            if (SlotData.ContainsKey("songlist"))
            {
                var songlistObj = (JObject)SlotData["songlist"];
                APData.SongNames = songlistObj.ToObject<Dictionary<string, int[]>>();
                APData.NeedsRegen = true;
            }
            else
            {
                ToastManager.ToastError($"Unable to parse song list. Report this to the APworld Devs!");
                Debug.LogError($"Unable to parse song list {JsonConvert.SerializeObject(SlotData)}");
                APEvents.session.Socket.DisconnectAsync();
                APEvents.session = null;
                return;
            }

            bool WasMissingSong = false;
            foreach(var i in APData.SongNames)
                if (!SongContainer.Songs.Any(x => x.Name == i.Key))
                {
                    WasMissingSong = true;
                    Debug.LogError($"{i.Key} Was not found in the current yarg song list");
                }

            if (WasMissingSong)
                DialogManager.Instance.ShowMessage("Missing Song Error", "One or more songs were not found in your YARG setlist\nEnsure you are using the YARG official setlist!");


            if (SlotData["Goal Song"] is string GoalSongName && APData.SongNames.ContainsKey(GoalSongName))
            {
                Debug.Log($"Goal Song {GoalSongName}");
                APEvents.GoalSong = GoalSongName;
            }
            else
            {
                ToastManager.ToastError($"Could not get Goal Song. Report this to the APworld Devs!");
                Debug.LogError($"Could not get Goal Song {JsonConvert.SerializeObject(SlotData)}");
                APEvents.session.Socket.DisconnectAsync();
                APEvents.session = null;
                return;
            }

            if (SlotData["Gems Required"] is long GoalItemsNeeded)
            {
                Debug.Log($"Gems Needed {GoalItemsNeeded}");
                APEvents.GoalItemNeeded = (int)GoalItemsNeeded;
            }
            else
            {
                ToastManager.ToastError($"Could not get Goal Item Requirement. Report this to the APworld Devs!");
                Debug.LogError($"Could not get Goal Song {JsonConvert.SerializeObject(SlotData)}");
                APEvents.session.Socket.DisconnectAsync();
                APEvents.session = null;
                return;
            }

            if (SlotData.TryGetValue("Goal Song Visibility", out var GSV) && GSV is Int64 VI)
            {
                APEvents.goalDisplaySetting = (APData.GoalDisplaySetting) VI;
            }

            APEvents.session.MessageLog.OnMessageReceived += APEvents.MessageLog_OnMessageReceived;
            APEvents.session.Items.ItemReceived += APEvents.Items_ItemReceived;

            if (SlotData.TryGetValue("Death Link", out var DLO) && DLO is long DLI && DLI > 0)
            {
                APEvents.deathLinkService = DeathLinkProvider.CreateDeathLinkService(APEvents.session);
                APEvents.deathLinkService.EnableDeathLink();
                APEvents.deathLinkService.OnDeathLinkReceived += APEvents.ProcessDeathLink;
                APEvents.deathLinkType = DLI > 1 ? APData.DeathLinkType.Fail : APData.DeathLinkType.RockMeter;
                
            }

            ToastManager.ToastInformation("Connected to Archipelago server successfully!");

            APEvents.UpdateRecievedSongs();
        }

        private void DoDisconnect()
        {
            if (APEvents._isConnected)
                APEvents.session.Socket.DisconnectAsync();

            APEvents.session.MessageLog.OnMessageReceived -= APEvents.MessageLog_OnMessageReceived;
            APEvents.session.Items.ItemReceived -= APEvents.Items_ItemReceived;
            APEvents.GoalSong = null;
            APEvents.deathLinkService = null;
            APData.SongNames = new Dictionary<string, int[]>();
            APData.NeedsRegen = true;
        }

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
