using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RT_Ray.Events.Volume_Absorption
{
    public class VolumeAbsorption
    {
        public Vector3 WorldPos { get; }

        public VolumeAbsorption(Vector3 worldPos)
        {
            WorldPos = worldPos;
        }
    }
}