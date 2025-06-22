using _Project.Ray_Tracer.Scripts.Utility;

namespace _Project.Ray_Tracer.Scripts.RT_Scene.Volumes
{
    public class Hazelnut : RTHeterogeneousVolume
    {
        public override VolumeManager.VolumeType VolumeType => VolumeManager.VolumeType.Hazelnut;
        
        private const string Path = "/hnut256_uint.raw";
        
        private static float[,,] _grid;
        private static float _maxDensity;
        
        protected override float[,,] Grid => _grid;
        protected override IntVector3 Size => new(256, 256, 256);
        protected override float MaxDensity => _maxDensity;

        private void Start()
        {
            if (IsLoaded) return;
            _grid = LoadFromFile(Path, Size, out _maxDensity);
            IsLoaded = true;
        }
    }
}