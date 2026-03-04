using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using YARG.Core.Extensions;
using YARG.Gameplay;
using YARG.Gameplay.HUD;
using YARG.Gameplay.Player;
using YARG.Menu.Persistent;
using static YARG.Assets.Script.Yarchipelago.Models;

namespace YARG.Assets.Script.Yarchipelago
{
    internal class Events
    {
        public static APSongLocation[] APSongLocations = Array.Empty<APSongLocation>();
        public static APGoalSong APGoalSong = null;
        public static GoalDisplaySetting GoalDisplaySetting = GoalDisplaySetting.BOTH;
        public static GameManager CurrentSong = null;
        public static bool PrintChatMessages = true;
        public static bool PrintUnrelatedItems = false;
        public static ArchipelagoSession Session;
        public static DeathLinkService DeathLinkService;
        public static DeathLinkType DeathLinkType = DeathLinkType.DISABLED;
        public static DeathLinkType DeathLinkYAML = DeathLinkType.DISABLED;
        public static EnergyLinkType EnergyLinkType = EnergyLinkType.ENABLED;
        public static EnergyLinkType EnergyLinkYAML = EnergyLinkType.ENABLED;
        private static readonly System.Random SeedRng = new();
        public static bool IsConnected => Session?.Socket != null && Session.Socket.Connected;

        public static HashSet<string> AllReceivedInstruments = new HashSet<string>();

        const long minScale = 20000;
        const long maxScale = 1000000;

        public static string DeathLinkKey => IsConnected ? $"EnergyLink{Session.Players.ActivePlayer.Team}" : "";

        public static void Items_ItemReceived(Archipelago.MultiClient.Net.Helpers.ReceivedItemsHelper helper)
        {
            APGoalSong?.UpdateGoalItems();
            UpdateRecievedInstruments();
            while (helper.Any())
            {
                var item = helper.DequeueItem();

                if (item.ItemId == (long) APFiller.StarPower && CurrentSong != null)
                    foreach (var i in CurrentSong.Players)
                        ApplyStarPowerItem(i, CurrentSong);
            }
        }

        public static void UpdateRecievedInstruments()
        {
            if (!IsConnected) return;
            HashSet<string> AllInstItems = APInstrumentKeyToName.Values.ToHashSet();
            foreach (var itemInfo in Session.Items.AllItemsReceived)
            {
                if (AllInstItems.Contains(itemInfo.ItemName))
                    AllReceivedInstruments.Add(itemInfo.ItemName);
            }
        }

        public static void MessageLog_OnMessageReceived(Archipelago.MultiClient.Net.MessageLog.Messages.LogMessage message)
        {
            ItemFlags? Flag = null;
            if (message is Archipelago.MultiClient.Net.MessageLog.Messages.ChatLogMessage && !PrintChatMessages)
                return;
            if (message is Archipelago.MultiClient.Net.MessageLog.Messages.ItemSendLogMessage itemMessage)
            {
                if (!itemMessage.IsReceiverTheActivePlayer && !itemMessage.IsSenderTheActivePlayer && !PrintUnrelatedItems)
                    return;
                Flag = ItemFlags.None;
                if (itemMessage.Item.Flags.HasFlag(ItemFlags.NeverExclude)) Flag = ItemFlags.NeverExclude;
                if (itemMessage.Item.Flags.HasFlag(ItemFlags.Advancement)) Flag = ItemFlags.Advancement;
            }
            if (Flag is null)
                ToastManager.APToastMessage(message.ToYargColoredString());
            else if (Flag == ItemFlags.None)
                ToastManager.APToastJunkItem(message.ToYargColoredString());
            else if (Flag == ItemFlags.NeverExclude)
                ToastManager.APToastStandardItem(message.ToYargColoredString());
            else if (Flag == ItemFlags.Advancement)
                ToastManager.APToastProgressionItem(message.ToYargColoredString());
            else
                ToastManager.APToastMessage(message.ToYargColoredString());
        }

        private static MethodInfo _gainStarPower;
        public static void ApplyStarPowerItem(BasePlayer player, GameManager handler)
        {
            if (handler == null) return;
            Debug.Log("Processing Star Power Item");
            var engine = player.BaseEngine;
            if (engine == null) return;

            //Since we can't edit yarg core, we have to use reflection to call GainStarPower since it is private
            _gainStarPower ??= engine.GetType().GetMethod("GainStarPower", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (_gainStarPower == null)
            {
                Debug.LogError("Failed to access GainStarPower with reflection");
                return;
            }
            _gainStarPower.Invoke(engine, new object[] { engine.TicksPerQuarterSpBar });
        }

        private static bool ShowGoalWarningForSong = false;

        /// Archipelago integration:
        /// Tracks the active song instance when gameplay begins.
        /// Inserted at the end of <see cref="YARG.Gameplay.GameManager.Awake"/>.
        public static void OnSongAwake(GameManager gameManager)
        {
            ShowGoalWarningForSong = false;
            CurrentSong = gameManager;
        }
        /// Archipelago integration:
        /// Clears the active song reference when the gameplay manager is destroyed.
        /// Inserted at the beginning of <see cref="YARG.Gameplay.GameManager.OnDestroy"/>.
        public static void OnSongDestroy(GameManager gameManager)
        {
            CurrentSong = null;
        }

        /// Archipelago integration:
        /// Handles Archipelago location checks and EnergyLink scoring when a song ends.
        /// Inserted in <see cref="YARG.Gameplay.GameManager.EndSong"/> immediately before
        /// <c>CrowdEventHandler.Dispose()</c>.
        public static void OnEndSong(GameManager gameManager)
        {
            TryCheckSongLocation(gameManager);
            if (EnergyLinkType > EnergyLinkType.DISABLED)
                SendEnergy(gameManager.BandScore);
        }

        /// Archipelago integration:
        /// Allows calls on each game tick.
        /// Inserted at the beginning of <see cref="YARG.Gameplay.GameManager.Update"/>.
        public static void OnSongUpdate(GameManager gameManager, PauseMenuManager _pauseMenu)
        {
            WarnGoalSongUnavailable(gameManager, _pauseMenu);
        }

        private static void WarnGoalSongUnavailable(GameManager gameManager, PauseMenuManager _pauseMenu)
        {
            if (!ShowGoalWarningForSong && !gameManager.IsPractice && APGoalSong != null && APGoalSong.MatchesSongEntry(gameManager.Song) && !APGoalSong.CanCompleteLocation())
            {
                StringBuilder Error = new StringBuilder();

                Error.AppendLine("You have selected your goal song but you do not have the items needed to complete it!");
                if (!APGoalSong.HasEnoughYargGems())
                    Error.AppendLine($"\nYou have {APGoalSong.GoalItemCount} Gems, but you need {APGoalSong.GoalItemNeeded}!");
                if (!APGoalSong.HasReceivedSong())
                    Error.AppendLine($"\nYou have not found your goal song item!");

                gameManager.SetPaused(!_pauseMenu.IsOpen);
                DialogManager.Instance.ShowMessage("Goal Song Not Unlocked", Error.ToString());
            }
            ShowGoalWarningForSong = true;
        }

        internal static void TryCheckSongLocation(GameManager gameManager)
        {
            if (!IsConnected) return;

            if (APGoalSong is not null && APGoalSong.MatchesSongEntry(gameManager.Song))
            {
                var canComplete = APGoalSong.CanCompleteLocation();
                var metReqs = APGoalSong.MetPlayRequirements(gameManager);
                if (canComplete && metReqs)
                    Session.SetGoalAchieved();
                return;
            }

            var matchingAPLocation = APSongLocations.FirstOrDefault(x => x.MatchesSongEntry(gameManager.Song));
            if (matchingAPLocation is not null)
            {
                var canComplete = matchingAPLocation.CanCompleteLocation();
                var metReqs = matchingAPLocation.MetPlayRequirements(gameManager);
                if (canComplete && metReqs)
                    Session.Locations.CompleteLocationChecksAsync(matchingAPLocation.LocationID1, matchingAPLocation.LocationID2, matchingAPLocation.LocationID3);
                return;
            }

        }

        public static void ProcessDeathLink(DeathLink deathLink)
        {
            if (CurrentSong == null || DeathLinkType <= DeathLinkType.DISABLED)
                return;
            ToastManager.ToastError($"{deathLink.Source} {deathLink.Cause}");
            switch (DeathLinkType)
            {
                case DeathLinkType.INSTANT:
                    CurrentSong.ForceSongFail();
                    break;
                case DeathLinkType.ONE_HIT:
                    //Set each players rock meter low enough that one missed note causes a fail.
                    foreach (var player in CurrentSong.Players)
                    {
                        var EngineContainer = player.GetEngineContainer();
                        EngineContainer.SetHappiness(CurrentSong.EngineManager, 0.02f);
                    }
                    break;
            }
        }

        public static string GetRandomDeatLinkMessage(GameManager game, List<BasePlayer> players)
        {
            List<string> AllMessages = new List<string>() { $"failed to play {game.Song.Name}" };
            foreach (var message in DeathLinkMessages)
            {
                foreach (var player in players)
                {
                    var Valid = message.Valid(player.Player.Profile.CurrentInstrument);
                    if (Valid)
                        AllMessages.Add(message.Message);
                }
            }
            var Selected = AllMessages.PickRandom(SeedRng);
            return Selected;
        }

        public static void SendEnergy(int amount)
        {
            if (!IsConnected) return;

            try
            {
                InitializeEnergyLink();
                long Energy = ApplyScale(amount, Session.Locations.AllLocations.Count, Session.Locations.AllLocationsChecked.Count);
                Session.DataStorage[DeathLinkKey] += Energy;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                ToastManager.ToastError(e.ToString());
            }
        }

        public static long ApplyScale(long Score, double AmountOfLocationsTotal, double AmountOfLocationsChecked)
        {
            double completionPercentage = AmountOfLocationsChecked / AmountOfLocationsTotal;
            double scale = minScale + (completionPercentage * (maxScale - minScale));
            return (long) (Score * scale);
        }

        public static void TestScaleFactor()
        {
            var Score = 200_000;
            var TotalLocations = 20;

            Dictionary<int, long> Tests = new Dictionary<int, long>();
            for (var checkedLocations = 0; checkedLocations <= TotalLocations; checkedLocations++)
            {
                Tests[checkedLocations] = ApplyScale(Score, TotalLocations, checkedLocations);
            }
            Debug.Log(Newtonsoft.Json.JsonConvert.SerializeObject(Tests, Newtonsoft.Json.Formatting.Indented));
        }

        public static void InitializeEnergyLink()
        {
            dynamic dataStorage = Session.DataStorage[DeathLinkKey];
            dynamic token = Newtonsoft.Json.Linq.JToken.FromObject(0);
            dataStorage.Initialize(token);
        }

        public static void UpdateDeathLinkTag()
        {
            if (DeathLinkType > DeathLinkType.DISABLED)
                DeathLinkService.EnableDeathLink();
            else
                DeathLinkService.DisableDeathLink();
        }
    }
}
