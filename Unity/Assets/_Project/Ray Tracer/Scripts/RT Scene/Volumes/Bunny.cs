using _Project.Ray_Tracer.Scripts.Utility;
using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RT_Scene.Volumes
{
    public class Bunny : RTHeterogeneousVolume
    {
        public override VolumeManager.VolumeType VolumeType => VolumeManager.VolumeType.Bunny;
        
        private const string Path = "/bunny512x512x361.raw";

        private static float[,,] _grid;
        private static float _maxDensity;
        
        protected override float[,,] Grid => _grid;
        protected override IntVector3 Size => new(512, 512, 361);
        protected override float MaxDensity => _maxDensity;
        
        private void Start()
        {
            if (IsLoaded) return;
            _grid = LoadFromFile(Path, Size, out _maxDensity);
            IsLoaded = true;
        }
        
        private new void Awake()
        {
            // The bunny is upside down by default, so we set it upright.
            Rotation = new Vector3(90, 180, 180);
            base.Awake();
        }
    }
}