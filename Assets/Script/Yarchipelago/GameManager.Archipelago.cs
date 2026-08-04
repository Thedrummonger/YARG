using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using YARG.Assets.Script.Yarchipelago;
using YARG.Core.Audio;

namespace YARG.Gameplay
{
    public partial class GameManager : MonoBehaviour
    {
        public async void ForceSongFail()
        {
            PlayerHasFailed = true;
            _mixer.FadeOut(SONG_END_DELAY);
            await UniTask.Delay(TimeSpan.FromSeconds(SONG_END_DELAY));
            GlobalAudioHandler.PlayVoxSample(VoxSample.FailSound);
            Pause();
        }

        /// Archipelago integration:
        /// Added a DeathLink trigger when the player fails a song.
        /// <see cref="YARG.Gameplay.GameManager.OnSongFailed"/>,
        /// <code>
        /// if (!PlayerHasFailed)
        /// {
        ///     PlayerHasFailed = true;
        ///     FlagDeathLink(); // Archipelago
        ///     _mixer.FadeOut(SONG_END_DELAY);
        ///     await UniTask.Delay(TimeSpan.FromSeconds(SONG_END_DELAY));
        ///     GlobalAudioHandler.PlayVoxSample(VoxSample.FailSound);
        ///     Pause();
        /// }
        /// </code>
        public void FlagDeathLink()
        {
            if (Events.IsConnected && Events.DeathLinkService != null && Events.DeathLinkType > 0)
            {
                Events.DeathLinkService.SendDeathLink(new Archipelago.MultiClient.Net.BounceFeatures.DeathLink.DeathLink(
                    Events.Session.Players.ActivePlayer.Name, Events.GetRandomDeatLinkMessage(this, _players)));
            }
        }
    }
}
