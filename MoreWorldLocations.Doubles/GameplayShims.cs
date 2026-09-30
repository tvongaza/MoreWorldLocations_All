// MWL's copies of the gameplay types that Valheim.Testing.Doubles declares in Doubles/GameplayDoubles.cs. That
// package file is left out of this build (MoreWorldLocations.Doubles.csproj) because of its HeightmapBuilder:
//
//  - HeightmapBuilder there builds on demand, so RequestTerrainSync never refuses and a zone can never be left
//    unbuilt. MWL's terrain bridge must refuse a zone the builder has not built, and its tests say which zones are
//    built, consume an entry and watch it come back (Built, Build, Rebuilding, SyncRequests).
//  - Utils there compresses with gzip and its GetEnabledComponentsInChildren leaves out the root's own components;
//    MWL's leaves the bytes alone and includes the root's components when the root is active. Which of the two
//    root rules is the game's has not been checked here.
//  - Container, Pickable and SpawnArea there are MonoBehaviours whose Awake reads a ZNetView on the same object, so
//    adding one to a fixture object without a view throws. MWL's fixtures describe templates, not live objects.
//  - DropTable.DropData there is a struct; RandomSpawn, RandomObject, CreatureSpawner, DropOnDestroyed and
//    PickableItem are not doubled at all.
//
// These are the shapes MWL's template walk reads, and nothing else: no rolls, no spawning, no lifecycle.
#nullable enable
// ReSharper disable InconsistentNaming

/// <summary>
/// Shim for Valheim's Utils, for the calls the terrain blob and the template walk make. The game's own compression is
/// not ours to test, so here it is the identity: what the tests exercise is the structure the encoder and decoder
/// agree on.
/// </summary>
public static class Utils
{
    public static byte[] Compress(byte[] data) => data;
    public static byte[] Decompress(byte[] data) => data;

    // The package's other doubles (localization, registries) call these; they live on the game's Utils, which MWL keeps.
    /// <summary>Ordinal, character by character (no culture rules), as the game's.</summary>
    public static bool CustomStartsWith(this string a, string b) => b.Length <= a.Length && string.CompareOrdinal(a, 0, b, 0, b.Length) == 0;
    public static bool CustomEndsWith(this string a, string b) => b.Length <= a.Length && string.CompareOrdinal(a, a.Length - b.Length, b, 0, b.Length) == 0;

    /// <summary>
    /// Vanilla's own reader for a location's children: enabled components, under enabled parents, which is the set
    /// the game would instantiate.
    /// </summary>
    public static T[] GetEnabledComponentsInChildren<T>(UnityEngine.GameObject root) where T : UnityEngine.Component
    {
        var found = new System.Collections.Generic.List<T>();
        Collect(root, true, found);
        return found.ToArray();
    }

    private static void Collect<T>(UnityEngine.GameObject go, bool enabled, System.Collections.Generic.List<T> into)
        where T : UnityEngine.Component
    {
        bool here = enabled && go.activeSelf;
        if (here)
        {
            T component = go.GetComponent<T>();
            if (component != null)
                into.Add(component);
        }
        for (int i = 0; i < go.transform.childCount; i++)
            Collect(go.transform.GetChild(i).gameObject, here, into);
    }
}

/// <summary>
/// Shim for HeightmapBuilder: the generated heights a client builds for itself. Tests register an array per zone; a
/// zone with none is one the builder has not built, which is the case the bridge must refuse rather than convert
/// against zero.
/// </summary>
public class HeightmapBuilder
{
    public class HMBuildData
    {
        public System.Collections.Generic.List<float> m_baseHeights = new();
    }

    // The package's ValheimWorldScope saves and restores the builder through this field.
    internal static HeightmapBuilder? m_instance;

    /// <summary>The world's builder; null unless a test installs one.</summary>
    public static HeightmapBuilder? instance
    {
        get => m_instance;
        set => m_instance = value;
    }

    public readonly System.Collections.Generic.Dictionary<Vector2s, HMBuildData> Built = new();
    public int SyncRequests;

    /// <summary>
    /// Like the game: true only while the entry is in the ready list. A consumed entry goes back to the build queue,
    /// so the NEXT question is false and the one after that is true again -- which is what makes a double read inside
    /// one pass fail here as it failed in game, without making the harness brittle for work that comes back later.
    /// </summary>
    public bool IsTerrainReady(UnityEngine.Vector3 centre, int width, float scale, bool distantLod, WorldGenerator gen)
    {
        var zone = ZoneSystem.GetZone(centre);
        if (Built.ContainsKey(zone))
            return true;
        if (Rebuilding.Remove(zone) && Recipes.TryGetValue(zone, out var gen2))
            Build(zone, gen2);          // the builder finished between questions
        return false;
    }

    /// <summary>Zones whose data was consumed and is being built again.</summary>
    public readonly System.Collections.Generic.HashSet<Vector2s> Rebuilding = new();
    /// <summary>The generator each zone was built from, so it can be built again.</summary>
    public readonly System.Collections.Generic.Dictionary<Vector2s, WorldGenerator> Recipes = new();

    /// <summary>
    /// Like the game: handing an entry out REMOVES it from the ready list (RequestTerrain does m_ready.RemoveAt), so
    /// asking twice does not answer twice. Modelling that is what makes a "can I?" call followed by a "do it" call fail
    /// here as it failed in game.
    /// </summary>
    public HMBuildData? RequestTerrainSync(UnityEngine.Vector3 centre, int width, float scale, bool distantLod, WorldGenerator gen)
    {
        SyncRequests++;
        var zone = ZoneSystem.GetZone(centre);
        if (!Built.TryGetValue(zone, out var d))
            return null;
        Built.Remove(zone);
        Rebuilding.Add(zone);
        return d;
    }

    /// <summary>Build a zone's heights from a generator, the way the game's builder does.</summary>
    public HMBuildData Build(Vector2s zone, WorldGenerator gen, int width = 64, float scale = 1f)
    {
        var centre = ZoneSystem.GetZonePos(zone);
        var data = new HMBuildData();
        int pitch = width + 1;
        for (int y = 0; y < pitch; y++)
            for (int x = 0; x < pitch; x++)
                data.m_baseHeights.Add(gen.GetHeight(
                    centre.x + (x - width / 2) * scale, centre.z + (y - width / 2) * scale));
        Built[zone] = data;
        Recipes[zone] = gen;
        Rebuilding.Remove(zone);
        return data;
    }
}

/// <summary>The loot a container rolls on the server and saves into its ZDO.</summary>
public class DropTable
{
    public System.Collections.Generic.List<DropData> m_drops = new();

    public class DropData
    {
        public UnityEngine.GameObject? m_item;
    }
}

/// <summary>Vanilla's chance-gated branch. Only the shape the trace reads.</summary>
public class RandomSpawn : UnityEngine.Component
{
    public UnityEngine.GameObject? m_OffObject;
    public float m_chanceToSpawn = 50f;
}

/// <summary>Vanilla's weighted pick among alternatives. Only the shape the trace reads.</summary>
public class RandomObject : UnityEngine.Component
{
    public class ObjectEntry
    {
        public UnityEngine.GameObject? m_object;
        public float m_weight = 1f;
    }
    public System.Collections.Generic.List<ObjectEntry> m_objects = new();
}

public class Container : UnityEngine.Component
{
    public DropTable m_defaultItems = new();
}

public class CreatureSpawner : UnityEngine.Component
{
    public UnityEngine.GameObject? m_creaturePrefab;
}

public class DropOnDestroyed : UnityEngine.Component
{
    public DropTable m_dropWhenDestroyed = new();
}

public class SpawnArea : UnityEngine.Component
{
    public System.Collections.Generic.List<SpawnData> m_prefabs = new();

    public class SpawnData
    {
        public UnityEngine.GameObject? m_prefab;
    }
}

public class PickableItem : UnityEngine.Component
{
    public RandomItem[] m_randomItemPrefabs = new RandomItem[0];

    /// <summary>A struct in the game, so there is no null entry to skip.</summary>
    public struct RandomItem
    {
        public UnityEngine.GameObject? m_itemPrefab;
    }
}

public class Pickable : UnityEngine.Component
{
    public UnityEngine.GameObject? m_itemPrefab;
}
