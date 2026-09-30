// MWL's additions to the Valheim doubles from Valheim.Testing.Doubles: game members the package does not declare
// yet and MWL's sources or tests use. Each mirrors the game's member of the same name; none changes a member the
// package already has. Global namespace, like the game types they extend.
#nullable enable
// ReSharper disable InconsistentNaming
using System.Collections.Generic;

public partial class Heightmap
{
    // The mask colours are the game's own values, copied from Heightmap in assembly_valheim: the conversion lerps
    // towards exactly these. The package has Dirt and Paved.
    public static UnityEngine.Color m_paintMaskCultivated = new(0f, 1f, 0f, 1f);
    public static UnityEngine.Color m_paintMaskNothing = new(0f, 0f, 0f, 1f);
    public static UnityEngine.Color m_paintMaskClearVegetation = new(0f, 0f, 0f, 0f);
    public static UnityEngine.Color m_paintMaskDeepSnow = new(1f, 1f, 1f, 1f);

    /// <summary>The generated heights this heightmap was built from; null until built.</summary>
    public HeightmapBuilder.HMBuildData? m_buildData;
}

/// <summary>
/// ZNetView settings the template walk reads, and the parameterless constructor <c>AddComponent</c> needs (the
/// package builds a view only around a ZDO). A view added this way has no ZDO until a test gives it one.
/// </summary>
public partial class ZNetView
{
    public bool m_persistent = true;
    public bool m_syncInitialScale;

    public ZNetView() { }
}

public partial class ZDO
{
    private UnityEngine.Quaternion m_rotation = UnityEngine.Quaternion.identity;
    public UnityEngine.Quaternion GetRotation() => m_rotation;
    public void SetRotation(UnityEngine.Quaternion rotation) => m_rotation = rotation;
}

public static partial class ZDOVars
{
    /// <summary>The location a LocationProxy stands for, by prefab hash.</summary>
    public static readonly int s_location = "location".GetStableHashCode();
}

/// <summary>What Fork/ValheimDoubles.cs's TerrainComp.Save writes and how a test makes it fail.</summary>
public partial class TerrainComp
{
    /// <summary>Makes Save write nothing while reporting nothing, as the game does for a compiler this peer does not own.</summary>
    public bool SaveFails;
    public int Operations;
    public UnityEngine.Vector3 LastOpPoint;
    public float LastOpRadius;
}

/// <summary>
/// ZoneSystem is a MonoBehaviour in the game, and the mod uses it as a coroutine host. The methods below are the ones
/// the mod's Harmony hooks name; their bodies are empty, because what these doubles cannot establish is when Harmony
/// runs a hook, which is the in-game check.
/// </summary>
public partial class ZoneSystem : UnityEngine.MonoBehaviour
{
    public void GenerateLocationsIfNeeded() { }

    public UnityEngine.GameObject? SpawnLocation(
        ZoneLocation location, int seed, UnityEngine.Vector3 pos, UnityEngine.Quaternion rot,
        SpawnMode mode, List<UnityEngine.GameObject> spawnedGhostObjects, bool cheated) => null;

    public bool SpawnZone(Vector2s zoneID, SpawnMode mode, out UnityEngine.GameObject? root)
    {
        root = null;
        return true;
    }

    public void PlaceLocations(Vector2s zoneID, Heightmap hmap, SpawnMode mode) { }

    public void Update() { }

    /// <summary>
    /// The game's own "the location list is now complete" moment, and where Jotunn adds MWL's. Named here only so
    /// the hook can point at it.
    /// </summary>
    public void SetupLocations() { }

    /// <summary>A new world. Named for the same reason as SetupLocations.</summary>
    public void Awake() { }

    /// <summary>The placements the game has decided on, one per zone, as vanilla keeps them.</summary>
    public Dictionary<Vector2s, LocationInstance> m_locationInstances = new();

    /// <summary>The world's own location list, which is what actually places buildings.</summary>
    public List<ZoneLocation> m_locations = new();

    /// <summary>The same list by prefab hash, which is how a proxy finds its template.</summary>
    public Dictionary<int, ZoneLocation> m_locationsByHash = new();

    public partial class ZoneLocation
    {
        public partial class PrefabEntry
        {
            /// <summary>The resolved template, or null until something loads it.</summary>
            public UnityEngine.GameObject? Asset;

            public void Load() { }
            public void Release() { }
        }
    }
}

/// <summary>
/// ZNet is a MonoBehaviour in the game, and the mod uses it as a coroutine host when there is no ZoneSystem. The
/// load-error flag and Save are what the mod's world-load and save hooks read and name.
/// </summary>
public sealed partial class ZNet : UnityEngine.MonoBehaviour
{
    public static bool m_loadError;
    public void Save() { }
}

/// <summary>
/// The few game constants the shims have to agree on. Valheim puts the water surface at y = 30; the package names it
/// WorldGenerator.WaterLevel, and MWL's synthetic worlds read it from here.
/// </summary>
public static class ShimWorld
{
    public const float SeaLevel = WorldGenerator.WaterLevel;
}
