using System.IO;
using _Project.Ray_Tracer.Scripts.Utility;
using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RT_Scene.Volumes
{
    public abstract class RTVolume : RTMesh
    {
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

        public float Absorption
        {
            get => absorption;
            set
            {
                if (absorption == value) return;
                absorption = value;
                OnMeshChanged.Invoke();
            }
        }

        [SerializeField] private float scattering = 0.5f; // probability per unit length that a particle is scattered

        public float Scattering
        {
            get => scattering;
            set
            {
                if (scattering == value) return;
                scattering = value;
                OnMeshChanged.Invoke();
            }
        }

        [SerializeField] private float emission = 0.5f;

        public float Emission
        {
            get => emission;
            set
            {
                if (emission == value) return;
                emission = value;
                OnMeshChanged.Invoke();
            }
        }

        [SerializeField] private float g = 0.95f;

        public float G
        {
            get => g;
            set
            {
                if (g == value) return;
                g = value;
                OnMeshChanged.Invoke();
            }
        }

        public float Extinction => Absorption + Scattering; // probability per unit length of any interaction

        public float Albedo => Scattering / Extinction; // probability that an extinction event is scattering

        public abstract float DensityAt(Vector3 worldPos);

        public float AbsorptionAt(Vector3 worldPos) => DensityAt(worldPos) * Absorption;

        public float ScatteringAt(Vector3 worldPos) => DensityAt(worldPos) * Scattering;

        public float ExtinctionAt(Vector3 worldPos) => DensityAt(worldPos) * Extinction;

        public float AlbedoAt(Vector3 worldPos) => DensityAt(worldPos) * Albedo;

        public abstract float GetStepLength(Vector3 pos, Vector3 direction);

        public bool Intersect(Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float maxDistance)
            => Collider.Raycast(new Ray(origin, direction), out hitInfo, maxDistance);

        public bool IsInBounds(Vector3 worldPos)
        {
            return Collider.bounds.Contains(worldPos);
        }


        protected abstract float MaxDensity { get; } // maximum density value across the volume

        protected float MaxExtinction => MaxDensity * Extinction;


        private Collider Collider { get; set; }

        
        protected new void Awake()
        {
            Collider = GetComponent<Collider>();
            base.Awake();
        }
    }
}