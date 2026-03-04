using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
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
    }
}
