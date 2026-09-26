using System.Diagnostics;
using ElderBlood.Configs;
using Debug = UnityEngine.Debug;

namespace ElderBlood.Services
{
    // Channel-gated logging wrapper.
    //
    // Stripping: every channel call carries three [Conditional] attributes, which are OR'd — the call
    // survives in the editor and in development builds, and dies in a release build unless ENABLE_LOGS
    // is added to the Scripting Define Symbols. A single ENABLE_LOGS symbol was tried first and stripped
    // everything everywhere, because that symbol is defined in no build profile by default.
    //
    // Gating: each channel checks its flag on LogConfig. Until Init() runs (GameLifetimeScope, at
    // container build) there is no config, and calls fall through to the console — silence before
    // init would hide whatever the editor menu tools and the pre-run code have to say.
    //
    // The flags gate the console write, not the argument: an interpolated message is still built
    // before the flag is read, so they buy quiet, not speed. Keep logging out of OnUpdate.
    //
    // Adding a channel: enum-free by design, so copy an existing method whole — a new one missing
    // the [Conditional] trio would ship its logs into the release build.
    public static class GameLogger
    {
        private static LogConfig _config;

        // LogConfig is injected once by WorldCreator via MainConfig.
        public static void Init(LogConfig config)
        {
            _config = config;
        }

        // --- Game flow ---

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("ENABLE_LOGS")]
        public static void Run(string message)
        {
            if (_config == null || _config.logRun) Debug.Log(message);
        }

        // --- ECS domains ---

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("ENABLE_LOGS")]
        public static void Damage(string message)
        {
            if (_config == null || _config.logDamage) Debug.Log(message);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("ENABLE_LOGS")]
        public static void Guard(string message)
        {
            if (_config == null || _config.logGuard) Debug.Log(message);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("ENABLE_LOGS")]
        public static void Blood(string message)
        {
            if (_config == null || _config.logBlood) Debug.Log(message);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("ENABLE_LOGS")]
        public static void Ability(string message)
        {
            if (_config == null || _config.logAbility) Debug.Log(message);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("ENABLE_LOGS")]
        public static void Upgrade(string message)
        {
            if (_config == null || _config.logUpgrade) Debug.Log(message);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("ENABLE_LOGS")]
        public static void Village(string message)
        {
            if (_config == null || _config.logVillage) Debug.Log(message);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("ENABLE_LOGS")]
        public static void Hud(string message)
        {
            if (_config == null || _config.logHud) Debug.Log(message);
        }

        // --- Infrastructure ---

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("ENABLE_LOGS")]
        public static void Save(string message)
        {
            if (_config == null || _config.logSave) Debug.Log(message);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("ENABLE_LOGS")]
        public static void Art(string message)
        {
            if (_config == null || _config.logArt) Debug.Log(message);
        }

        // Deliberately not [Conditional] and not channel-gated: an error that only shows up in the
        // editor is an error nobody sees in the build that shipped it.
        public static void Error(string message)
        {
            Debug.LogError(message);
        }
    }
}
