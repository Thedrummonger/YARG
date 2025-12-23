using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using DG.Tweening.Core.Easing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using YARG.Gameplay;
using YARG.Gameplay.Player;
using YARG.Menu.Persistent;

namespace YARG.Assets.Script.YargAP
{
    internal class APEvents
    {
        public static string GoalSong;
        public static GameManager CurrentSong = null;
        public static bool PrintChatMessages = true;
        public static bool PrintUnrelatedItems = false;
        public static ArchipelagoSession session;
        public static DeathLinkService deathLinkService;
        public static bool _isConnected => APEvents.session?.Socket != null && APEvents.session.Socket.Connected;

        public static HashSet<string> RecievedSongs = new HashSet<string>();

        public static void UpdateRecievedSongs()
        {
            if (!_isConnected)
                return;

            foreach(var item in session.Items.AllItemsReceived)
            {
                if (APData.APItemIDToHash().TryGetValue(item.ItemId, out var data))
                    RecievedSongs.Add(data);
            }
        }

        public static HashSet<string> GetUnplayedAvailableLocations()
        {
            HashSet<string> AvailableSongLocations = new();
            if (!_isConnected)
                return AvailableSongLocations;
            foreach (var i in RecievedSongs)
            {
                if (APData.SongHashToAPLocations().TryGetValue(i, out var locations) &&  locations.Any(x => !session.Locations.AllLocationsChecked.Contains(x)))
                    AvailableSongLocations.Add(i);
            }
            return AvailableSongLocations;
        }

        public static void Items_ItemReceived(Archipelago.MultiClient.Net.Helpers.ReceivedItemsHelper helper)
        {
            UpdateRecievedSongs();
            while (helper.Any())
            {
                var item = helper.DequeueItem();

                //Item ID 1 is Yarg Gem, for now I just made it grant star power
                if (item.ItemId == (long) APData.APFiller.YargGem && CurrentSong != null)
                    foreach (var i in CurrentSong.Players)
                        ApplyStarPowerItem(i, CurrentSong);
            }
        }

        public static void MessageLog_OnMessageReceived(Archipelago.MultiClient.Net.MessageLog.Messages.LogMessage message)
        {
            if (message is Archipelago.MultiClient.Net.MessageLog.Messages.ChatLogMessage chatMessage && !PrintChatMessages)
                return;
            if (message is Archipelago.MultiClient.Net.MessageLog.Messages.ItemSendLogMessage itemMessage && !itemMessage.IsReceiverTheActivePlayer && !PrintUnrelatedItems)
                return;
            ToastManager.ToastMessage(message.ToString());
        }

        private static MethodInfo _gainStarPower;
        public static void ApplyStarPowerItem(BasePlayer player, GameManager handler)
        {
            if (handler == null)
                return;

            Debug.Log("Processing Star Power Item");

            var engine = player.BaseEngine;
            if (engine == null) return;

            //Since we can;t edit yarg core, we have to use reflection to call GainStarPower since it is private
            _gainStarPower ??= engine.GetType().GetMethod("GainStarPower", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (_gainStarPower == null)
            {
                Debug.LogError("Failed to access GainStarPower with reflection");
                return;
            }
            _gainStarPower.Invoke(engine, new object[] { engine.TicksPerQuarterSpBar });
        }

        internal static void TryCheckSongLocation(GameManager gameManager)
        {
            if (!_isConnected || !RecievedSongs.Contains(gameManager.Song.Name))
                return;

            if (APData.SongHashToAPLocations().TryGetValue(gameManager.Song.Name, out var Locations))
                session.Locations.CompleteLocationChecksAsync(Locations);

            if (GoalSong != null && GoalSong == gameManager.Song.Name)
                session.SetGoalAchieved();

        }
    }
}
