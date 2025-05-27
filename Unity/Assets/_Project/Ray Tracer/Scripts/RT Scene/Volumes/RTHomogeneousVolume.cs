using UnityEngine;
using Random = UnityEngine.Random;

namespace _Project.Ray_Tracer.Scripts.RT_Scene.Volumes
{
    public class RTHomogeneousVolume : RTVolume
    {
        [Header("Homogeneous volume settings")]
        [SerializeField] private float uniformDensity = 0.5f;

        protected override float MaxDensity => uniformDensity;

        public override float DensityAt(Vector3 worldPoint)
        {
            return uniformDensity;
        }

        public override float GetStepLength(Vector3 pos, Vector3 direction)
        {
            return -Mathf.Log(1f - Random.value) / ExtinctionAt(pos);
        }
    }
}