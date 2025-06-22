using _Project.Ray_Tracer.Scripts.Utility;

namespace _Project.Ray_Tracer.Scripts.RT_Scene.Volumes
{
    public class Bucky : RTHeterogeneousVolume
    {
        public override VolumeManager.VolumeType VolumeType => VolumeManager.VolumeType.Bucky;

        private const string Path = "/Bucky32x32x32.raw";
        
        private static float[,,] _grid;
        private static float _maxDensity;
        
        protected override float[,,] Grid => _grid;
        protected override IntVector3 Size => new(32, 32, 32);
        protected override float MaxDensity => _maxDensity;
        
        private void Start()
        {
            if (IsLoaded) return;
            _grid = LoadFromFile(Path, Size, out _maxDensity);
            IsLoaded = true;
        }
    }
}