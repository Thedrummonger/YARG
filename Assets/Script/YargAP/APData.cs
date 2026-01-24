using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using YARG.Core;
using YARG.Core.Song;
using YARG.Gameplay;
using YARG.Song;

namespace YARG.Assets.Script.YargAP
{
    internal static class APData
    {
        public abstract class APSongData
        {
            public string SongName;
            public string SongSource;
            public long   ItemID;
            public string Instrument;
            public SongEntry GetYargSongEntry() => SongContainer.Songs.FirstOrDefault(x => x.Name == SongName && x.Source.Original == SongSource);
            public bool HasReceivedSong() => APEvents.IsConnected &&
                APEvents.Session.Items.AllItemsReceived.Any(x => x.ItemId == ItemID);

            public bool UsedMatchingInstrument(GameManager gameManager)
            {
                if (Instrument is null) return true;
                var usedInstruments = new HashSet<string>();
                var harmonyCount = gameManager.Players.Count(x => x.Player.Profile.CurrentInstrument == Core.Instrument.Harmony);
                if (harmonyCount > 1) usedInstruments.Add("harmony2");
                if (harmonyCount > 2) usedInstruments.Add("harmony3");
                foreach (var player in gameManager.Players)
                    if (YargInstrumentToAPKey.TryGetValue(player.Player.Profile.CurrentInstrument, out var inst))
                        usedInstruments.Add(inst);
                return usedInstruments.Contains(Instrument);
            }

            public bool HasReceiveInstrumentItem()
            {
                if (!APEvents.IsConnected) return false;
                if (Instrument is null) return true;
                if (!APInstrumentKeyToName.TryGetValue(Instrument, out var inst)){ return false;}
                return APEvents.AllReceivedInstruments.Contains(inst);
            }

            public bool MatchesSongEntry(SongEntry entry) => SongName == entry.Name && SongSource == entry.Source.Original;

            public abstract bool CanCompleteLocation();

            public abstract bool VisibleInSongList();

            public bool MetPlayRequirements(GameManager gameManager)
            {
                if (!UsedMatchingInstrument(gameManager))
                    return false;
                return true;
            }
        }

        public class APGoalSong : APSongData
        {
            public APGoalSong(string name, int GoalItems, SongMetadata meta)
            {
                SongName = name;
                SongSource = meta.Source;
                ItemID = meta.ItemId;
                GoalItemNeeded = GoalItems;
                Instrument = meta.Instrument;
            }
            public int GoalItemCount  { get; private set; }  = 0;
            public int GoalItemNeeded { get; }
            public bool HasEnoughYargGems() => GoalItemCount >= GoalItemNeeded;

            public void UpdateGoalItems() =>
                GoalItemCount = APEvents.IsConnected
                    ? APEvents.Session.Items.AllItemsReceived.Count(x => x.ItemId == (long) APData.APFiller.YargGem)
                    : 0;

            public override bool CanCompleteLocation() => HasReceivedSong() && HasReceiveInstrumentItem() && HasEnoughYargGems();

            public override bool VisibleInSongList() =>
                APEvents.GoalDisplaySetting == APData.GoalDisplaySetting.FULL ||
                (HasReceivedSong() && APEvents.GoalDisplaySetting == APData.GoalDisplaySetting.SONG) ||
                (HasEnoughYargGems() && APEvents.GoalDisplaySetting == APData.GoalDisplaySetting.GEMS) ||
                (HasReceivedSong() && HasEnoughYargGems() && APEvents.GoalDisplaySetting == APData.GoalDisplaySetting.BOTH);
        }
        public class APSongLocation : APSongData
        {
            public APSongLocation(string name, SongMetadata meta)
            {
                SongName = name;
                SongSource = meta.Source;
                ItemID = meta.ItemId;
                LocationID1 = meta.Loc1Id;
                LocationID2 = meta.Loc2Id;
                LocationID3 = meta.Loc3Id;
                Instrument = meta.Instrument;
            }
            public long LocationID1;
            public long LocationID2;
            public long LocationID3;
            public bool HasCheckedBothLocations() => APEvents.IsConnected &&
                APEvents.Session.Locations.AllLocationsChecked.Contains(LocationID1) &&
                APEvents.Session.Locations.AllLocationsChecked.Contains(LocationID2);
            public override bool VisibleInSongList() => HasReceivedSong() && !HasCheckedBothLocations();
            public override bool CanCompleteLocation() => HasReceivedSong() && HasReceiveInstrumentItem() && !HasCheckedBothLocations();
        }

        public class ConnectionCache
        {
            public string IP;
            public int    Port;
            public string SlotName;
            public string Password;
        }

        public static List<DeathLinkMessage> DeathLinkMessages = new()
        {
            new("got boo'd offstage."),
            new("tripped over a cable."),
            new("failed a stage dive."),
            new("dropped their pick.", new(){Instrument.FiveFretGuitar, Instrument.FiveFretBass, Instrument.FiveFretRhythm, Instrument.FiveFretCoopGuitar}),
            new("dropped their last drum sticks.", new(){Instrument.EliteDrums, Instrument.FourLaneDrums, Instrument.ProDrums, Instrument.FiveLaneDrums}),
            new("dropped the mic.", new(){Instrument.Vocals, Instrument.Harmony}),
            new("knocked over the keyboard.", new(){Instrument.Keys, Instrument.ProKeys}),
        };
        public class DeathLinkMessage
        {
            public DeathLinkMessage(string message, HashSet<Instrument> instruments = null)
            {
                Message = message;
                InstrumentTags = instruments ?? new HashSet<Instrument>();
            }
            public string Message;
            public HashSet<Instrument> InstrumentTags = new();
            public override string ToString() => Message;
            public bool Valid(Instrument instrument) => InstrumentTags.Count == 0 || InstrumentTags.Contains(instrument);
        }

        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attribute?.Description ?? value.ToString();
        }

        public enum APFiller
        {
            YargGem = 1,
            StarPower = 2
        }
        public static APData.DeathLinkType[] DeathLinkValues = Enum.GetValues(typeof(APData.DeathLinkType)).Cast<APData.DeathLinkType>().ToArray();
        public enum DeathLinkType
        {
            [Description("Disabled")]
            DISABLED  = 0,
            [Description("One Hit")]
            ONE_HIT = 1,
            [Description("Instant")]
            INSTANT = 2
        }
        public static APData.EnergyLinkType[] EnergyLinkValues = Enum.GetValues(typeof(APData.EnergyLinkType)).Cast<APData.EnergyLinkType>().ToArray();
        public enum EnergyLinkType
        {
            [Description("Disabled")]
            DISABLED = 0,
            [Description("Enabled")]
            ENABLED  = 1
        }

        public enum GoalDisplaySetting
        {
            FULL = 0,
            SONG = 1,
            GEMS = 2,
            BOTH = 3
        }
        public class SongMetadata
        {
            public long Loc1Id { get; set; }
            public long Loc2Id { get; set; }
            public long Loc3Id { get; set; }
            public long ItemId { get; set; }
            public string Source { get; set; }
            public string Instrument { get; set; }

            public static SongMetadata FromArray(JArray array)
            {
                var result = new SongMetadata
                {
                    Loc1Id = array[0].ToObject<long>(),
                    Loc2Id = array[1].ToObject<long>(),
                    Loc3Id = array[2].ToObject<long>(),
                    ItemId = array[3].ToObject<long>(),
                    Source = array[4].ToObject<string>()
                };

                if (array.Count > 5)
                    result.Instrument = array[5].ToObject<string>();
                else
                    result.Instrument = null;

                return result;
            }
        }

        public class YargSlotData
        {
            public string                           GoalSong           { get; set; }
            public string                           GoalSongSource     { get; set; }
            public Dictionary<string, SongMetadata> Songlist           { get; set; }
            public int                              GemsRequired       { get; set; }
            public int                              GoalSongVisibility { get; set; }
            public int                              DeathLink          { get; set; }
            public int                              EnergyLink         { get; set; }
            public int                              InstrumentShuffle  { get; set; }

            public static YargSlotData Parse(Dictionary<string, object> slotData)
            {
                var result = new YargSlotData
                {
                    GoalSong = slotData["Goal Song"].ToString(),
                    GoalSongSource = slotData["Goal Song Source"].ToString(),
                    GemsRequired = Convert.ToInt32(slotData["Gems Required"]),
                    GoalSongVisibility = Convert.ToInt32(slotData["Goal Song Visibility"]),
                    DeathLink = Convert.ToInt32(slotData["Death Link"]),
                    EnergyLink = Convert.ToInt32(slotData["Energy Link"]),
                    InstrumentShuffle = Convert.ToInt32(slotData["Instrument Shuffle"]),
                    Songlist = new Dictionary<string, SongMetadata>()
                };

                var songlistJson = slotData["songlist"] as JObject;
                foreach (var song in songlistJson)
                    result.Songlist[song.Key] = SongMetadata.FromArray(song.Value as JArray);

                return result;
            }
        }
        public static readonly Dictionary<Instrument, string> YargInstrumentToAPKey = new Dictionary<Instrument, string>
        {
            { Instrument.FiveFretGuitar, "guitar5F" },
            { Instrument.FiveFretBass, "bass5F" },
            { Instrument.FiveFretRhythm, "rhythm5F" },
            { Instrument.FiveFretCoopGuitar, "coop5F" },

            { Instrument.SixFretGuitar, "guitar6F" },
            { Instrument.SixFretBass, "bass6F" },
            { Instrument.SixFretRhythm, "rhythm6F" },
            { Instrument.SixFretCoopGuitar, "coop6F" },

            { Instrument.FourLaneDrums, "drums" },
            { Instrument.ProDrums, "drums" },
            { Instrument.FiveLaneDrums, "drums" },
            { Instrument.EliteDrums, "drumsElite" },

            { Instrument.Keys, "keys5F" },
            { Instrument.ProKeys, "keysPro" },

            { Instrument.Vocals, "vocals" },
            //{ Instrument.Harmony, "harmony2" }
        };

        public static Dictionary<string, string> APInstrumentKeyToName = new Dictionary<string, string>
        {
            { "guitar5F", "Guitar" },
            { "bass5F", "Bass" },
            { "rhythm5F", "Rhythm" },
            { "coop5F", "Co-op"},
            { "guitar6F", "6 Fret Guitar" },
            { "bass6F", "6 Fret Bass" },
            { "rhythm6F", "6 Fret Rhythm" },
            { "coop6F", "6 Fret Co-op"},
            { "drums", "Drums" },
            { "drumsElite", "Elite Drums" },
            { "keys5F", "Keys" },
            { "keysPro", "Pro Keys" },
            { "vocals", "Vocals" },
            { "harmony2", "2 Part Harmony" },
            { "harmony3", "3 Part Harmony" }
        };
    }
}
