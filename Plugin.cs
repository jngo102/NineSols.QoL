using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QoL;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("NineSols.exe")]
public class Plugin : BaseUnityPlugin {
    public string settingsPath = Path.Combine(Application.persistentDataPath, Assembly.GetExecutingAssembly().GetName().Name, "settings.json");

    internal static Plugin Instance { get; private set; }
    
    internal Settings Settings { get; private set; } = new();

    private const string LogoSceneName = "Logo";
    private const string TitleScreenSceneName = "TitleScreenMenu";

    private Scene _previousScene;
    private Scene _loadedScene;

    /// <summary>
    /// Whether the main menu scene was loaded from launching the game, NOT when quitting to it from a gameplay scene.
    /// </summary>
    internal bool LoadedMainMenuFromStart =>
        string.IsNullOrEmpty(_previousScene.name) && _loadedScene.name is TitleScreenSceneName;

    private void Awake() {
        Instance = this;
        Settings = LoadSettings();
        SceneManager.sceneLoaded += (scene, _) => {
            _previousScene = _loadedScene;
            _loadedScene = scene;
        }; 
        CheckSettings();
        PatchManager.Initialize();
    }

    private void Update() {
        var playerInput = PlayerInputBinder.Instance;
        if (Settings.SkippableCutscenes && playerInput.IsCutSceneActionEnable && playerInput.cutSceneActions.ESC.WasPressed) {
            SkippableManager.Instance.TrySkip();
        }
    }

    private void OnDestroy() {
        Logger.LogInfo($"Saving settings to {settingsPath}...");
        SaveSettings();
    }

    /// <summary>
    /// Check settings and make modifications accordingly.
    /// </summary>
    private void CheckSettings() {
        if (Settings.SkipLogo) {
            SceneManager.sceneLoaded += (_, _) => SkipLogo();
        }
    }

    /// <summary>
    /// Skip the developer logo and warnings scene on game startup.
    /// </summary>
    private void SkipLogo() {
        if (_loadedScene.name == LogoSceneName) {
            SceneManager.LoadScene(TitleScreenSceneName);
        }
    }
    
    /// <summary>
    /// Load QoL configuration settings from disk.
    /// </summary>
    /// <returns>The loaded settings.</returns>
    public Settings LoadSettings() {
        Logger.LogInfo($"Loading settings at {settingsPath}...");
        if (!File.Exists(settingsPath)) {
            Logger.LogWarning("No settings detected. Creating an empty one.");
            return new Settings();
        }
        var fileContents = File.ReadAllText(settingsPath);
        try {
            var settings = JsonConvert.DeserializeObject<Settings>(fileContents);
            Logger.LogInfo("Previous settings successfully loaded.");
            return settings;
        } catch (JsonException error) {
            Logger.LogError("Failed to deserialize JSON file: " + error);
            return new Settings();
        }
    }

    /// <summary>
    /// Save QoL configuration settings to disk.
    /// </summary>
    public void SaveSettings() {
        var settingsJson = JsonConvert.SerializeObject(Settings);
        var settingsDir = Path.GetDirectoryName(settingsPath);
        if (settingsDir != null && !Directory.Exists(settingsDir)) {
            Directory.CreateDirectory(settingsDir);
        }
        File.WriteAllText(settingsPath, settingsJson);
    }
    
    /// <summary>
    /// Log a message; for developer use.
    /// </summary>
    /// <param name="message">The message to log.</param>
    internal void Log(object message) {
        Logger.LogInfo(message);
    }
}
