using System.IO;
using GameNetcodeStuff;
using UnityEngine;

namespace NildoFullbrightLite
{
    public class LiteBehaviour : MonoBehaviour
    {
        private Rect _win = new Rect(60, 60, 340, 250);
        private bool _open;
        private Texture2D _logo, _bg;
        private bool _logoTried;
        private GUIStyle _title, _sub, _winStyle;
        private CursorLockMode _prevLock;
        private bool _prevVisible;

        // valores originais da luz de visao noturna
        private bool _nvActive, _nvEnabled;
        private float _nvIntensity, _nvRange;

        internal static bool InGame =>
            StartOfRound.Instance != null && StartOfRound.Instance.localPlayerController != null;

        private void Update()
        {
            if (Plugin.MenuKey.Value.IsDown()) Toggle();
            if (Plugin.NightKey.Value.IsDown()) Plugin.NightVision.Value = !Plugin.NightVision.Value;
            if (Plugin.FogKey.Value.IsDown()) Plugin.FogRemover.Value = !Plugin.FogRemover.Value;

            FogRemover.Tick();

            if (InGame) UpdateNightVision(StartOfRound.Instance.localPlayerController);
            else _nvActive = false;   // a luz e recriada ao trocar de cena
        }

        private void UpdateNightVision(PlayerControllerB me)
        {
            var l = me.nightVision;
            if (l == null) return;

            if (Plugin.NightVision.Value)
            {
                if (!_nvActive) { _nvIntensity = l.intensity; _nvRange = l.range; _nvEnabled = l.enabled; _nvActive = true; }
                l.enabled = true;
                l.intensity = 2000f * Plugin.NightIntensity.Value;
                l.range = 600f;
            }
            else if (_nvActive)
            {
                l.intensity = _nvIntensity; l.range = _nvRange; l.enabled = _nvEnabled; _nvActive = false;
            }
        }

        // ================= menu =================
        private void Toggle()
        {
            _open = !_open;
            if (_open) { _prevLock = Cursor.lockState; _prevVisible = Cursor.visible; }
            else { Cursor.lockState = _prevLock; Cursor.visible = _prevVisible; }
        }

        private void LateUpdate()
        {
            if (_open) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        }

        private void OnGUI()
        {
            if (!_open) return;
            EnsureStyles();
            _win = GUI.Window(0x4E46, _win, DrawWindow, "", _winStyle);
        }

        private void DrawWindow(int id)
        {
            GUILayout.BeginHorizontal();
            var logo = GetLogo();
            if (logo != null) GUILayout.Label(logo, GUILayout.Width(44), GUILayout.Height(44));
            GUILayout.BeginVertical();
            GUILayout.Label(PluginInfo.NAME, _title);
            GUILayout.Label("v" + PluginInfo.VERSION + "  |  by NildoMods", _sub);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(8);

            Plugin.NightVision.Value = GUILayout.Toggle(Plugin.NightVision.Value, " Visao noturna  (" + Plugin.NightKey.Value + ")");
            if (Plugin.NightVision.Value)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Intensidade: " + Plugin.NightIntensity.Value.ToString("0.0"), GUILayout.Width(130));
                Plugin.NightIntensity.Value = GUILayout.HorizontalSlider(Plugin.NightIntensity.Value, 0.2f, 10f);
                GUILayout.EndHorizontal();
            }

            Plugin.FogRemover.Value = GUILayout.Toggle(Plugin.FogRemover.Value, " Fog Remover  (" + Plugin.FogKey.Value + ")");

            GUILayout.Space(8);
            GUILayout.Label("Menu: " + Plugin.MenuKey.Value, _sub);

            GUI.DragWindow(new Rect(0, 0, 10000, 50));
        }

        private void EnsureStyles()
        {
            if (_title != null) return;
            _title = new GUIStyle(GUI.skin.label) { fontSize = 17, fontStyle = FontStyle.Bold };
            _title.normal.textColor = new Color(0.85f, 0.85f, 0.9f);
            _sub = new GUIStyle(GUI.skin.label) { fontSize = 12, fontStyle = FontStyle.Italic };

            _bg = new Texture2D(1, 1);
            _bg.SetPixel(0, 0, new Color(0.07f, 0.07f, 0.1f, 0.96f));
            _bg.Apply();
            _winStyle = new GUIStyle(GUI.skin.window);
            _winStyle.normal.background = _bg;
            _winStyle.onNormal.background = _bg;
            _winStyle.padding = new RectOffset(10, 10, 8, 10);
        }

        private Texture2D GetLogo()
        {
            if (_logoTried) return _logo;
            _logoTried = true;
            try
            {
                string dir = Path.GetDirectoryName(typeof(Plugin).Assembly.Location);
                string path = Path.Combine(dir, "icon.png");
                if (File.Exists(path))
                {
                    var tex = new Texture2D(2, 2);
                    if (ImageConversion.LoadImage(tex, File.ReadAllBytes(path))) _logo = tex;
                }
            }
            catch { }
            return _logo;
        }
    }
}
