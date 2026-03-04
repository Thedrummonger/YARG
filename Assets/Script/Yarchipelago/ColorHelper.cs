using Archipelago.MultiClient.Net.MessageLog.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YARG.Assets.Script.Yarchipelago
{
    public static class ColorHelper
    {
        public static string[] Colors = new[]
        {
            "#C97682",
            "#75C275",
            "#CA94C2",
            "#D9A07D",
            "#767EBD",
            "#EEE391"
        };
        public static string ToYargColoredString(this LogMessage message)
        {
            var result = new StringBuilder();
            foreach (var i in message.Parts)
            {
                var hexColor = $"#{i.Color.R:X2}{i.Color.G:X2}{i.Color.B:X2}";
                result.Append($"<color={hexColor}>{i.Text}</color>");
            }
            return result.ToString();
        }
        public static string ToRainbowString(this string input)
        {
            var result = new StringBuilder();
            int colorIndex = 0;
            foreach (char c in input)
            {
                if (char.IsWhiteSpace(c))
                    result.Append(c);
                else
                {
                    result.Append($"<color={Colors[colorIndex]}>{c}</color>");
                    colorIndex = colorIndex + 1 >= Colors.Length ? 0 : colorIndex + 1;
                }
            }
            return result.ToString();
        }
    }
}
