using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YARG.Assets.Script.Yarchipelago;
using YARG.Core.Song;
using YARG.Menu.ListMenu;
using YARG.Menu.MusicLibrary;
using YARG.Menu.Persistent;

namespace YARG.Menu.MusicLibrary
{
    public partial class MusicLibraryMenu : ListMenu<ViewType, SongView>
    {
        /// Archipelago integration:
        /// Inserts Archipelago menu entries into the music library menu before the
        /// standard Random Song / Playlists buttons.
        ///<see cref="YARG.Menu.MusicLibrary.MusicLibraryMenu.CreateNormalViewList"/>.
        /// <code>
        /// if (!_searchField.IsSearching)
        /// {
        ///     AddAPMenuItems(list); // Archipelago
        /// }
        /// </code>
        public void AddAPMenuItems(List<ViewType> list)
        {
            var AvailableSongs = Events.APSongLocations.Where(x => x.VisibleInSongList()).ToArray();
            var ShouldDisplayGoalSong = Events.APGoalSong?.VisibleInSongList() ?? false;

            if (ShouldDisplayGoalSong)
            {
                if (Events.GoalDisplaySetting == Models.GoalDisplaySetting.SONG ||
                    Events.GoalDisplaySetting == Models.GoalDisplaySetting.FULL ||
                    Events.GoalDisplaySetting == Models.GoalDisplaySetting.BOTH)
                    list.Add(new CategoryViewType($"Gems {Events.APGoalSong?.GoalItemCount}\\{Events.APGoalSong?.GoalItemNeeded}", 0,
                        Array.Empty<SongEntry>(), () => RefreshAndReselect(false, true)));

                if (Events.GoalDisplaySetting == Models.GoalDisplaySetting.GEMS ||
                    Events.GoalDisplaySetting == Models.GoalDisplaySetting.FULL ||
                    Events.GoalDisplaySetting == Models.GoalDisplaySetting.BOTH)
                    list.Add(new CategoryViewType($"Goal Song Item: {(Events.APGoalSong?.HasReceivedSong() ?? false ? "Found" : "Missing")}", 0,
                        Array.Empty<SongEntry>(), () => RefreshAndReselect(false, true)));
            }

            var goalSong = Events.APGoalSong?.GetYargSongEntry();
            if (goalSong == null)
                ShouldDisplayGoalSong = false;

            if (ShouldDisplayGoalSong)
            {
                if (String.IsNullOrWhiteSpace(Events.APGoalSong.Instrument))
                    list.Add(new CategoryViewType("AP Goal Song".ToRainbowString(), 1, new SongEntry[] { goalSong }, () => RefreshAndReselect(false, true)));
                else
                {
                    string instname = Models.APInstrumentKeyToName[Events.APGoalSong.Instrument];
                    string instrumentDisplay = Events.APGoalSong.HasReceiveInstrumentItem()
                        ? $"<color=#00FF88>{instname}</color>"
                        : $"<color=#FF4040>{instname}</color>";
                    list.Add(new CategoryViewType($"{"AP Goal Song".ToRainbowString()}: {instrumentDisplay}", 1,
                        new SongEntry[] { goalSong }, () => RefreshAndReselect(false, true)));
                }
                list.Add(new SongViewType(this, goalSong));
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
                    list.Add(new CategoryViewType("AP Songs".ToRainbowString(), availableAPSongs.Count, availableAPSongs.ToArray(), () => RefreshAndReselect(false, true)));
                    foreach (var song in availableAPSongs)
                        list.Add(new SongViewType(this, song));
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
                            header.Value.ToArray(), () => RefreshAndReselect(false, true)));
                        foreach (var song in header.Value)
                            list.Add(new SongViewType(this, song));
                    }
                }
            }
        }
    }
}
