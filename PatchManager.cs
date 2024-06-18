using System;
using HarmonyLib;
using System.Reflection;

namespace QoL;
/// <summary>
/// Handles patching and unpatching of methods in the main Assembly.
/// </summary>
internal static class PatchManager {
    /// <summary>
    /// An instance of Harmony.
    /// </summary>
    private static Harmony _harmony;

    /// <summary>
    /// Initialize the patch manager.
    /// </summary>
    public static void Initialize() {
        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        var autoloadId = Plugin.Instance.Settings.AutoLoadSaveId;
        if (autoloadId is >= 0 and < 4) {
            _harmony.PatchAll(typeof(AutoLoadSavePatches));   
        }
    }

    /// <summary>
    /// Unpatch all methods patched by QoL's patch classes in the Harmony instance.
    /// </summary>
    public static void Unpatch() {
        var patches = new[] {
            typeof(AutoLoadSavePatches),
        };
        foreach (var patch in patches) {
            var methodInfos = patch.GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
            foreach (var info in methodInfos) {
                _harmony.Unpatch(info, HarmonyPatchType.All, MyPluginInfo.PLUGIN_GUID);
            }
        }
    }
}