using System;

namespace QoL;
[Serializable]
public class Settings {
    /// <summary>
    /// Whether to skip the intro logo scene when launching the game.
    /// </summary>
    public bool SkipLogo { get; set; } = true;

    /// <summary>
    /// Whether to have optionally skippable cutscenes.
    /// </summary>
    public bool SkippableCutscenes { get; set; } = true;
}
