// MWL's copies of the Unity components that Valheim.Testing.Doubles declares in Doubles/UnityRenderingDoubles.cs.
// That package file is left out of this build (MoreWorldLocations.Doubles.csproj): its Light and Animator (like its
// Rigidbody and Canvas) carry their settings as public fields, where Unity keeps them in native properties. MWL's
// template walk reads a game component's public fields by reflection and names a component with none as "not
// compared" -- in the game that is every light and animator, and a test pins it. Here these components have exactly
// the public fields the walk's by-name readers use (colliders, renderers, mesh filters) and nothing else.
//
// Not modelled here, unlike the package: collider bounds, rigidbodies, material properties and instancing, mesh
// geometry, canvases.
#nullable enable
// ReSharper disable InconsistentNaming

namespace UnityEngine
{
    /// <summary>
    /// A renderer, with the parts the subtree comparison reads: whether it is on, and which materials it shows.
    /// </summary>
    public class Renderer : Component
    {
        public bool enabled = true;
        public Material[] sharedMaterials = new Material[0];
    }

    public class Material : Object { }

    public class Mesh : Object { }

    public class MeshFilter : Component
    {
        public Mesh? sharedMesh;
    }

    /// <summary>A collider, and the geometry a player actually meets.</summary>
    public class Collider : Component
    {
        public bool enabled = true;
        public bool isTrigger;
    }

    public class BoxCollider : Collider
    {
        public Vector3 size = new(1f, 1f, 1f);
        public Vector3 center;
    }

    public class SphereCollider : Collider
    {
        public float radius = 0.5f;
        public Vector3 center;
    }

    public class CapsuleCollider : Collider
    {
        public float radius = 0.5f;
        public float height = 2f;
        public int direction = 1;
        public Vector3 center;
    }

    public class MeshCollider : Collider
    {
        public bool convex;
        public Mesh? sharedMesh;
    }

    /// <summary>Engine and presentation state: present, and not compared. See TemplateFacts.UncomparedComponents.</summary>
    public class Light : Component { }

    public class Animator : Component { }

    public partial class Object
    {
        // What Instantiate makes of an object that is not a GameObject or a component: a copy of its serialized
        // fields. UnityComponentDoubles.cs calls it; the package declares it in the file left out.
        private protected virtual Object CopyForInstantiate()
        {
            var copy = UnitySerialization.NewInstance(GetType());
            UnitySerialization.CopySerializedFields(this, copy, new System.Collections.Generic.Dictionary<Object, Object>());
            copy.m_name = m_name;
            return copy;
        }
    }
}
