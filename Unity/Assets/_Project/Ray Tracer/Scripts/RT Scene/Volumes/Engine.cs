using _Project.Ray_Tracer.Scripts.Utility;
using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RT_Scene.Volumes
{
    public class Engine : RTHeterogeneousVolume
    {
        public override VolumeManager.VolumeType VolumeType => VolumeManager.VolumeType.Bucky;
        
        private const string Path = "/engine256x256x256.raw";

        private static float _maxDensity;
        private static float[,,] _grid;
        private static readonly IntVector3 Size = new(256, 256, 256);
        private static bool _isLoaded;

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
            new(0.556f, new Color(1, 0, 0, 0.3f)),
            new(1.0f, new Color(0, 0, 1, 0.7f))
        };

        public override bool IsLoaded => _isLoaded;
        protected override float MaxDensity => _maxDensity;
    }
}