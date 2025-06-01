using _Project.Ray_Tracer.Scripts.Utility;
using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RT_Scene.Volumes
{
    public class Bunny : RTHeterogeneousVolume
    {
        public override VolumeManager.VolumeType VolumeType => VolumeManager.VolumeType.Bucky;
        
        private const string Path = "/bunny512x512x361.raw";

        private static float _maxDensity;
        private static float[,,] _grid;
        private static readonly IntVector3 Size = new(512, 512, 361);
        private static bool _isLoaded = false;

        public static void Load()
        {
            if (_isLoaded) return;
            _grid = LoadFromFile(Path, Size, out _maxDensity);
            _isLoaded = true;
        }

        protected override float[,,] Grid => _grid;
        protected override IntVector3 GridSize => Size;

        public override ColorTableEntry[] ColorLookupTable { get; } =
        {
            new(0f, Color.clear),
            new(0f, Color.clear),
            new(0f, Color.clear),
            new(0.568f, new Color(0, 1, 0, 0.3f)),
            new(1.0f, Color.clear),
        };

        public override bool IsLoaded => _isLoaded;

        protected override float MaxDensity => _maxDensity;

        private new void Awake()
        {
            // The bunny is upside down by default, so we set it upright.
            Rotation = new Vector3(90, 180, 180);
            base.Awake();
        }
    }
}