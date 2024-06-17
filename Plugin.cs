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

    private Settings Settings { get; set; } = new();

    private const string LogoSceneName = "Logo";
    private const string TitleScreenSceneName = "TitleScreenMenu";

    private Scene _loadedScene;

    private void Awake() {
        Settings = LoadSettings();
        SceneManager.sceneLoaded += (scene, _) => _loadedScene = scene; 
        CheckSettings();
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

        if (Settings.AutoLoadSaveId is >= 0 and < 4) {
            SceneManager.sceneLoaded += async (_, _) => await AutoLoadSave(Settings.AutoLoadSaveId);
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
    /// Automatically load a save file.
    /// </summary>
    /// <param name="saveId">The ID of the save to load.</param>
    private async UniTask AutoLoadSave(int saveId = 0) {
        if (_loadedScene.name != TitleScreenSceneName) return;
        Logger.LogInfo("Loading save at slot " + saveId);
        var saveSlotButton = FindObjectsOfType<SaveSlotUIButton>(true).FirstOrDefault(btn => btn.index == saveId);
        if (!saveSlotButton) return;
        await UniTask.Delay(10);
        saveSlotButton.Submit();
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
}
