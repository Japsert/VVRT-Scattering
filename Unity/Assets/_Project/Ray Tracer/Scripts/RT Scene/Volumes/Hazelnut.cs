using _Project.Ray_Tracer.Scripts.Utility;
using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RT_Scene.Volumes
{
    /// <remarks>
    /// Oli is part of these.
    /// Do not confuse with oil.
    /// </remarks>
    public class Hazelnut : RTHeterogeneousVolume, ILoadable
    {
        private const string Path = "/hnut256_uint.raw";

        private static float _maxDensity;
        private static float[,,] _grid;
        private static readonly IntVector3 Size = new(256, 256, 256);
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
            new(0.353f, new Color(0, 1, 0, 0.5f)),
            new(0.435f, new Color(1, 1, 0, 0.7f)),
            new(1.0f, Color.clear)
        };

        public override bool IsLoaded => _isLoaded;
        protected override float MaxDensity => _maxDensity;
    }
}