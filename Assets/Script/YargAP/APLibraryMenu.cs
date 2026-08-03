using System;
using System.Collections.Generic;
using System.Linq;
using YARG.Core.Song;
using YARG.Menu.MusicLibrary;
using YARG.Menu.Persistent;

namespace YARG.Assets.Script.YargAP
{
    public static class APLibraryMenu
    {
        public static void AddAPMenuItems(this MusicLibraryMenu library, List<ViewType> list, Action Refresh)
        {
            var AvailableSongs = APEvents.APSongLocations.Where(x => x.VisibleInSongList()).ToArray();
            var ShouldDisplayGoalSong = APEvents.APGoalSong?.VisibleInSongList() ?? false;

            if (ShouldDisplayGoalSong)
            {
                if (APEvents.GoalDisplaySetting == APData.GoalDisplaySetting.SONG ||
                    APEvents.GoalDisplaySetting == APData.GoalDisplaySetting.FULL ||
                    APEvents.GoalDisplaySetting == APData.GoalDisplaySetting.BOTH)
                    list.Add(new CategoryViewType($"Gems {APEvents.APGoalSong?.GoalItemCount}\\{APEvents.APGoalSong?.GoalItemNeeded}", 0,
                        Array.Empty<SongEntry>(), Refresh));

                if (APEvents.GoalDisplaySetting == APData.GoalDisplaySetting.GEMS ||
                    APEvents.GoalDisplaySetting == APData.GoalDisplaySetting.FULL ||
                    APEvents.GoalDisplaySetting == APData.GoalDisplaySetting.BOTH)
                    list.Add(new CategoryViewType($"Goal Song Item: {(APEvents.APGoalSong?.HasReceivedSong()??false ? "Found" : "Missing")}", 0,
                        Array.Empty<SongEntry>(), Refresh));
            }

            var goalSong = APEvents.APGoalSong?.GetYargSongEntry();
            if (goalSong == null)
                ShouldDisplayGoalSong = false;

            if (ShouldDisplayGoalSong)
            {
                if (String.IsNullOrWhiteSpace(APEvents.APGoalSong.Instrument))
                    list.Add(new CategoryViewType("AP Goal Song".ToRainbowString(), 1, new SongEntry[] { goalSong }, Refresh));
                else
                {
                    string instname = APData.APInstrumentKeyToName[APEvents.APGoalSong.Instrument];
                    string instrumentDisplay = APEvents.APGoalSong.HasReceiveInstrumentItem()
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
                        string instname = APData.APInstrumentKeyToName[header.Key];
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