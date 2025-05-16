using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RT_Ray.Events.Volume_Absorption
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class AbsorptionRenderer: MonoBehaviour
    {
        public float Radius { get; } = 0.1f;
        public Vector3 Origin { get; set; }
    }
}