using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using YARG.Gameplay;
using YARG.Gameplay.Player;
using YARG.Menu.Persistent;
using static UnityEngine.Rendering.STP;

namespace YARG.Assets.Script.YargAP
{
    internal class ArchipelagoManager : MonoBehaviour
    {
        private void Start()
        {
            // I have attached this class to the persistant scene under integration so it runs when Yarg loads
            // For now it just connects to a local archipelago server with hardcoded credentials, but later the connection can
            // be made through the menu. This will probably be repurposed to just run any initialization code we need.
            ConnectTestSession();
        }
        public void ConnectTestSession()
        {
        }
    }
}
