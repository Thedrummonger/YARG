using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using JetBrains.Annotations;
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
            public SongEntry GetYargSongEntry() => SongContainer.Songs.FirstOrDefault(x => x.Name == SongName && x.Source.Original == SongSource);
            public bool HasReceivedSong() => APEvents.IsConnected &&
                APEvents.Session.Items.AllItemsReceived.Any(x => x.ItemId == ItemID);

            public bool MatchesSongEntry(SongEntry entry) => SongName == entry.Name && SongSource == entry.Source.Original;

            public abstract bool CanCompleteLocation();

            public abstract bool VisibleInSongList();

            public bool MetPlayRequirements(GameManager gameManager)
            {
                //TODO when this is added
                return true;
            }
        }

        public class APGoalSong : APSongData
        {
            public APGoalSong(string name, string source, long itemID, int goalItemNeeded)
            {
                SongName = name;
                SongSource = source;
                ItemID = itemID;
                GoalItemNeeded =  goalItemNeeded;
            }
            public int GoalItemCount  { get; private set; }  = 0;
            public int GoalItemNeeded { get; }
            public bool HasEnoughYargGems() => GoalItemCount >= GoalItemNeeded;

            public void UpdateGoalItems() =>
                GoalItemCount = APEvents.IsConnected
                    ? APEvents.Session.Items.AllItemsReceived.Count(x => x.ItemId == (long) APData.APFiller.YargGem)
                    : 0;
            public override bool CanCompleteLocation() => HasReceivedSong() && HasEnoughYargGems();

            public override bool VisibleInSongList() =>
                APEvents.GoalDisplaySetting == APData.GoalDisplaySetting.FULL ||
                (HasReceivedSong() && APEvents.GoalDisplaySetting == APData.GoalDisplaySetting.SONG) ||
                (HasEnoughYargGems() && APEvents.GoalDisplaySetting == APData.GoalDisplaySetting.GEMS) ||
                (HasReceivedSong() && HasEnoughYargGems() && APEvents.GoalDisplaySetting == APData.GoalDisplaySetting.BOTH);
        }
        public class APSongLocation : APSongData
        {
            public APSongLocation(string name, string source, long loc1ID, long loc2ID, long itemID)
            {
                SongName = name;
                SongSource = source;
                ItemID = itemID;
                LocationID1 = loc1ID;
                LocationID2 = loc2ID;
            }
            public long LocationID1;
            public long LocationID2;
            public bool HasCheckedBothLocations() => APEvents.IsConnected &&
                APEvents.Session.Locations.AllLocationsChecked.Contains(LocationID1) &&
                APEvents.Session.Locations.AllLocationsChecked.Contains(LocationID2);
            public override bool VisibleInSongList() => HasReceivedSong() && !HasCheckedBothLocations();
            public override bool CanCompleteLocation() => HasReceivedSong() && !HasCheckedBothLocations();
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
    }
}
