using System.IO;
using _Project.Ray_Tracer.Scripts.Utility;
using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RT_Scene.Volumes
{
    public abstract class RTVolume : RTMesh // TODO: RTVolume shares attributes with RTMesh, but shouldn't subclass it
    {
        [SerializeField] private MeshChanged
            onAbsorptionChanged,
            onScatteringChanged,
            onEmissionColorChanged,
            onEmissionChanged,
            onGChanged;

        protected static float[,,] LoadFromFile(string path, IntVector3 size, out float maxDensity)
        {
            float[,,] grid = new float[size.x, size.y, size.z];
            maxDensity = 0;

            BinaryReader reader = new(File.Open((Application.streamingAssetsPath + path), FileMode.Open));
            for (int z = 0; z < size.z; z++)
            for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
            {
                float density = (float)reader.ReadByte() / 255;
                grid[x, y, z] = density;
                if (density > maxDensity)
                    maxDensity = density;
            }

            reader.Close();
            return grid;
        }


        [Header("Volume settings")] [SerializeField, Range(0f, 1f)]
        private float absorption = 0.5f;

        public float Absorption
        {
            get => absorption;
            set
            {
                if (absorption == value) return;
                absorption = value;
                OnMeshChanged.Invoke();
                onAbsorptionChanged.Invoke();
            }
        }

        [SerializeField, Range(0f, 1f)] private float scattering = 0.5f;

        public float Scattering
        {
            get => scattering;
            set
            {
                if (scattering == value) return;
                scattering = value;
                OnMeshChanged.Invoke();
                onScatteringChanged.Invoke();
            }
        }

        [SerializeField] private Color emissionColor = new(0.833333f, 0f, 0f);

        public Color EmissionColor
        {
            get => emissionColor;
            set
            {
                if (emissionColor == value) return;
                emissionColor = value;
                OnMeshChanged.Invoke();
                onEmissionColorChanged.Invoke();
            }
        }

        [SerializeField, Range(0f, 1f)] private float emission = 0.5f;

        public float Emission
        {
            get => emission;
            set
            {
                if (emission == value) return;
                emission = value;
                OnMeshChanged.Invoke();
                onEmissionChanged.Invoke();
            }
        }

        [SerializeField, Range(-1f, 1f)] private float g = 0.9f;

        public float G
        {
            get => g;
            set
            {
                if (g == value) return;
                g = value;
                OnMeshChanged.Invoke();
                onGChanged.Invoke();
            }
        }

        public float Extinction => Absorption + Scattering; // probability per unit length of any interaction

        public float Albedo => Scattering / Extinction; // probability that an extinction event is scattering

        public abstract float DensityAt(Vector3 worldPos);

        public float AbsorptionAt(Vector3 worldPos) => DensityAt(worldPos) * Absorption;

        public float ScatteringAt(Vector3 worldPos) => DensityAt(worldPos) * Scattering;

        public Color EmissionOf(float density) => density * EmissionColor * Emission;

        public float ExtinctionAt(Vector3 worldPos) => DensityAt(worldPos) * Extinction;

        public float ExtinctionOf(float density) => density * Extinction;

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