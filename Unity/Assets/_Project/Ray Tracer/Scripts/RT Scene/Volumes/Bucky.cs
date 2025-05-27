using _Project.Ray_Tracer.Scripts.Utility;
using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RT_Scene.Volumes
{
    public class Bucky : RTHeterogeneousVolume, ILoadable
    {
        private const string Path = "/bucky32x32x32.raw";

        private static float _maxDensity;
        private static float[,,] _grid;
        private static readonly IntVector3 Size = new(32, 32, 32);
        private static bool _isLoaded = false;

        protected override float[,,] Grid => _grid;
        protected override IntVector3 GridSize => Size;

        protected override float MaxDensity => _maxDensity;

        public override bool IsLoaded => _isLoaded;

        public static void Load() {
            if (_isLoaded) return;
            _grid = LoadFromFile(Path, Size, out float maxDensity);
            _maxDensity = maxDensity;
            _isLoaded = true;
        }
        
        public override ColorTableEntry[] ColorLookupTable { get; } =
        {
            new(0f, Color.clear),
            new(0f, Color.clear),
            new(0f, Color.clear),
            new(0f, Color.clear),
            new(1.0f, new Color(1, 1, 1, 0.3f)),
        };
    }
}