using Archipelago.MultiClient.Net.MessageLog.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

        public static Sprite APBlueIcon = LoadSpriteFromResources("Archipelago/blue-icon");
        public static Sprite APColorIcon = LoadSpriteFromResources("Archipelago/color-icon");
        public static Sprite APWhiteIcon = LoadSpriteFromResources("Archipelago/white-icon");
        public static Sprite LoadSpriteFromResources(string path)
        {
            var sprite = Resources.Load<Sprite>(path);
            if (sprite != null) return sprite;

            // Fallback: if it imported as Texture2D instead of Sprite
            var tex = Resources.Load<Texture2D>(path);
            if (tex == null) throw new System.Exception($"Missing resource at: Resources/{path}");

            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
