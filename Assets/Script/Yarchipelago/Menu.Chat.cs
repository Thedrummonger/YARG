using Archipelago.MultiClient.Net.MessageLog.Messages;
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
        private Vector2 _chatScrollPosition = Vector2.zero;
        public static List<LogMessage> ChatHistory = new List<LogMessage>();
        GUIStyle richTextStyle = null;
        private string _chatInputText = "";
        private int _lastChatCount = 0;
        private float _contentHeight = 0;

        public bool ChatOpen = false;

        private void DrawChatUI()
        {
            _chatScrollPosition = GUILayout.BeginScrollView(_chatScrollPosition, GUILayout.Height(190));

            if (richTextStyle == null)
            {
                richTextStyle = new GUIStyle(GUI.skin.label);
                richTextStyle.richText = true;
            }

            GUILayout.BeginVertical();
            int startIndex = Mathf.Max(0, ChatHistory.Count - 500);
            for (int i = startIndex; i < ChatHistory.Count; i++)
                GUILayout.Label(ChatHistory[i].ToYargColoredString(), richTextStyle);
            GUILayout.EndVertical();

            if (Event.current.type == EventType.Repaint)
                _contentHeight = GUILayoutUtility.GetLastRect().height;

            GUILayout.EndScrollView();

            float maxScroll = Mathf.Max(0, _contentHeight - 190);
            bool isAtBottom = _chatScrollPosition.y >= maxScroll - 10;

            if (ChatHistory.Count > _lastChatCount)
            {
                if (isAtBottom || _lastChatCount == 0)
                    _chatScrollPosition.y = float.MaxValue;

                _lastChatCount = ChatHistory.Count;
            }

            GUILayout.Space(16);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Message", GUILayout.Width(80));

            _chatInputText = GUILayout.TextField(_chatInputText, GUILayout.Width(240));

            using (new GUIEnabledScope(Events.IsConnected))
                if (GUILayout.Button("Send", GUILayout.Height(22), GUILayout.Width(80)))
                    if (!string.IsNullOrWhiteSpace(_chatInputText))
                    {
                        Events.Session.Say(_chatInputText);
                        _chatInputText = "";
                        GUI.FocusControl(null);
                        GUIUtility.keyboardControl = 0;
                    }

            GUILayout.EndHorizontal();

            GUILayout.Space(8);

            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Close", GUILayout.Height(28)))
                    Show = false;

                if (GUILayout.Button("Show Connection Window", GUILayout.Height(28)))
                    ChatOpen = !ChatOpen;
            }

            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }
    }
}
