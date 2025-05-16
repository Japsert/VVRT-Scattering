using _Project.Ray_Tracer.Scripts.RT_Scene.Volumes;
using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RT_Ray.Events.Volume_Sample
{
    public class VolumeSample
    {
        private readonly RTVolume _volume;
        public Vector3 WorldPos { get; }

        public VolumeSample(RTVolume volume, Vector3 worldPos)
        {
            _volume = volume;
            WorldPos = worldPos;
        }
    }
}