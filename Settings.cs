using System;

namespace QoL;
[Serializable]
public class Settings {
    /// <summary>
    /// Whether to skip the intro logo scene when launching the game.
    /// </summary>
    public bool SkipLogo { get; set; } = true;

    /// <summary>
    /// The ID of the save slot to automatically load on game startup.
    /// </summary>
    public int AutoLoadSaveId { get; set; } = -1;

    /// <summary>
    /// Whether to have optionally skippable cutscenes.
    /// </summary>
    public bool SkippableCutscenes { get; set; } = true;
}
