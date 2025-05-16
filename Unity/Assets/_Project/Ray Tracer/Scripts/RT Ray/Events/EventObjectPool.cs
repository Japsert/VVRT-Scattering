using _Project.Ray_Tracer.Scripts.RT_Ray.Events.Volume_Absorption;
using _Project.Ray_Tracer.Scripts.RT_Ray.Events.Volume_Sample;
using UnityEngine;
using UnityEngine.Pool;

namespace _Project.Ray_Tracer.Scripts.RT_Ray.Events
{
    public class EventObjectPool : MonoBehaviour
    {
        [SerializeField] private SampleObject samplePrefab;
        [SerializeField] private AbsorptionObject absorptionPrefab;
        private ObjectPool<SampleObject> _samplePool;
        private ObjectPool<AbsorptionObject> _absorptionPool;

        public EventObjectPool()
        {
            _samplePool = new ObjectPool<SampleObject>(
                () => Instantiate(samplePrefab),
                actionOnGet: sampleObject => sampleObject.gameObject.SetActive(true),
                actionOnRelease: sampleObject => sampleObject.gameObject.SetActive(false),
                actionOnDestroy: sampleObject => Destroy(sampleObject.gameObject),
                collectionCheck: true, defaultCapacity: 100
            );
            _absorptionPool = new ObjectPool<AbsorptionObject>(
                () => Instantiate(absorptionPrefab),
                actionOnGet: absorptionObject => absorptionObject.gameObject.SetActive(true),
                actionOnRelease: absorptionObject => absorptionObject.gameObject.SetActive(false),
                actionOnDestroy: absorptionObject => Destroy(absorptionObject.gameObject),
                collectionCheck: true, defaultCapacity: 50
            );
        }
    }
}