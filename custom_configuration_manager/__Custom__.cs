using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ConfigurationManager {

    public static class Constants {
        public const string Version = "18.3";
    }

    [BepInPlugin(GUID + "_custom", "Custom Configuration Manager", Version)]
    public class CustomConfigManager : ConfigurationManager {
        private Harmony m_harmony = new Harmony(GUID);
        private static CustomConfigManager m_instance;
        public static CustomConfigManager Instance {
            get {
                return m_instance;
            }
        }

        private static void _error_log(object text) {
            Logger.LogError(text);
        }

        private void Awake() {
            try {
                m_instance = this;
                this.m_harmony.PatchAll();
                Logger.LogInfo($"{GUID} v{Version} loaded.");
            } catch (Exception e) {
                Logger.LogError("** Awake FATAL - " + e);
            }
        }

        public class CcmInterface : MonoBehaviour {
            private MethodInfo m_method_info_update;
            private MethodInfo m_method_info_on_gui;
            private ConfigEntry<KeyboardShortcut> m_keybind;

            private void Awake() {
                this.m_method_info_update = ReflectionUtils.get_method(CustomConfigManager.Instance, "Update", typeof(ConfigurationManager));
                this.m_method_info_on_gui = ReflectionUtils.get_method(CustomConfigManager.Instance, "OnGUI", typeof(ConfigurationManager));
                this.m_keybind = (ConfigEntry<KeyboardShortcut>) ReflectionUtils.get_field_value(CustomConfigManager.Instance, "_keybind", typeof(ConfigurationManager));
                //foreach (MethodInfo method in typeof(ConfigurationManager).GetMethods(ReflectionUtils.BINDING_FLAGS_ALL)) {
                //    _error_log(method.Name);
                //}
                _error_log($"keybind: {this.m_keybind.Value.MainKey}");
            }
            private bool m_prev_display = false;
            private void Update() {
                this.m_method_info_update.Invoke(CustomConfigManager.Instance, new object[] {});
                if (CustomConfigManager.Instance.DisplayingWindow != this.m_prev_display) {
                    this.m_prev_display = CustomConfigManager.Instance.DisplayingWindow;
                    _error_log($"DisplayingWindow = {CustomConfigManager.Instance.DisplayingWindow}");
                }
                foreach (PluginInfo info in Chainloader.PluginInfos.Values) {
                    _error_log($"location: {info.Location}, instance: {info.Instance}");
                }
            }

            private void OnGUI() {
                this.m_method_info_on_gui.Invoke(CustomConfigManager.Instance, new object[] {});
            }
        }

        [HarmonyPatch(typeof(AudioManager), "Awake")]
        class HarmonyPatch_AudioManager_Awake {
            private static void Postfix(AudioManager __instance) {
                __instance.gameObject.AddComponent<CcmInterface>();
            }
        }
    }

    /*
    public class CustomConfigManager {
        private static CustomConfigManager m_instance = null;
        public static CustomConfigManager Instance {
            get {
                return m_instance;
            }
        }
        ConfigurationManager m_plugin = null;
        public static ManualLogSource m_logger;

        public static void initialize(ConfigurationManager plugin) {
            if (m_instance == null) {
                m_instance = new CustomConfigManager(plugin);
            }
        }

        private CustomConfigManager(ConfigurationManager plugin) {
            this.m_plugin = plugin;
            m_logger = this.m_plugin.Logger;
        }
    }
    */
}