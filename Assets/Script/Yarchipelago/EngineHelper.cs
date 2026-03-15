using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YARG.Core.Audio;
using YARG.Core.Engine;
using YARG.Gameplay;
using YARG.Gameplay.Player;

namespace YARG.Assets.Script.Yarchipelago
{
    public static class EngineHelper
    {
        private static MethodInfo _addHappinessMethod;
        private static PropertyInfo _happinessProperty;
        private static MethodInfo _updateHappinessMethod;
        private static FieldInfo _engineContainerField;
        public static EngineManager.EngineContainer GetEngineContainer(this BasePlayer player)
        {
            if (_engineContainerField == null)
                _engineContainerField = typeof(BasePlayer).GetField("EngineContainer", BindingFlags.NonPublic | BindingFlags.Instance);

            return (EngineManager.EngineContainer) _engineContainerField.GetValue(player);
        }

        public static void AddHappiness(this EngineManager.EngineContainer container, float delta)
        {
            if (_addHappinessMethod == null)
                _addHappinessMethod = typeof(EngineManager.EngineContainer).GetMethod("AddHappiness", BindingFlags.NonPublic | BindingFlags.Instance);

            _addHappinessMethod?.Invoke(container, new object[] { delta });
        }

        public static void SetHappiness(this EngineManager.EngineContainer container, EngineManager engineManager, float value)
        {
            if (_happinessProperty == null)
                _happinessProperty = typeof(EngineManager.EngineContainer).GetProperty("Happiness", BindingFlags.Public | BindingFlags.Instance);

            if (_updateHappinessMethod == null)
                _updateHappinessMethod = typeof(EngineManager).GetMethod("UpdateHappiness", BindingFlags.NonPublic | BindingFlags.Instance);

            value = Math.Clamp(value, -3f, 1f);

            _happinessProperty?.SetValue(container, value);

            _updateHappinessMethod?.Invoke(engineManager, null);
        }

    }
}
