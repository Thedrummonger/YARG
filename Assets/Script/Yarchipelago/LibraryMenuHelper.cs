using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using YARG.Core.Song;
using YARG.Menu.MusicLibrary;
using YARG.Menu.Persistent;

namespace YARG.Assets.Script.Yarchipelago
{
    public static class LibraryMenuHelper
    {
        public static void AddAPMenuItems(this MusicLibraryMenu library, List<ViewType> list, Action Refresh)
        {
            var AvailableSongs = Events.APSongLocations.Where(x => x.VisibleInSongList()).ToArray();
            var ShouldDisplayGoalSong = Events.APGoalSong?.VisibleInSongList() ?? false;

            if (ShouldDisplayGoalSong)
            {
                if (Events.GoalDisplaySetting == Models.GoalDisplaySetting.SONG ||
                    Events.GoalDisplaySetting == Models.GoalDisplaySetting.FULL ||
                    Events.GoalDisplaySetting == Models.GoalDisplaySetting.BOTH)
                    list.Add(new CategoryViewType($"Gems {Events.APGoalSong?.GoalItemCount}\\{Events.APGoalSong?.GoalItemNeeded}", 0,
                        Array.Empty<SongEntry>(), Refresh));

                if (Events.GoalDisplaySetting == Models.GoalDisplaySetting.GEMS ||
                    Events.GoalDisplaySetting == Models.GoalDisplaySetting.FULL ||
                    Events.GoalDisplaySetting == Models.GoalDisplaySetting.BOTH)
                    list.Add(new CategoryViewType($"Goal Song Item: {(Events.APGoalSong?.HasReceivedSong() ?? false ? "Found" : "Missing")}", 0,
                        Array.Empty<SongEntry>(), Refresh));
            }

            var goalSong = Events.APGoalSong?.GetYargSongEntry();
            if (goalSong == null)
                ShouldDisplayGoalSong = false;

            if (ShouldDisplayGoalSong)
            {
                if (String.IsNullOrWhiteSpace(Events.APGoalSong.Instrument))
                    list.Add(new CategoryViewType("AP Goal Song".ToRainbowString(), 1, new SongEntry[] { goalSong }, Refresh));
                else
                {
                    string instname = Models.APInstrumentKeyToName[Events.APGoalSong.Instrument];
                    string instrumentDisplay = Events.APGoalSong.HasReceiveInstrumentItem()
                        ? $"<color=#00FF88>{instname}</color>"
                        : $"<color=#FF4040>{instname}</color>";
                    list.Add(new CategoryViewType($"{"AP Goal Song".ToRainbowString()}: {instrumentDisplay}", 1,
                        new SongEntry[] { goalSong }, Refresh));
                }
                list.Add(new SongViewType(library, goalSong));
            }

            if (AvailableSongs.Any())
            {
                var availableAPSongs = new List<SongEntry>();
                var availableAPSongsWithHeader = new Dictionary<string, List<SongEntry>>();
                var headerAquireStatus = new Dictionary<string, bool>();
                foreach (var apSong in AvailableSongs)
                {
                    var song = apSong.GetYargSongEntry();
                    if (song != null)
                        if (String.IsNullOrWhiteSpace(apSong.Instrument))
                            availableAPSongs.Add(song);
                        else
                        {
                            if (!availableAPSongsWithHeader.ContainsKey(apSong.Instrument))
                                availableAPSongsWithHeader[apSong.Instrument] = new List<SongEntry>();
                            availableAPSongsWithHeader[apSong.Instrument].Add(song);
                            headerAquireStatus[apSong.Instrument] = apSong.HasReceiveInstrumentItem();
                        }
                    else
                        ToastManager.ToastError($"Failed to find song with song hash {apSong}!\nEnsure you are using the YARG official setlist!");
                }
                if (availableAPSongs.Count > 0)
                {
                    list.Add(new CategoryViewType("AP Songs".ToRainbowString(), availableAPSongs.Count, availableAPSongs.ToArray(), Refresh));
                    foreach (var song in availableAPSongs)
                        list.Add(new SongViewType(library, song));
                }

                if (availableAPSongsWithHeader.Count > 0)
                {
                    foreach (var header in availableAPSongsWithHeader)
                    {
                        string instname = Models.APInstrumentKeyToName[header.Key];
                        string instrumentDisplay = headerAquireStatus[header.Key]
                            ? $"<color=#00FF88>{instname}</color>"
                            : $"<color=#FF4040>{instname}</color>";
                        list.Add(new CategoryViewType($"{"AP Songs".ToRainbowString()}: {instrumentDisplay}", header.Value.Count,
                            header.Value.ToArray(), Refresh));
                        foreach (var song in header.Value)
                            list.Add(new SongViewType(library, song));
                    }
                }
            }
        }
    }
}
