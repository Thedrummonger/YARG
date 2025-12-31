using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YARG.Core;

namespace YARG.Assets.Script.YargAP
{
    internal class APData
    {
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

        public static bool NeedsRegen = true;
        public enum APFiller
        {
            YargGem = 1,
            StarPower = 2
        }
        public enum DeathLinkType
        {
            Fail = 1,
            RockMeter = 2
        }

        public enum GoalDisplaySetting
        {
            alwaysvisible = 0,
            song = 1,
            gems = 2,
            both = 3
        }

        public static Dictionary<string, int[]> SongNames = new Dictionary<string, int[]>();

        private static Dictionary<string, long[]> _SongNameToAPLocations;
        public static Dictionary<string, long[]> SongHashToAPLocations()
        {
            if (_SongNameToAPLocations is not null && !NeedsRegen)
                return _SongNameToAPLocations;
            _SongNameToAPLocations = new();
            foreach (var i in SongNames)
                _SongNameToAPLocations[i.Key] = new long[] { i.Value[0], i.Value[1] };
            NeedsRegen = false;
            return _SongNameToAPLocations;
        }

        private static Dictionary<long, string> _APItemIDToSongName;
        public static Dictionary<long, string> APItemIDToHash()
        {
            if (_APItemIDToSongName is not null && !NeedsRegen)
                return _APItemIDToSongName;
            _APItemIDToSongName = new();
            foreach (var i in SongNames)
                _APItemIDToSongName[i.Value[2]] = i.Key;
            NeedsRegen = false;
            return _APItemIDToSongName;
        }
    }
}
