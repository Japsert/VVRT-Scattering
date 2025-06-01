using _Project.Ray_Tracer.Scripts.Utility;

namespace _Project.Ray_Tracer.Scripts.RT_Scene.Volumes
{
    public class Cloud: RTHeterogeneousVolume
    {
        protected override float MaxDensity { get; }
        public override VolumeManager.VolumeType VolumeType { get; }
        public override ColorTableEntry[] ColorLookupTable { get; }
        protected override float[,,] Grid { get; }
        protected override IntVector3 GridSize { get; }
        public override bool IsLoaded { get; }
    }
}