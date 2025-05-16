using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RT_Ray.Events.Volume_Absorption
{
    [RequireComponent(typeof(AbsorptionRenderer))]
    public class AbsorptionObject : MonoBehaviour
    {
        private VolumeAbsorption _absorption;
        private AbsorptionRenderer _absorptionRenderer;

        public void Draw()
        {
            _absorptionRenderer.Origin = _absorption.WorldPos;
        }

        private void Awake()
        {
            _absorptionRenderer = GetComponent<AbsorptionRenderer>();
        }
    }
}