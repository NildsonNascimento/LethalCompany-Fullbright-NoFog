using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;

namespace NildoFullbrightLite
{
    internal static class PluginInfo
    {
        public const string NAME    = "Nildo-Fullbright-Lite";
        public const string GUID    = "nildomods.fullbrightlite";
        public const string VERSION = "1.0.0";
    }

    [BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;

        internal static ConfigEntry<KeyboardShortcut> MenuKey, NightKey, FogKey;
        internal static ConfigEntry<bool> NightVision, FogRemover;
        internal static ConfigEntry<float> NightIntensity;

        private void Awake()
        {
            Log = Logger;

            MenuKey = Config.Bind("Keys", "OpenMenu", new KeyboardShortcut(KeyCode.Home), "Abre/fecha o menu.");
            NightKey = Config.Bind("Keys", "ToggleNightVision", new KeyboardShortcut(KeyCode.F7), "Liga/desliga a visao noturna.");
            FogKey = Config.Bind("Keys", "ToggleFogRemover", new KeyboardShortcut(KeyCode.F8), "Liga/desliga o Fog Remover.");

            NightVision = Config.Bind("Fullbright", "NightVision", false, "Visao noturna.");
            NightIntensity = Config.Bind("Fullbright", "NightVisionIntensity", 2f, "Intensidade da visao noturna (0.2 a 10).");
            FogRemover = Config.Bind("Fullbright", "FogRemover", false, "Remove a nevoa.");

            var go = new GameObject(PluginInfo.NAME + "_Host");
            go.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(go);
            go.AddComponent<LiteBehaviour>();

            Log.LogInfo(PluginInfo.NAME + " " + PluginInfo.VERSION + " carregado.");
        }
    }
}
