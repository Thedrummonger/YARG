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
using JetBrains.Annotations;
using UnityEngine;
using YARG.Core.Extensions;
using YARG.Core.Song;
using YARG.Gameplay;
using YARG.Gameplay.Player;
using YARG.Menu.Persistent;
using YARG.Song;
using static UnityEngine.Rendering.DebugUI;

namespace YARG.Assets.Script.YargAP
{
    internal class APEvents
    {
        public static APData.APSongLocation[]   APSongLocations     = Array.Empty<APData.APSongLocation>();
        public static APData.APGoalSong         APGoalSong          = null;
        public static APData.GoalDisplaySetting GoalDisplaySetting  = APData.GoalDisplaySetting.BOTH;
        public static GameManager               CurrentSong         = null;
        public static bool                      PrintChatMessages   = true;
        public static bool                      PrintUnrelatedItems = false;
        public static ArchipelagoSession        Session;
        public static DeathLinkService          DeathLinkService;
        public static APData.DeathLinkType      DeathLinkType       = APData.DeathLinkType.DISABLED;
        public static APData.DeathLinkType      DeathLinkYAML       = APData.DeathLinkType.DISABLED;
        public static APData.EnergyLinkType     EnergyLinkType      = APData.EnergyLinkType.ENABLED;
        public static APData.EnergyLinkType     EnergyLinkYAML      = APData.EnergyLinkType.ENABLED;
        private static readonly System.Random   SeedRng             = new();
        public static bool                      IsConnected => Session?.Socket != null && APEvents.Session.Socket.Connected;

        public static string DeathLinkKey => IsConnected ? $"EnergyLink{Session.Players.ActivePlayer.Team}" : "";

        public static void Items_ItemReceived(Archipelago.MultiClient.Net.Helpers.ReceivedItemsHelper helper)
        {
            APGoalSong?.UpdateGoalItems();
            while (helper.Any())
            {
                var item = helper.DequeueItem();

                if (item.ItemId == (long) APData.APFiller.StarPower && CurrentSong != null)
                    foreach (var i in CurrentSong.Players)
                        ApplyStarPowerItem(i, CurrentSong);
            }
        }

        public static void MessageLog_OnMessageReceived(Archipelago.MultiClient.Net.MessageLog.Messages.LogMessage message)
        {
            if (message is Archipelago.MultiClient.Net.MessageLog.Messages.ChatLogMessage chatMessage && !PrintChatMessages)
                return;
            if (message is Archipelago.MultiClient.Net.MessageLog.Messages.ItemSendLogMessage itemMessage && !itemMessage.IsReceiverTheActivePlayer && !itemMessage.IsSenderTheActivePlayer && !PrintUnrelatedItems)
                return;
            ToastManager.ToastMessage(message.ToString());
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

        internal static void TryCheckSongLocation(GameManager gameManager)
        {
            if (!IsConnected) return;
            var matchingAPLocation = APSongLocations.FirstOrDefault(x => x.MatchesSongEntry(gameManager.Song));

            if (matchingAPLocation != null && matchingAPLocation.CanCompleteLocation() && matchingAPLocation.MetPlayRequirements(gameManager))
                Session.Locations.CompleteLocationChecksAsync(matchingAPLocation.LocationID1, matchingAPLocation.LocationID2);

            if (APGoalSong is not null && APGoalSong.MatchesSongEntry(gameManager.Song) && APGoalSong.CanCompleteLocation() && APGoalSong.MetPlayRequirements(gameManager))
                Session.SetGoalAchieved();

        }

        public static void ProcessDeathLink(DeathLink deathLink)
        {
            if (CurrentSong == null || DeathLinkType <= APData.DeathLinkType.DISABLED)
                return;
            ToastManager.ToastError($"{deathLink.Source} {deathLink.Cause}");
            switch (DeathLinkType)
            {
                case APData.DeathLinkType.INSTANT:
                    _ = CurrentSong.ForceSongFail();
                    break;
                case APData.DeathLinkType.ONE_HIT:
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
            foreach (var message in APData.DeathLinkMessages)
            {
                foreach(var player in players)
                {
                    var Valid = message.Valid(player.Player.Profile.CurrentInstrument);
                    if (Valid)
                        AllMessages.Add(message.Message);
                }
            }
            var Selected = AllMessages.PickRandom(SeedRng);
            return Selected;
        }

        const long minScale = 20000;
        const long maxScale = 1000000;
        public static void SendEnergy(int amount)
        {
            if (!IsConnected) return;

            int AmountOfLocationsTotal = Session.Locations.AllLocations.Count;
            int AmountOfLocationsChecked = Session.Locations.AllLocationsChecked.Count;
            double completionPercentage = AmountOfLocationsChecked / AmountOfLocationsTotal;
            double scale = minScale + (completionPercentage * (maxScale - minScale));
            long Energy = (long)(amount * scale);

            InitializeEnergyLink();
            Session.DataStorage[DeathLinkKey] += Energy;
        }

        public static void InitializeEnergyLink()
        {
            dynamic dataStorage = Session.DataStorage[DeathLinkKey];
            dynamic token = Newtonsoft.Json.Linq.JToken.FromObject(0);
            dataStorage.Initialize(token);
        }

        public static void UpdateDeathLinkTag()
        {
            if (DeathLinkType > APData.DeathLinkType.DISABLED)
                DeathLinkService.EnableDeathLink();
            else
                DeathLinkService.DisableDeathLink();
        }

    }
}
