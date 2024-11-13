using BepInEx.Configuration;

public class Settings {
    private static Settings m_instance = null;
    public static Settings Instance {
        get {
            if (m_instance == null) {
                m_instance = new Settings();
            }
            return m_instance;
        }
    }
    private DDPlugin m_plugin = null;

    // General
    public static ConfigEntry<bool> m_enabled;
    public static ConfigEntry<string> m_log_level;
    public static ConfigEntry<float> m_normal_speed_multiplier;
    public static ConfigEntry<float> m_sprint_speed_multiplier;
    public static ConfigEntry<float> m_jump_speed_multiplier;
    public static ConfigEntry<float> m_swim_speed_multiplier;

    public void load(DDPlugin plugin) {
        this.m_plugin = plugin;

        // General
        m_enabled = this.m_plugin.Config.Bind<bool>("General", "Enabled", true, "Set to false to disable this mod.");
        m_log_level = this.m_plugin.Config.Bind<string>("General", "Log Level", "info", "[Advanced] Logging level, one of: 'none' (no logging), 'error' (only errors), 'warn' (errors and warnings), 'info' (normal logging), 'debug' (extra log messages for debugging issues).  Not case sensitive [string, default info].  Debug level not recommended unless you're noticing issues with the mod.  Changes to this setting require an application restart.");
        m_normal_speed_multiplier = this.m_plugin.Config.Bind<float>("General", "Normal Speed Multiplier", 1, "Multiplier applied to normal (ground) movement (float, default 1 [no change]).");
        m_sprint_speed_multiplier = this.m_plugin.Config.Bind<float>("General", "Sprint Speed Multiplier", 1.5f, "Multiplier applied (in addition to Normal Speed Multiplier) to normal (ground) movement.  This overrides the 1.5x value used for sprinting, so set it higher than 1.5 to run faster when sprinting (float, default 1.5 [no change]).");
        m_jump_speed_multiplier = this.m_plugin.Config.Bind<float>("General", "Jump Speed Multiplier", 1, "Multiplier applied to upward jump velocity (float, default 1 [no change]).");
        m_swim_speed_multiplier = this.m_plugin.Config.Bind<float>("General", "Swim Speed Multiplier", 1, "Multiplier applied to swimming speed (float, default 1 [no change]).");
    }
}