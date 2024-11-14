using BepInEx;
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
            private MethodInfo method_info_update;

            private void Awake() {
                method_info_update = typeof(ConfigurationManager).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.InvokeMethod);
                foreach (MethodInfo method in typeof(ConfigurationManager).GetMethods(ReflectionUtils.BINDING_FLAGS_ALL)) {
                    _error_log(method.Name);
                }
            }

            private void Update() {
                //method_info_update.Invoke(CustomConfigManager.Instance, new object[] {});
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