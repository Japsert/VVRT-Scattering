using System.IO;
using _Project.Ray_Tracer.Scripts.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Ray_Tracer.Scripts.RT_Scene.Volumes
{
    public abstract class RTVolume : RTMesh
    {
        public Collider Collider { get; private set; }

        protected abstract float MaxDensity { get; } // maximum density value across the volume
        public float MaxExtinction => MaxDensity * Extinction;
        
        protected static float[,,] LoadFromFile(string path, IntVector3 size, out float maxDensity)
        {
            float[,,] grid = new float[size.x, size.y, size.z];
            maxDensity = 0;

            BinaryReader reader = new(File.Open((Application.streamingAssetsPath + path), FileMode.Open));
            for (int z = 0; z < size.z; z++)
            for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
            {
                float density = (float) reader.ReadByte() / 255;
                grid[x, y, z] = density;
                if (density > maxDensity)
                    maxDensity = density;
            }

            reader.Close();
            return grid;
        }
        
        [Header("Volume settings")]
        [SerializeField] private float absorption = 0.5f; // probability per unit length that a particle is absorbed
        [SerializeField] private float scattering = 0.5f; // probability per unit length that a particle is scattered
        [SerializeField] private float emission = 0.5f;
        [SerializeField] private float g = 0.95f;

        private float Extinction => absorption + scattering; // probability per unit length of any interaction
        private float Albedo => scattering / Extinction; // probability that an extinction event is scattering
        public float G => g;

        public abstract float DensityAt(Vector3 worldPos);

        public bool IsInBounds(Vector3 worldPos)
        {
            return Collider.bounds.Contains(worldPos);
        }

        public float AbsorptionAt(Vector3 worldPos) => DensityAt(worldPos) * absorption;

        public float ScatteringAt(Vector3 worldPos) => DensityAt(worldPos) * scattering;

        public float ExtinctionAt(Vector3 worldPos) => DensityAt(worldPos) * Extinction;

        public float AlbedoAt(Vector3 worldPos) => DensityAt(worldPos) * Albedo;

        public abstract float GetStepLength(Vector3 pos, Vector3 direction);

        public bool Intersect(Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float maxDistance)
            => Collider.Raycast(new Ray(origin, direction), out hitInfo, maxDistance);

        protected new void Awake()
        {
            Collider = GetComponent<Collider>();
            type = ObjectType.Volume;
            base.Awake();
        }
    }
}