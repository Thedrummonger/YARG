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
            YargGem = 1
        }
        public enum DeathLinkType
        {
            Fail = 1,
            RockMeter = 2
        }

        public static Dictionary<string, object[]> SongNames = new Dictionary<string, object[]>();

        private static Dictionary<long, string> _APLocationIDToSongName;
        public static Dictionary<long, string> APLocationIDToHash()
        {
            if (_APLocationIDToSongName is not null && !NeedsRegen)
                return _APLocationIDToSongName;
            _APLocationIDToSongName = new();
            var Index = 1; //Song locations start at index 1 in the apworld 
            foreach(var i in SongNames)
            {
                _APLocationIDToSongName[Index] = i.Key;
                Index++;
                _APLocationIDToSongName[Index] = i.Key;
                Index++;
            }
            NeedsRegen = false;
            return _APLocationIDToSongName;
        }

        private static Dictionary<string, long[]> _SongNameToAPLocations;
        public static Dictionary<string, long[]> SongHashToAPLocations()
        {
            if (_SongNameToAPLocations is not null && !NeedsRegen)
                return _SongNameToAPLocations;
            _SongNameToAPLocations = new();
            int Index = 1; //Song locations start at index 1 in the apworld 
            foreach (var i in SongNames)
            {
                _SongNameToAPLocations[i.Key] = new long[] { Index, Index + 1 };
                Index += 2;
            }
            NeedsRegen = false;
            return _SongNameToAPLocations;
        }

        private static Dictionary<long, string> _APItemIDToSongName;
        public static Dictionary<long, string> APItemIDToHash()
        {
            if (_APItemIDToSongName is not null && !NeedsRegen)
                return _APItemIDToSongName;
            _APItemIDToSongName = new();
            var Index = 2; //Song Items start at index 2 in the apworld, index 1 is Yarg Gem
            foreach (var i in SongNames)
            {
                _APItemIDToSongName[Index] = i.Key;
                Index++;
            }
            NeedsRegen = false;
            return _APItemIDToSongName;
        }
    }
}
