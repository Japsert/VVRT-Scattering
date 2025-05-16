using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RT_Ray
{
    /// <summary>
    /// A Unity object that visually represent a ray traced by the ray tracer.
    /// </summary>
    [RequireComponent(typeof(RayBodyRenderer))]
    public class RayBodyObject : MonoBehaviour
    {
        /// <summary>
        /// The length to which this ray object is drawn. Generally this is the same as the length of <see cref="Ray"/>,
        /// but if the ray is infinitely long the drawn length will be set to
        /// <see cref="RayManager.InfiniteRayDrawLength"/>.
        /// </summary> 
        public float DrawLength { get; private set; }

        // TODO: this class is strongly coupled with its parent class
        private RayObject _rayObject;
        private RayBodyRenderer _rayBodyRenderer;
        private RayManager _rayManager;

        /// <summary>   
        /// Draw the ray as a cylinder where <paramref name="radius"/> determines the drawn radius of the cylinder. The
        /// length of the cylinder is the same as the ray's true length unless that length is infinity, then the length is
        /// clamped to <see cref="RayManager.InfiniteRayDrawLength"/>.
        /// </summary>
        /// <param name="radius"> The drawn radius of the cylinder. </param>
        public void Draw(float radius)
        {
            _rayBodyRenderer.Radius =
                _rayObject.Ray.AreaRay ? DrawLength : radius; // The arearay's radius is based on its length
            _rayBodyRenderer.Length = DrawLength;
        }

        /// <summary>
        /// Draw the ray as a cylinder where <paramref name="radius"/> determines the drawn radius of the cylinder. The
        /// length of the cylinder is given by <paramref name="length"/>, but it is clamped between 0 and
        /// <see cref="DrawLength"/>.
        /// </summary>
        /// <param name="radius"> The drawn radius of the cylinder. </param>
        /// <param name="length"> The drawn length of the cylinder. Clamped between 0 and <see cref="DrawLength"/> </param>
        public void Draw(float radius, float length)
        {
            length = Mathf.Clamp(length, 0.0f, DrawLength);
            _rayBodyRenderer.Radius =
                _rayObject.Ray.AreaRay ? length : radius; // The arearay's radius is based on its length
            _rayBodyRenderer.Length = length;
        }

        public void Reset()
        {
            // hack to avoid null ref exception. needs refactoring, see comment at top of class
            if (_rayObject.Ray == null) return;
            
            DetermineDrawLength();

            _rayBodyRenderer.Origin = _rayObject.Ray.Origin;
            _rayBodyRenderer.Direction = _rayObject.Ray.Direction;
            _rayBodyRenderer.Length = 0.0f;

            if (_rayObject.Ray.AreaRay) _rayBodyRenderer.SetAreaLightRay(_rayObject.Ray.AreaLightPoints);

            ReloadMaterial();
        }

        public void ReloadMaterial()
        {
            _rayBodyRenderer.Material = _rayManager.GetRayMaterial(_rayObject.Ray.Contribution, _rayObject.Ray.Type,
                _rayObject.Ray.Color, _rayObject.Ray.AreaRay);
        }

        private void DetermineDrawLength()
        {
            DrawLength = float.IsInfinity(_rayObject.Ray.Length)
                ? _rayManager.InfiniteRayDrawLength
                : _rayObject.Ray.Length;
        }

        private void Awake()
        {
            _rayObject = GetComponentInParent<RayObject>();
            _rayBodyRenderer = GetComponent<RayBodyRenderer>();
        }

        private void Start()
        {
            _rayManager = RayManager.Get();
        }

        private void OnEnable()
        {
            _rayManager = RayManager.Get();
        }
    }
}