// MWL's TerrainModifier. Valheim.Testing.Doubles declares one in Doubles/TerrainModifierDoubles.cs as a plain class
// with its own transform and a live list of modifiers (Awake/OnDestroy, the game's apply order). MWL's template walk
// finds modifiers as COMPONENTS of a template's objects -- AddComponent, GetComponent and
// Utils.GetEnabledComponentsInChildren<TerrainModifier> -- and reads a modifier's position from its object's
// transform, so that package file is left out of this build (MoreWorldLocations.Doubles.csproj) and this one is used.
#nullable enable
// ReSharper disable InconsistentNaming

/// <summary>
/// Shim for Valheim's TerrainModifier. The conversion takes a modifier's settings as a value rather than reading the
/// component, so only the settings are here. The PaintType order matters: it is what the serialised m_paintType
/// integer in a location bundle means.
/// </summary>
public partial class TerrainModifier : UnityEngine.Component
{
    public enum PaintType
    {
        Dirt,
        Cultivate,
        Paved,
        Reset,
        ClearVegetation,
        DeepSnow,
    }

    // The settings LocationTerrainReader copies, with the game's own defaults so a test that leaves one out gets what
    // an unconfigured modifier in Unity gets, not a zero the game never produces.
    public bool enabled = true;
    public int m_sortOrder;
    public bool m_useTerrainCompiler;
    public bool m_playerModifiction;
    public float m_levelOffset;
    public bool m_level;
    public float m_levelRadius = 2f;
    public bool m_square = true;
    public bool m_smooth;
    public float m_smoothRadius = 2f;
    public float m_smoothPower = 3f;
    public bool m_paintCleared = true;
    public bool m_paintHeightCheck;
    public PaintType m_paintType;
    public float m_paintRadius = 2f;
    public float m_paintStrength = 1f;
}

namespace Valheim.Testing.Doubles
{
    public sealed partial class ValheimWorldScope
    {
        // The package's scope restores its live modifier list here (TerrainModifierDoubles.cs). MWL's modifiers keep no
        // list, so there is nothing to restore.
        private void RestoreTerrainModifiers() { }
    }
}
