using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using UnityEditor.PackageManager;
using UnityEngine;
using YARG.Helpers;
using YARG.Menu.Persistent;
using YARG.Song;

namespace YARG.Assets.Script.Yarchipelago
{
    internal class ConnectionHelper
    {
        public static void DoConnect(string ServerAddress, string slotName, string Password, string GameID)
        {
            var GameCode = string.IsNullOrEmpty(GameID) ? "YARG" : $"YARG{GameID.Trim()}";
            if (Events.IsConnected) return;
            Events.Session = ArchipelagoSessionFactory.CreateSession(ServerAddress);
            var Result = Events.Session.TryConnectAndLogin(GameCode, slotName, Archipelago.MultiClient.Net.Enums.ItemsHandlingFlags.AllItems, password: Password);
            if (Result is LoginFailure failure)
            {
                ApToastManager.APToastError("Failed to connect to Archipelago server: " + string.Join(Environment.NewLine, failure.Errors));
                Events.Session = null;
                return;
            }

            var SlotData = Events.Session.DataStorage.GetSlotData();
            string ConnectionError = "Failed to parse Slot Data";
            Models.YargSlotData ParsedSlotData = null;
            try { ParsedSlotData = Models.YargSlotData.Parse(SlotData); }
            catch (KeyNotFoundException ex) { ConnectionError = ($"\nMissing required field: {ex.Message}"); }
            catch (NullReferenceException ex) { ConnectionError = ($"\nNull value encountered: {ex.Message}"); }
            catch (InvalidCastException ex) { ConnectionError = ($"\nInvalid data type: {ex.Message}"); }
            catch (Exception ex) { ConnectionError = ($": {ex.Message}"); }

            if (ParsedSlotData == null)
            {
                DoDisconnect(true, $"Connection Failed:\n{ConnectionError}");
                return;
            }

            List<Models.APSongLocation> APSongLocations = new List<Models.APSongLocation>();
            Models.APGoalSong APGoalSong = null;
            var BadSongs = new List<Models.APSongData>();
            foreach (var song in ParsedSlotData.Songlist.Values)
            {
                Debug.Log($"Scanning [{song.Source}] {song.Name} by {song.Artist}");
                if (song.Name == ParsedSlotData.GoalSong && (string) song.Source == ParsedSlotData.GoalSongSource && song.Artist == ParsedSlotData.GoalSongArtist)
                {
                    APGoalSong = new Models.APGoalSong(ParsedSlotData.GemsRequired, song);
                    if (SongContainer.Songs.All(x => !APGoalSong.MatchesSongEntry(x)))
                        BadSongs.Add(APGoalSong);
                    continue;
                }

                var songLocation = new Models.APSongLocation(song);
                APSongLocations.Add(songLocation);

                if (SongContainer.Songs.All(x => !songLocation.MatchesSongEntry(x)))
                    BadSongs.Add(songLocation);
            }

            if (APGoalSong is null)
            {
                DoDisconnect(true, $"Connection Failed:\nFailed to find Goal song [{ParsedSlotData.GoalSongSource}] {ParsedSlotData.GoalSong} by {ParsedSlotData.GoalSongArtist} in APSongList");
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
            Events.APSongLocations = APSongLocations.ToArray();
            Events.APGoalSong = APGoalSong;
            Events.GoalDisplaySetting = (Models.GoalDisplaySetting) ParsedSlotData.GoalSongVisibility;
            Events.DeathLinkType = (Models.DeathLinkType) ParsedSlotData.DeathLink;
            Events.DeathLinkYAML = (Models.DeathLinkType) ParsedSlotData.DeathLink;
            Events.EnergyLinkType = (Models.EnergyLinkType) ParsedSlotData.EnergyLink;
            Events.EnergyLinkYAML = (Models.EnergyLinkType) ParsedSlotData.EnergyLink;
            Events.DeathLinkService = Events.Session.CreateDeathLinkService();

            Events.Session.MessageLog.OnMessageReceived += Events.MessageLog_OnMessageReceived;
            Events.Session.Items.ItemReceived += Events.Items_ItemReceived;
            Events.DeathLinkService.OnDeathLinkReceived += Events.ProcessDeathLink;

            Events.UpdateDeathLinkTag();
            Events.UpdateRecievedInstruments();
            Events.APGoalSong.UpdateGoalItems();

            SaveConnectionCache(Events.Session, Password);

            ApToastManager.APToastInformation("Connected to Archipelago server successfully!");
        }

        public static void DoDisconnect(bool Early = false, string ErrorMessage = null)
        {
            if (Events.IsConnected)
                Events.Session.Socket.DisconnectAsync();

            if (!Early)
            {
                Events.Session.MessageLog.OnMessageReceived -= Events.MessageLog_OnMessageReceived;
                Events.Session.Items.ItemReceived -= Events.Items_ItemReceived;
                Events.DeathLinkService.OnDeathLinkReceived -= Events.ProcessDeathLink;
            }

            Events.Session = null;
            Events.APGoalSong = null;
            Events.DeathLinkService = null;
            Events.APSongLocations = Array.Empty<Models.APSongLocation>();

            if (ErrorMessage is null)
                ApToastManager.APToastInformation("Disconnected from Archipelago!");
            else
                ApToastManager.APToastError(ErrorMessage);
        }

        private static string ConnectionCachePath = Path.Combine(PathHelper.PersistentDataPath, "APConnectionCache.json");
        public static void SaveConnectionCache(ArchipelagoSession session, string password)
        {
            if (session is null || !session.Socket.Connected) return;
            Models.ConnectionCache Cache = new Models.ConnectionCache()
            {
                IP = session.Socket.Uri.Host,
                Port = session.Socket.Uri.Port,
                SlotName = session.Players.ActivePlayer.Name,
                Password = password,
            };
            File.WriteAllText(ConnectionCachePath, JsonUtility.ToJson(Cache));
            Debug.Log($"Saved Connection Cache to {ConnectionCachePath}\n{JsonConvert.SerializeObject(Cache, Formatting.Indented)}");
        }

        public static Models.ConnectionCache LoadConnectionCache()
        {
            if (!File.Exists(ConnectionCachePath)) return null;
            try
            {
                var content = File.ReadAllText(ConnectionCachePath);
                return JsonUtility.FromJson<Models.ConnectionCache>(content);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
    }
}
