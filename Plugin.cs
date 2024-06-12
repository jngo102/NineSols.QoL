using System.IO;
using System.Reflection;
using BepInEx;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QoL;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("NineSols.exe")]
public class Plugin : BaseUnityPlugin {
    public string SettingsPath = Path.Combine(Application.persistentDataPath, Assembly.GetExecutingAssembly().GetName().Name, "settings.json");

    internal Settings Settings { get; private set; } = new();

    private const string LogoSceneName = "Logo";
    private const string TitleScreenSceneName = "TitleScreenMenu";

    internal static Plugin Instance { get; private set; }

    private void Awake() {
        Instance = this;

        Settings = LoadSettings();
        CheckSettings();
    }

    private void Update() {
        var playerInput = PlayerInputBinder.Instance;
        if (Settings.SkippableCutscenes && playerInput.IsCutSceneActionEnable && playerInput.cutSceneActions.ESC.WasPressed) {
            SkippableManager.Instance.TrySkip();
        }
    }

    private void OnDestroy() {
        Unsubscribe();
        Logger.LogInfo($"Saving settings to {SettingsPath}...");
        SaveSettings();
    }

    /// <summary>
    /// Check settings and make modifications accordingly.
    /// </summary>
    private void CheckSettings() {
        if (Settings.SkipLogo) {
            SceneManager.sceneLoaded += SkipLogo;
        }
    }

    /// <summary>
    /// Skip the developer logo and warnings scene on game startup.
    /// </summary>
    private void SkipLogo(Scene loadedScene, LoadSceneMode loadSceneMode) {
        if (loadedScene.name == LogoSceneName) {
            SceneManager.LoadScene(TitleScreenSceneName);
        }
    }

    /// <summary>
    /// Load QoL configuration settings from disk.
    /// </summary>
    /// <returns>The loaded settings.</returns>
    public Settings LoadSettings() {
        Logger.LogInfo($"Loading settings at {SettingsPath}...");
        if (!File.Exists(SettingsPath)) {
            Logger.LogWarning("No settings detected. Creating an empty one.");
            return new Settings();
        }
        var fileContents = File.ReadAllText(SettingsPath);
        var settings = JsonConvert.DeserializeObject<Settings>(fileContents);
        Logger.LogInfo("Previous settings successfully loaded.");
        return settings;
    }

    /// <summary>
    /// Save QoL configuration settings to disk.
    /// </summary>
    public void SaveSettings() {
        var settingsJson = JsonConvert.SerializeObject(Settings);
        var settingsDir = Path.GetDirectoryName(SettingsPath);
        if (!Directory.Exists(settingsDir)) {
            Directory.CreateDirectory(settingsDir);
        }
        File.WriteAllText(SettingsPath, settingsJson);
    }

    /// <summary>
    /// Unsubscribe all callbacks.
    /// </summary>
    public void Unsubscribe() {
        SceneManager.sceneLoaded -= SkipLogo;
    }
}
