using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Mono.Unix.Native;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using YARG.Menu.Persistent;
using YARG.Song;

namespace YARG.Assets.Script.YargAP
{
    internal class APConnectionHelper
    {
        public static void DoConnect(string ServerAddress, string slotName, string Password)
        {
            if (APEvents._isConnected) return;
            APEvents.session = ArchipelagoSessionFactory.CreateSession(ServerAddress);
            var Result = APEvents.session.TryConnectAndLogin("YARG", slotName, Archipelago.MultiClient.Net.Enums.ItemsHandlingFlags.AllItems, password: Password);
            if (Result is LoginFailure failure)
            {
                ToastManager.ToastError("Failed to connect to Archipelago server: " + string.Join(Environment.NewLine, failure.Errors));
                APEvents.session = null;
                return;
            }

            var SlotData = APEvents.session.DataStorage.GetSlotData();

            if (SlotData.ContainsKey("songlist"))
            {
                var songlistObj = (JObject) SlotData["songlist"];
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
            foreach (var i in APData.SongNames)
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
                APEvents.GoalItemNeeded = (int) GoalItemsNeeded;
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

        public static void DoDisconnect()
        {
            if (APEvents._isConnected)
                APEvents.session.Socket.DisconnectAsync();

            APEvents.session.MessageLog.OnMessageReceived -= APEvents.MessageLog_OnMessageReceived;
            APEvents.session.Items.ItemReceived -= APEvents.Items_ItemReceived;
            APEvents.GoalSong = null;
            APEvents.deathLinkService = null;
            APData.SongNames = new Dictionary<string, int[]>();
            APData.NeedsRegen = true;
            ToastManager.ToastInformation("Disconnected from Archipelago!");
        }
    }
}
