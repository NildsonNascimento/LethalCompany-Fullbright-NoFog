using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

namespace NildoFullbrightLite
{
    /// <summary>
    /// Remove a nevoa SEM alterar os perfis do jogo: cria um Volume global proprio, com prioridade maxima,
    /// que sobrescreve Fog.enabled = false. Ligar/desligar so ativa/desativa esse objeto.
    /// Tambem desliga os LocalVolumetricFog que estavam ligados e os religa ao desativar.
    /// </summary>
    internal static class FogRemover
    {
        private static GameObject _go;
        private static Volume _vol;
        private static readonly Dictionary<Behaviour, bool> _local = new Dictionary<Behaviour, bool>();
        private static Type _localType;
        private static bool _localSearched;
        private static float _next;

        public static void Tick()
        {
            bool on = Plugin.FogRemover.Value && LiteBehaviour.InGame;

            if (!on)
            {
                if (_go != null && _go.activeSelf) _go.SetActive(false);
                RestoreLocal();
                return;
            }

            EnsureOverride();
            if (_go == null) return;
            if (!_go.activeSelf) _go.SetActive(true);

            if (Time.unscaledTime < _next) return;
            _next = Time.unscaledTime + 1f;

            try { SyncLayer(); DisableLocal(); }
            catch (Exception e) { Plugin.Log.LogDebug("Fog: " + e.Message); }
        }

        private static void EnsureOverride()
        {
            if (_go != null) return;
            try
            {
                _go = new GameObject("NildoFullbright_FogOverride");
                _go.hideFlags = HideFlags.HideAndDontSave;
                UnityEngine.Object.DontDestroyOnLoad(_go);

                _vol = _go.AddComponent<Volume>();
                _vol.isGlobal = true;
                _vol.priority = 10000f;
                _vol.weight = 1f;

                var prof = ScriptableObject.CreateInstance<VolumeProfile>();
                prof.hideFlags = HideFlags.HideAndDontSave;

                var fogType = FindType("Fog", "UnityEngine.Rendering.HighDefinition");
                if (fogType != null)
                {
                    var comp = prof.Add(fogType, false);
                    SetParam(comp, "enabled", true, false);
                    SetParam(comp, "enableVolumetricFog", true, false);
                }
                else Plugin.Log.LogWarning("Fog: tipo HDRP Fog nao encontrado.");

                _vol.sharedProfile = prof;
                SyncLayer();
            }
            catch (Exception e) { Plugin.Log.LogWarning("Fog: " + e.Message); }
        }

        // usa a mesma layer dos volumes do jogo, para a camera enxergar o nosso
        private static void SyncLayer()
        {
            if (_go == null) return;
            Volume pick = null;
            foreach (var v in UnityEngine.Object.FindObjectsOfType<Volume>())
            {
                if (v == null || v == _vol) continue;
                if (pick == null || (v.isGlobal && !pick.isGlobal)) pick = v;
            }
            if (pick != null) _go.layer = pick.gameObject.layer;
        }

        private static void DisableLocal()
        {
            var lt = _localSearched ? _localType : (_localType = FindType("LocalVolumetricFog", null) ?? FindType("DensityVolume", null));
            _localSearched = true;
            if (lt == null) return;

            foreach (var o in UnityEngine.Object.FindObjectsOfType(lt))
            {
                var b = o as Behaviour;
                if (b == null || !b.enabled) continue;
                _local[b] = true;
                b.enabled = false;
            }
        }

        private static void RestoreLocal()
        {
            if (_local.Count == 0) return;
            foreach (var kv in _local)
            {
                try { if (kv.Key != null) kv.Key.enabled = true; } catch { }
            }
            _local.Clear();
        }

        private static Type FindType(string name, string ns)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var t in asm.GetTypes())
                        if (t.Name == name && (ns == null || t.Namespace == ns)) return t;
                }
                catch { }
            }
            return null;
        }

        private static void SetParam(object comp, string name, bool overrideState, bool value)
        {
            try
            {
                var ct = comp.GetType();
                var f = ct.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                object p = f != null ? f.GetValue(comp) : ct.GetProperty(name)?.GetValue(comp, null);
                if (p == null) return;
                SetMember(p, "overrideState", overrideState);
                SetMember(p, "value", value);
            }
            catch { }
        }

        private static void SetMember(object o, string n, object v)
        {
            var t = o.GetType();
            var pr = t.GetProperty(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (pr != null && pr.CanWrite) { pr.SetValue(o, v, null); return; }
            var fi = t.GetField(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fi != null) fi.SetValue(o, v);
        }
    }
}
