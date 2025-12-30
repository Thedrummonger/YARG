using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YARG.Assets.Script.YargAP
{
    internal class APData
    {
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
