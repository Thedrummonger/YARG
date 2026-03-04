using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using YARG.Menu.Persistent;

namespace YARG.Assets.Script.Yarchipelago
{
    internal class ApToastManager
    {
        public static void APToastMessage(string text, Action onClick = null)
            => ToastManager.AddToast(100, text, onClick);
        public static void APToastInformation(string text, Action onClick = null)
            => ToastManager.AddToast(101, text, onClick);
        public static void APToastSuccess(string text, Action onClick = null)
            => ToastManager.AddToast(102, text, onClick);
        public static void APToastWarning(string text, Action onClick = null)
            => ToastManager.AddToast(103, text, onClick);
        public static void APToastError(string text, Action onClick = null)
            => ToastManager.AddToast(104, text, onClick);
        public static void APToastJunkItem(string text, Action onClick = null)
            => ToastManager.AddToast(105, text, onClick);
        public static void APToastStandardItem(string text, Action onClick = null)
            => ToastManager.AddToast(106, text, onClick);
        public static void APToastProgressionItem(string text, Action onClick = null)
            => ToastManager.AddToast(107, text, onClick);

        public static bool HandleAPToasts(int type, string body, Action onClick, Component component, Toast _toastPrefab,
            Color _generalColor, Color _informationColor, Color _successColor, Color _warningColor, Color _errorColor)
        {
            if (type < 100) return false;

            var (text, color, icon) = type switch
            {
                100 => ("Archipelago", _generalColor, ColorHelper.APWhiteIcon),
                101 => ("Archipelago", _informationColor, ColorHelper.APWhiteIcon),
                102 => ("Archipelago", _successColor, ColorHelper.APWhiteIcon),
                103 => ("Archipelago", _warningColor, ColorHelper.APWhiteIcon),
                104 => ("Archipelago", _errorColor, ColorHelper.APWhiteIcon),
                105 => ("Archipelago", Color.cyan, ColorHelper.APBlueIcon),
                106 => ("Archipelago", Color.slateBlue, ColorHelper.APBlueIcon),
                107 => ("Archipelago", Color.plum, ColorHelper.APColorIcon),
                _ => throw new ArgumentException($"Invalid toast type {type}!")
            };

            var toast = UnityEngine.Object.Instantiate(_toastPrefab, component.transform);
            toast.Initialize(text, body, icon, color, onClick);
            return true;
        }
    }
}