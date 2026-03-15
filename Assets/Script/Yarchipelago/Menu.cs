using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace YARG.Assets.Script.Yarchipelago
{
    public partial class ArchipelagoConnectionDialog : MonoBehaviour
    {
        public static ArchipelagoConnectionDialog Instance { get; private set; }

        [Header("State")]
        public bool Show = false;

        private Rect _windowRect = new Rect(20, 20, 430, 310);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            var Cache = ConnectionHelper.LoadConnectionCache();
            if (Cache is not null)
            {
                address = $"{Cache.IP}:{Cache.Port}";
                slotName = Cache.SlotName;
                password = Cache.Password;
                connectionSuffix = Cache.ConnectionSuffix;
            }
        }

        private void OnGUI()
        {
            if (!Show) return;

            _windowRect = GUI.Window(0xA1C4, _windowRect, DrawWindow, "Archipelago Connection");
        }
        private void DrawWindow(int id)
        {
            if (ChatOpen)
                DrawChatUI();
            else
                DrawConnectionUI();
        }

        private readonly struct GUIEnabledScope : System.IDisposable
        {
            private readonly bool _prev;
            public GUIEnabledScope(bool enabled)
            {
                _prev = GUI.enabled;
                GUI.enabled = enabled;
            }
            public void Dispose() => GUI.enabled = _prev;
        }

        public static void ToggleArchipelagoDialog()
        {
            var dialog = GetOrCreateApDialog();
            dialog.Show = !dialog.Show;
        }

        public static ArchipelagoConnectionDialog GetOrCreateApDialog()
        {
            if (Instance != null)
                return Instance;

            var DialogObject = new GameObject("ArchipelagoConnectionDialog");
            DontDestroyOnLoad(DialogObject);

            return DialogObject.AddComponent<ArchipelagoConnectionDialog>();
        }
    }
}
