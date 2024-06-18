using HarmonyLib;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using UObject = UnityEngine.Object;

namespace QoL;

internal class AutoLoadSavePatches {
    /// <summary>
    /// Automatically load a save slot from the provided settings field.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(StartMenuLogic), "Start")]
    private static async void AutoLoadSave(StartMenuLogic __instance) {
        if (!Plugin.Instance.LoadedMainMenuFromStart) return;
        var saveId = Plugin.Instance.Settings.AutoLoadSaveId;
        Plugin.Instance.Log("Save ID: " + saveId);
        // var canInteractField = __instance.GetType().GetField("CanInteract", BindingFlags.Instance | BindingFlags.NonPublic);
        // if (canInteractField is null) return;
        await UniTask.WaitUntil(() =>
        {
            var value = __instance.GetType().GetField("CanInteract", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.GetValue(__instance);
            Plugin.Instance.Log("Can interact: " + value);
            return value is true;
        });
        var saveExists = UObject.FindObjectsOfType<SaveSlotUIButton>(true).Any(btn => btn.index == saveId);
        Plugin.Instance.Log("Save exists: " + saveExists);
        await __instance.CreateOrLoadSaveSlotAndPlay(saveId, saveExists);
    }
}