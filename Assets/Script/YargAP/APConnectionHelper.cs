using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using YARG.Helpers;
using YARG.Menu.Persistent;
using YARG.Song;
using Object = System.Object;

namespace YARG.Assets.Script.YargAP
{
    internal class APConnectionHelper
    {
        public static void DoConnect(string ServerAddress, string slotName, string Password, string GameID)
        {
            var GameCode = string.IsNullOrEmpty(GameID) ? "YARG" : $"YARG{GameID.Trim()}";
            if (APEvents.IsConnected) return;
            APEvents.Session = ArchipelagoSessionFactory.CreateSession(ServerAddress);
            var Result = APEvents.Session.TryConnectAndLogin(GameCode, slotName, Archipelago.MultiClient.Net.Enums.ItemsHandlingFlags.AllItems, password: Password);
            if (Result is LoginFailure failure)
            {
                ToastManager.ToastError("Failed to connect to Archipelago server: " + string.Join(Environment.NewLine, failure.Errors));
                APEvents.Session = null;
                return;
            }

            var SlotData = APEvents.Session.DataStorage.GetSlotData();
            string ConnectionError = "Failed to parse Slot Data";
            APData.YargSlotData ParsedSlotData = null;
            try { ParsedSlotData = APData.YargSlotData.Parse(SlotData); }
            catch (KeyNotFoundException ex) { ConnectionError = ($"\nMissing required field: {ex.Message}"); }
            catch (NullReferenceException ex) { ConnectionError = ($"\nNull value encountered: {ex.Message}"); }
            catch (InvalidCastException ex) { ConnectionError = ($"\nInvalid data type: {ex.Message}"); }
            catch (Exception ex) { ConnectionError = ($": {ex.Message}"); }

            if (ParsedSlotData == null)
            {
                DoDisconnect(true, $"Connection Failed:\n{ConnectionError}");
                return;
            }

            List<APData.APSongLocation> APSongLocations = new List<APData.APSongLocation>();
            APData.APGoalSong APGoalSong = null;
            var BadSongs = new List<APData.APSongData>();
            foreach (var song in ParsedSlotData.Songlist)
            {

                if (song.Key == ParsedSlotData.GoalSong && (string) song.Value.Source == ParsedSlotData.GoalSongSource)
                {
                    APGoalSong = new APData.APGoalSong(song.Key, ParsedSlotData.GemsRequired, song.Value);
                    if (SongContainer.Songs.All(x => !APGoalSong.MatchesSongEntry(x)))
                        BadSongs.Add(APGoalSong);
                    continue;
                }

                var songLocation = new APData.APSongLocation(song.Key, song.Value);
                APSongLocations.Add(songLocation);

                if (SongContainer.Songs.All(x => !songLocation.MatchesSongEntry(x)))
                    BadSongs.Add(songLocation);
            }

            if (APGoalSong is null)
            {
                DoDisconnect(true, $"Connection Failed:\nFailed to find Goal song [{ParsedSlotData.GoalSongSource}] {ParsedSlotData.GoalSong} in APSongList");
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
            APEvents.APGoalSong = APGoalSong;
            APEvents.GoalDisplaySetting = (APData.GoalDisplaySetting) ParsedSlotData.GoalSongVisibility;
            APEvents.DeathLinkType = (APData.DeathLinkType) ParsedSlotData.DeathLink;
            APEvents.DeathLinkYAML = (APData.DeathLinkType) ParsedSlotData.DeathLink;
            APEvents.EnergyLinkType = (APData.EnergyLinkType) ParsedSlotData.EnergyLink;
            APEvents.EnergyLinkYAML = (APData.EnergyLinkType) ParsedSlotData.EnergyLink;
            APEvents.DeathLinkService = APEvents.Session.CreateDeathLinkService();

            APEvents.Session.MessageLog.OnMessageReceived += APEvents.MessageLog_OnMessageReceived;
            APEvents.Session.Items.ItemReceived += APEvents.Items_ItemReceived;
            APEvents.DeathLinkService.OnDeathLinkReceived += APEvents.ProcessDeathLink;

            APEvents.UpdateDeathLinkTag();
            APEvents.UpdateRecievedInstruments();
            APEvents.APGoalSong.UpdateGoalItems();

            SaveConnectionCache(APEvents.Session, Password);

            ToastManager.ToastInformation("Connected to Archipelago server successfully!");
        }

        public static void DoDisconnect(bool Early = false, string ErrorMessage = null)
        {
            if (APEvents.IsConnected)
                APEvents.Session.Socket.DisconnectAsync();

            if (!Early)
            {
                APEvents.Session.MessageLog.OnMessageReceived -= APEvents.MessageLog_OnMessageReceived;
                APEvents.Session.Items.ItemReceived -= APEvents.Items_ItemReceived;
                APEvents.DeathLinkService.OnDeathLinkReceived -= APEvents.ProcessDeathLink;
            }

            APEvents.Session = null;
            APEvents.APGoalSong = null;
            APEvents.DeathLinkService = null;
            APEvents.APSongLocations = Array.Empty<APData.APSongLocation>();

            if (ErrorMessage is null)
                ToastManager.ToastInformation("Disconnected from Archipelago!");
            else
                ToastManager.ToastError(ErrorMessage);
        }

        private static string ConnectionCachePath = Path.Combine(PathHelper.PersistentDataPath, "APConnectionCache.json");
        public static void SaveConnectionCache(ArchipelagoSession session, string password)
        {
            if (session is null || !session.Socket.Connected) return;
            APData.ConnectionCache Cache = new APData.ConnectionCache()
            {
                IP = session.Socket.Uri.Host,
                Port = session.Socket.Uri.Port,
                SlotName = session.Players.ActivePlayer.Name,
                Password = password,
            };
            File.WriteAllText(ConnectionCachePath, JsonUtility.ToJson(Cache));
            Debug.Log($"Saved Connection Cache to {ConnectionCachePath}\n{JsonConvert.SerializeObject(Cache, Formatting.Indented)}");
        }

        public static APData.ConnectionCache LoadConnectionCache()
        {
            if (!File.Exists(ConnectionCachePath)) return null;
            try
            {
                var content = File.ReadAllText(ConnectionCachePath);
                return JsonUtility.FromJson<APData.ConnectionCache>(content);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
    }
}
