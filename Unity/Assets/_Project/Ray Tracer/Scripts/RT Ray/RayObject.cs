using _Project.Ray_Tracer.Scripts.RT_Ray.Events.Volume_Absorption;
using _Project.Ray_Tracer.Scripts.RT_Ray.Events.Volume_Sample;
using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RT_Ray
{
    public class RayObject : MonoBehaviour
    {
        private RTRay _ray;
        /// <summary>
        /// The <see cref="RTRay"/> produced by the ray tracer that this ray object represents.
        /// </summary>
        public RTRay Ray
        {
            get => _ray;
            set
            {
                _ray = value;
                BodyObject.Reset();
                UpdateEvents();
            }
        }

        public RayBodyObject BodyObject { get; private set; }
        private SampleObject _sampleObject;
        private AbsorptionObject _absorptionObject;

        private void UpdateEvents()
        {
            if (_ray == null) return;

            _sampleObject.gameObject.SetActive(false);
            _absorptionObject.gameObject.SetActive(false);
            
            if (_ray.Sample != null)
            {
                _sampleObject.transform.position = _ray.Sample.WorldPos;
                _sampleObject.gameObject.SetActive(true);
            }

            if (_ray.Absorption != null)
            {
                _absorptionObject.transform.position = _ray.Absorption.WorldPos;
                _absorptionObject.gameObject.SetActive(true);
            }
        }

        public void Draw(float radius, bool drawSamples)
        {
            BodyObject.Draw(radius);
            if (drawSamples)
                _sampleObject.Show();
            else
                _sampleObject.Hide();
        }

        public void Draw(float radius, float distance, bool drawSamples)
        {
            BodyObject.Draw(radius, distance);
            if (drawSamples)
                _sampleObject.Show();
            else
                _sampleObject.Hide();
        }

        /// <remarks>
        /// We need these calls in Awake because <see cref="Ray"/>'s setter calls
        /// <see cref="RayBodyObject.Reset">BodyObject.Reset()</see> before Start initializes <see cref="BodyObject"/>.
        /// </remarks>
        private void Awake()
        {
            BodyObject = GetComponentInChildren<RayBodyObject>();
            _sampleObject = GetComponentInChildren<SampleObject>();
            _absorptionObject = GetComponentInChildren<AbsorptionObject>();
        }
    }
}