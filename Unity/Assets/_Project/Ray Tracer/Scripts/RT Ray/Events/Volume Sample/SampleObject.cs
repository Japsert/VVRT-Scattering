using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RT_Ray.Events.Volume_Sample
{
    [RequireComponent(typeof(SampleRenderer))]
    public class SampleObject: MonoBehaviour
    {
        private VolumeSample _sample;
        private SampleRenderer _sampleRenderer;

        public void Draw()
        {
            _sampleRenderer.Origin = _sample.WorldPos;
        }

        public void Hide()
        {
            _sampleRenderer.Renderer.enabled = false;
        }

        public void Show()
        {
            _sampleRenderer.Renderer.enabled = true;
        }

        private void Awake()
        {
            _sampleRenderer = GetComponent<SampleRenderer>();
        }
    }
}