using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using UnityEditor.PackageManager;
using UnityEngine;
using YARG.Menu.Persistent;

namespace YARG.Assets.Script.Yarchipelago
{
    public partial class ArchipelagoConnectionDialog : MonoBehaviour
    {
        [Header("Defaults")]
        [SerializeField] private string address = "archipelago.gg:38281";
        [SerializeField] private string slotName = "";
        [SerializeField] private string password = "";
        [SerializeField] private string connectionSuffix = "";

        private int DeathLinkOverride = Models.DeathLinkValues.Length;
        private int EnergyLinkOverride = Models.EnergyLinkValues.Length;

        private void DrawConnectionUI()
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.Width(180));
            GUILayout.Label("Address");
            using (new GUIEnabledScope(!Events.IsConnected))
                address = GUILayout.TextField(address);

            GUILayout.Label("Slot Name");
            using (new GUIEnabledScope(!Events.IsConnected))
                slotName = GUILayout.TextField(slotName);

            GUILayout.Label("Password");
            using (new GUIEnabledScope(!Events.IsConnected))
                password = GUILayout.PasswordField(password, '*');

            GUILayout.Label("Game ID");
            using (new GUIEnabledScope(!Events.IsConnected))
                connectionSuffix = GUILayout.TextField(connectionSuffix);

            GUILayout.Space(10);
            string buttonText = Events.IsConnected ? "Disconnect" : "Connect";
            if (GUILayout.Button(buttonText, GUILayout.Height(28)))
            {
                GUI.FocusControl(null);
                GUIUtility.keyboardControl = 0;
                if (Events.IsConnected)
                    ConnectionHelper.DoDisconnect();
                else
                    ConnectionHelper.DoConnect(address, slotName, password, connectionSuffix);
            }
            GUILayout.Space(6);
            if (GUILayout.Button("Close", GUILayout.Height(28)))
                Show = false;
            GUILayout.EndVertical();

            GUILayout.Space(20);

            GUILayout.BeginVertical(GUILayout.Width(190));

            GUILayout.Label($"Deathlink YAML: {(Events.IsConnected ? Events.DeathLinkYAML.GetDescription() : "N/A")}");

            GUILayout.Label("Deathlink Override");

            using (new GUIEnabledScope(Events.IsConnected && Events.DeathLinkService != null))
            {
                var Display = DeathLinkOverride == Models.DeathLinkValues.Length ? "Use Yaml" : Events.DeathLinkType.GetDescription();
                if (GUILayout.Button(Display, GUILayout.Height(20)))
                {
                    DeathLinkOverride++;
                    if (DeathLinkOverride > Models.DeathLinkValues.Length) DeathLinkOverride = 0;
                    if (DeathLinkOverride == Models.DeathLinkValues.Length)
                        Events.DeathLinkType = Events.DeathLinkYAML;
                    else
                        Events.DeathLinkType = Models.DeathLinkValues[DeathLinkOverride];
                    Events.UpdateDeathLinkTag();
                    Debug.Log($"{DeathLinkOverride} | {Events.DeathLinkType} | {Events.DeathLinkYAML}");
                }
            }

            GUILayout.Label($"Energylink YAML: {(Events.IsConnected ? Events.EnergyLinkYAML.GetDescription() : "N/A")}");

            GUILayout.Label("Energylink Override");
            using (new GUIEnabledScope(Events.IsConnected))
            {
                var Display = EnergyLinkOverride == Models.EnergyLinkValues.Length ? "Use Yaml" : Events.EnergyLinkType.GetDescription();
                if (GUILayout.Button(Display, GUILayout.Height(20)))
                {
                    EnergyLinkOverride++;
                    if (EnergyLinkOverride > Models.EnergyLinkValues.Length) EnergyLinkOverride = 0;
                    if (EnergyLinkOverride == Models.EnergyLinkValues.Length)
                        Events.EnergyLinkType = Events.EnergyLinkYAML;
                    else
                        Events.EnergyLinkType = Models.EnergyLinkValues[EnergyLinkOverride];
                    Debug.Log($"{EnergyLinkOverride} | {Events.EnergyLinkType} | {Events.EnergyLinkYAML}");
                }
            }

            GUILayout.Label("Chat settings");

            if (GUILayout.Button($"Print Chat Messages: {Events.PrintChatMessages}"))
                Events.PrintChatMessages = !Events.PrintChatMessages;

            if (GUILayout.Button($"Print Unrelated Items: {Events.PrintUnrelatedItems}"))
                Events.PrintUnrelatedItems = !Events.PrintUnrelatedItems;

            GUILayout.Space(25);

            if (GUILayout.Button($"Show Chat Window", GUILayout.Height(28)))
                ChatOpen = !ChatOpen;

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }
    }
}
