using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RT_Ray.Events.Volume_Sample
{
    public class VolumeSample
    {
        public Vector3 WorldPos { get; }

        public VolumeSample(Vector3 worldPos)
        {
            WorldPos = worldPos;
        }
    }
}