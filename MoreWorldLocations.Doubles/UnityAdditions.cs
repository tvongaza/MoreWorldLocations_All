// MWL's additions to the Unity doubles from Valheim.Testing.Doubles: members the package does not declare yet and
// MWL's template walk or tests use. Rotations stay yaw-only, as MWL's doubles always were: vanilla places a location
// with a yaw, and a test that needed a full rotation would be testing the double.
#nullable enable
// ReSharper disable InconsistentNaming

namespace UnityEngine
{
    public partial struct Color
    {
        public static bool operator ==(Color a, Color b) =>
            a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
        public static bool operator !=(Color a, Color b) => !(a == b);
        public override bool Equals(object? o) => o is Color c && this == c;
        public override int GetHashCode() => (r, g, b, a).GetHashCode();
    }

    public partial struct Quaternion
    {
        /// <summary>
        /// Only the part the template walk reads. Enough for a yaw, which is what vanilla places a location with; a
        /// full rotation is not modelled.
        /// </summary>
        public Vector3 eulerAngles =>
            new(0f, 2f * (float)System.Math.Atan2(y, w) * 180f / Mathf.PI, 0f);

        /// <summary>
        /// Composing two rotations, to the same yaw-only fidelity: enough for a transform walk that asks a child for
        /// its world rotation, and not a general quaternion product.
        /// </summary>
        public static Quaternion operator *(Quaternion a, Quaternion b)
        {
            float yaw = 2f * (float)System.Math.Atan2(a.y, a.w) + 2f * (float)System.Math.Atan2(b.y, b.w);
            return new Quaternion(0f, (float)System.Math.Sin(yaw / 2f), 0f, (float)System.Math.Cos(yaw / 2f));
        }

        /// <summary>Rotating a point about the y axis, which is all a placement does.</summary>
        public static Vector3 operator *(Quaternion rotation, Vector3 point)
        {
            float radians = 2f * (float)System.Math.Atan2(rotation.y, rotation.w);
            float cos = (float)System.Math.Cos(radians), sin = (float)System.Math.Sin(radians);
            return new Vector3(point.x * cos + point.z * sin, point.y, -point.x * sin + point.z * cos);
        }
    }

    public partial class GameObject
    {
        /// <summary>
        /// A new child, appended, standing at the world origin as a new object does before a fixture places it.
        /// Returns the child so fixtures read top down. Not a Unity member: a fixture helper.
        /// </summary>
        public GameObject Child(string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(transform, true);
            return child;
        }
    }
}

/// <summary>
/// The rotation members the template walk reads. The package's Transform stores one world rotation and does not
/// compose it with the parent's; these are derived from it, yaw-only.
/// </summary>
public partial class Transform
{
    /// <summary>
    /// A new transform is unrotated, as in Unity. The package leaves <see cref="rotation"/> at the zero quaternion,
    /// which the walk would record as a rotation of its own.
    /// </summary>
    public Transform() => rotation = UnityEngine.Quaternion.identity;

    /// <summary>The rotation relative to the parent: the parent's undone, then this one's.</summary>
    public UnityEngine.Quaternion localRotation
    {
        get => parent is { } up ? Conjugate(up.rotation) * rotation : rotation;
        set => rotation = parent is { } up ? up.rotation * value : value;
    }

    /// <summary>The world rotation as Euler angles (yaw only).</summary>
    public UnityEngine.Vector3 eulerAngles
    {
        get => rotation.eulerAngles;
        set => rotation = UnityEngine.Quaternion.Euler(0f, value.y, 0f);
    }

    private static UnityEngine.Quaternion Conjugate(UnityEngine.Quaternion q) => new(-q.x, -q.y, -q.z, q.w);
}
