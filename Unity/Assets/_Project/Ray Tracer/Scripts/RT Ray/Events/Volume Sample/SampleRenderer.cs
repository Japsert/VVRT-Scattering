using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RT_Ray.Events.Volume_Sample
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class SampleRenderer: MonoBehaviour
    {
        public float Radius => 0.1f;
        public Vector3 Origin { get; set; }

        public MeshRenderer Renderer { get; private set; }

        private void Awake()
        {
            Renderer = GetComponent<MeshRenderer>();
        }
    }
}