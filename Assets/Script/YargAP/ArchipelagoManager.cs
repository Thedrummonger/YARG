using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using YARG.Gameplay;
using YARG.Gameplay.Player;
using YARG.Menu.Persistent;
using static UnityEngine.Rendering.STP;

namespace YARG.Assets.Script.YargAP
{
    internal class ArchipelagoManager : MonoBehaviour
    {
        private void Start()
        {
            // I have attached this class to the persistant scene under integration so it runs when Yarg loads
            // For now it just connects to a local archipelago server with hardcoded credentials, but later the connection can
            // be made through the menu. This will probably be repurposed to just run any initialization code we need.
            ConnectTestSession();
        }
        public void ConnectTestSession()
        {
            APEvents.session = ArchipelagoSessionFactory.CreateSession("localhost");
            var Result = APEvents.session.TryConnectAndLogin("YARG", "Player1", Archipelago.MultiClient.Net.Enums.ItemsHandlingFlags.AllItems);
            if (Result is LoginFailure failure)
            {
                Debug.LogError("Failed to connect to Archipelago server: " + string.Join(Environment.NewLine, failure.Errors));
                APEvents.session = null;
                return;
            }
            Debug.Log("Connected to Archipelago server successfully!");

            APEvents.session.MessageLog.OnMessageReceived += APEvents.MessageLog_OnMessageReceived;
            APEvents.session.Items.ItemReceived += APEvents.Items_ItemReceived;

            var SlotData = APEvents.session.DataStorage.GetSlotData();
            if (SlotData["Goal Song"] is string GoalSongName && APData.SongHashMap.Values.Any(x => x == GoalSongName))
            {
                APEvents.GoalHash = APData.SongHashMap.First(x => x.Value == GoalSongName).Key;
            }
            else
            {
                ToastManager.ToastError($"Could not get Goal Song. Report this to the APworld Devs!");
                Debug.LogError($"Could not get Goal Song {JsonConvert.SerializeObject(SlotData)}");
            }

            bool DeathLinkEnabled = false;
            //We can use the yaml to enable death link and grab the option from slot data.
            if (DeathLinkEnabled)
            {
                APEvents.deathLinkService = DeathLinkProvider.CreateDeathLinkService(APEvents.session);
                APEvents.deathLinkService.EnableDeathLink();
            }

            APEvents.UpdateRecievedSongs();
        }
    }
}
