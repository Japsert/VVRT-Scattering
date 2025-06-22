using _Project.Ray_Tracer.Scripts.Utility;

namespace _Project.Ray_Tracer.Scripts.RT_Scene.Volumes
{
    public class Engine : RTHeterogeneousVolume
    {
        public override VolumeManager.VolumeType VolumeType => VolumeManager.VolumeType.Engine;
        
        private const string Path = "/engine256x256x256.raw";
        
        private static float[,,] _grid;
        private static float _maxDensity;
        
        protected override float[,,] Grid => _grid;
        protected override IntVector3 Size => new(256, 256, 256);
        protected override float MaxDensity => _maxDensity;

        public void Start()
        {
            if (IsLoaded) return;
            _grid = LoadFromFile(Path, Size, out _maxDensity);
            IsLoaded = true;
        }
    }
}