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
using Object = System.Object;

namespace YARG.Assets.Script.YargAP
{
    internal class APConnectionHelper
    {
        public static void DoConnect(string ServerAddress, string slotName, string Password)
        {
            if (APEvents.IsConnected) return;
            APEvents.Session = ArchipelagoSessionFactory.CreateSession(ServerAddress);
            var Result = APEvents.Session.TryConnectAndLogin("YARG", slotName, Archipelago.MultiClient.Net.Enums.ItemsHandlingFlags.AllItems, password: Password);
            if (Result is LoginFailure failure)
            {
                ToastManager.ToastError("Failed to connect to Archipelago server: " + string.Join(Environment.NewLine, failure.Errors));
                APEvents.Session = null;
                return;
            }

            var SlotData = APEvents.Session.DataStorage.GetSlotData();

            object SlotDataGoalSong;
            object SlotDataGoalSongSource;
            object SlotDataGemsRequired;
            object SlotDataSongList;
            object SlotDataGoalSongVisibility;
            object SlotDataDeathLink;

            string ConnectionError = null;

            if (!SlotData.TryGetValue("Goal Song", out SlotDataGoalSong) || SlotDataGoalSong is not string)
                ConnectionError = "Goal Song could not be parsed from slot data";

            if (!SlotData.TryGetValue("Goal Song Source", out SlotDataGoalSongSource) || SlotDataGoalSongSource is not string)
                ConnectionError = "Goal Song source could not be parsed from slot data";

            if (!SlotData.TryGetValue("Gems Required", out SlotDataGemsRequired) || SlotDataGemsRequired is not long)
                ConnectionError = "Required Gems could not be parsed from slot data";

            if (!SlotData.TryGetValue("songlist", out SlotDataSongList) || SlotDataSongList is not JObject)
                ConnectionError = "Song Data could not be parsed from slot data";

            if (!SlotData.TryGetValue("Goal Song Visibility", out SlotDataGoalSongVisibility) || SlotDataGoalSongVisibility is not long)
                ConnectionError = "Goal Song Visibility could not be parsed from slot data";

            if (!SlotData.TryGetValue("Death Link", out SlotDataDeathLink) || SlotDataDeathLink is not long)
                ConnectionError = "Death Link could not be parsed from slot data";

            if (ConnectionError is not null)
            {
                DoDisconnect(true, $"Connection Failed:\n{ConnectionError}");
                return;
            }

            List<APData.APSongLocation> APSongLocations = new List<APData.APSongLocation>();
            var BadSongs = new List<APData.APSongData>();
            foreach (var song in ((JObject)SlotDataSongList!).ToObject<Dictionary<string, object[]>>())
            {
                if (song.Key == (string) SlotDataGoalSong && (string) song.Value[3] == (string) SlotDataGoalSongSource)
                {
                    APEvents.APGoalSong = new APData.APGoalSong(song.Key, (string) song.Value[3], (long) song.Value[2], (int) (long) SlotDataGemsRequired!);
                    APEvents.APGoalSong?.UpdateGoalItems();
                    if (SongContainer.Songs.All(x => !APEvents.APGoalSong.MatchesSongEntry(x)))
                        BadSongs.Add(APEvents.APGoalSong);
                    continue;
                }

                var songLocation = new APData.APSongLocation(song.Key, (string) song.Value[3], (long) song.Value[0],
                    (long) song.Value[1], (long) song.Value[2]);
                APSongLocations.Add(songLocation);

                if (SongContainer.Songs.All(x => !songLocation.MatchesSongEntry(x)))
                    BadSongs.Add(songLocation);
            }

            if (APEvents.APGoalSong is null)
            {
                DoDisconnect(true, $"Connection Failed:\nFailed to find Goal song [{SlotDataGoalSongSource}] {SlotDataGoalSong} in APSongList");
                return;
            }

            if (BadSongs.Count > 0)
            {
                var badList = string.Join(" | ",
                    BadSongs.OrderBy(x => x.SongSource).ThenBy(x => x.SongName)
                        .Select(x => $"[{x.SongSource}] {x.SongName}"));
                DialogManager.Instance.ShowMessage("ERROR: Missing Songs!",
                    $"The following songs were included in your seed but were not found in YARG:\n\n{badList}");
            }
            APEvents.APSongLocations = APSongLocations.ToArray();
            APEvents.GoalDisplaySetting = (APData.GoalDisplaySetting) (long) SlotDataGoalSongVisibility!;

            APEvents.Session.MessageLog.OnMessageReceived += APEvents.MessageLog_OnMessageReceived;
            APEvents.Session.Items.ItemReceived += APEvents.Items_ItemReceived;

            if ((long)SlotDataDeathLink > 0)
            {
                APEvents.DeathLinkService = DeathLinkProvider.CreateDeathLinkService(APEvents.Session);
                APEvents.DeathLinkService.EnableDeathLink();
                APEvents.DeathLinkService.OnDeathLinkReceived += APEvents.ProcessDeathLink;
                APEvents.DeathLinkType = (long)SlotDataDeathLink  > 1 ? APData.DeathLinkType.Fail : APData.DeathLinkType.RockMeter;

            }
            ToastManager.ToastInformation("Connected to Archipelago server successfully!");

        }

        public static void DoDisconnect(bool Early = false, string ErrorMessage = null)
        {
            if (APEvents.IsConnected)
                APEvents.Session.Socket.DisconnectAsync();

            APEvents.APGoalSong = null;
            APEvents.DeathLinkService = null;
            APEvents.APSongLocations = Array.Empty<APData.APSongLocation>();
            if (Early) return;
            APEvents.Session.MessageLog.OnMessageReceived -= APEvents.MessageLog_OnMessageReceived;
            APEvents.Session.Items.ItemReceived -= APEvents.Items_ItemReceived;
            if (ErrorMessage is null)
                ToastManager.ToastInformation("Disconnected from Archipelago!");
            else
                ToastManager.ToastError(ErrorMessage);
        }
    }
}
