using System;
using System.Collections;
using System.Collections.Generic;
using _Project.Ray_Tracer.Scripts.RT_Ray;
using _Project.Ray_Tracer.Scripts.RT_Scene;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Camera;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Light;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Point_Light;
using _Project.Ray_Tracer.Scripts.RT_Scene.Volumes;
using _Project.Ray_Tracer.Scripts.Utility;
using _Project.UI.Scripts;
using _Project.UI.Scripts.Render_Image_Window;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

namespace _Project.Ray_Tracer.Scripts
{
    /// <summary>
    /// A simple ray tracer that can render <see cref="RTScene"/> objects. The <see cref="Render"/> function is not
    /// efficient as it stores all rays it traces in a list of ray trees. Therefore, this function should only be used
    /// to produce a relatively small number of rays for the <see cref="RayManager"/> to visualize. For larger images
    /// (and no ray trees) the <see cref="RenderImage"/> function should be used.
    /// </summary>
    public partial class UnityRayTracer : MonoBehaviour
    {
        public delegate void RayTracerChanged();

        /// <summary>
        /// An event invoked whenever a property of this ray tracer is changed.
        /// </summary>
        public event RayTracerChanged OnRayTracerChanged;

        [SerializeField] private float epsilon = 0.001f;

        [SerializeField] private bool renderShadows = true;

        /// <summary>
        /// Whether this ray tracer renders shadows.
        /// </summary>
        public bool RenderShadows
        {
            get => renderShadows;
            set
            {
                if (value == renderShadows) return;
                renderShadows = value;
                OnRayTracerChanged?.Invoke();
            }
        }

        [SerializeField] private int maxDepth = 3;

        /// <summary>
        /// The maximum depth of any ray tree produced by this ray tracer.
        /// </summary>
        public int MaxDepth
        {
            get => maxDepth;
            set
            {
                if (value == maxDepth) return;
                maxDepth = value;
                OnRayTracerChanged?.Invoke();
            }
        }

        [SerializeField] private int superSamplingFactor = 1;

        /// <summary>
        /// The supersampling factor.
        /// </summary>
        public int SuperSamplingFactor
        {
            get => superSamplingFactor;
            set
            {
                if (value == superSamplingFactor) return;
                superSamplingFactor = value;
                OnRayTracerChanged?.Invoke();
            }
        }

        [SerializeField] protected bool superSamplingVisual = false;

        /// <summary>
        /// Whether SS is visualized.
        /// </summary>
        public bool SuperSamplingVisual
        {
            get => superSamplingVisual;
            set
            {
                if (value == superSamplingVisual) return;
                superSamplingVisual = value;
                OnRayTracerChanged?.Invoke();
            }
        }

        [SerializeField] private Color backgroundColor;

        /// <summary>
        /// The color produced by rays that don't intersect an object.
        /// </summary>
        public Color BackgroundColor
        {
            get => backgroundColor;
            set
            {
                if (value == backgroundColor) return;
                backgroundColor = value;
                UnityEngine.Camera.main.backgroundColor = backgroundColor;
                OnRayTracerChanged?.Invoke();
            }
        }

        public enum RayMarchAlgorithmType
        {
            NoScattering,
            SingleScattering,
            MultipleScattering
        }

        [Header("Volume Marching")] [SerializeField]
        private RayMarchAlgorithmType rayMarchAlgorithm = RayMarchAlgorithmType.NoScattering;

        public RayMarchAlgorithmType RayMarchAlgorithm
        {
            get
            {
                Debug.Log($"RayMarchAlg.get: {rayMarchAlgorithm}");
                return rayMarchAlgorithm;
            }
            set
            {
                if (value == rayMarchAlgorithm) return;
                rayMarchAlgorithm = value;
                OnRayTracerChanged?.Invoke();
            }
        }

        [SerializeField] private float stepSizeSS = 0.5f;

        public float StepSizeSS
        {
            get => stepSizeSS;
            set
            {
                if (value == stepSizeSS) return;
                stepSizeSS = value;
                OnRayTracerChanged?.Invoke();
            }
        }

        [SerializeField, Range(1, 5)] private int nrRandomWalksMSScene = 3;

        public int NrRandomWalksMSScene
        {
            get => nrRandomWalksMSScene;
            set
            {
                if (value == nrRandomWalksMSScene) return;
                nrRandomWalksMSScene = value;
                OnRayTracerChanged?.Invoke();
            }
        }

        [SerializeField, Range(1, 1000)] private int nrRandomWalksMSRender = 100;

        public int NrRandomWalksMSRender
        {
            get => nrRandomWalksMSRender;
            set
            {
                if (value == nrRandomWalksMSRender) return;
                nrRandomWalksMSRender = value;
                OnRayTracerChanged?.Invoke();
            }
        }

        public enum PhaseFunctionMSType
        {
            Isotropic,
            HenyeyGreenstein
        }

        [SerializeField] private PhaseFunctionMSType phaseFunctionMS = PhaseFunctionMSType.HenyeyGreenstein;

        public PhaseFunctionMSType PhaseFunctionMS
        {
            get => phaseFunctionMS;
            set
            {
                if (value == phaseFunctionMS) return;
                phaseFunctionMS = value;
                OnRayTracerChanged?.Invoke();
            }
        }

        private static UnityRayTracer _instance;
        protected RTSceneManager RTSceneManager;

        public Texture2D Image { get; private set; }

        protected RTScene Scene;
        protected RTCamera Camera;

        protected int RayTracerLayer;
        private int _rayTracerVolumeLayer;

        /// <summary>
        /// A class that stores the raw mesh data of a collider. This is used to cache a list of recently intersected
        /// meshes.
        /// </summary>
        private class MeshData
        {
            public Collider Collider;
            public Vector3[] Normals;
            public int[] Indices;
        }

        private const int CacheCapacity = 8;
        private static readonly List<MeshData> MeshCache = new(CacheCapacity);

        /// <summary>
        /// A struct that calculates and stores all relevant information about a ray-object intersection.
        /// </summary>
        private readonly struct HitInfo
        {
            public readonly Vector3 Point;
            public readonly Vector3 View;
            public readonly Vector3 Normal;
            public readonly bool InversedNormal;

            public readonly Color Color;
            public readonly float Ambient;
            public readonly float Diffuse;
            public readonly float Specular;
            public readonly float Shininess;
            public readonly float RefractiveIndex;
            public readonly bool IsTransparent;

            public HitInfo(RaycastHit hit, Vector3 direction, RTMesh mesh)
            {
                Point = hit.point;
                View = -direction;
                Normal = hit.normal;
                InversedNormal = false;

                // Get the material's properties.
                Color = mesh.Color;
                Ambient = mesh.Ambient;
                Diffuse = mesh.Diffuse;
                Specular = mesh.Specular;
                Shininess = mesh.Shininess;
                RefractiveIndex = mesh.RefractiveIndex;
                IsTransparent = mesh.type == RTMesh.ObjectType.Transparent;

                // Interpolate the hit normal to achieve smooth shading.
                if (mesh.ShadeSmooth)
                    Normal = SmoothedNormal(hit);

                // The shading normal always points in the direction of the view, as required by the Phong illumination
                // model.
                InversedNormal = Vector3.Dot(Normal, View) < -0.1f;
                Normal = InversedNormal ? -Normal : Normal;
            }

            private static Vector3 SmoothedNormal(RaycastHit hit)
            {
                // See if we have this mesh cached.
                MeshCollider meshCollider = hit.collider as MeshCollider;
                MeshData cachedMesh = MeshCache.Find(data => data.Collider == meshCollider);

                // If not, we add it to the cache.
                if (cachedMesh == null)
                {
                    Mesh mesh = meshCollider.sharedMesh;
                    cachedMesh = new MeshData
                    {
                        Collider = meshCollider,
                        Normals = mesh.normals,
                        Indices = mesh.triangles
                    };
                    MeshCache.Add(cachedMesh);
                }

                // Prevent excess memory use by limiting the cache capacity.
                while (MeshCache.Count > CacheCapacity)
                    MeshCache.RemoveAt(0);

                // Extract local space normals of the triangle we hit.
                Vector3 n0 = cachedMesh.Normals[cachedMesh.Indices[hit.triangleIndex * 3 + 0]];
                Vector3 n1 = cachedMesh.Normals[cachedMesh.Indices[hit.triangleIndex * 3 + 1]];
                Vector3 n2 = cachedMesh.Normals[cachedMesh.Indices[hit.triangleIndex * 3 + 2]];

                // Interpolate the normal using the barycentric coordinate of the hit point.
                Vector3 baryCenter = hit.barycentricCoordinate;
                Vector3 interpolatedNormal = n0 * baryCenter.x + n1 * baryCenter.y + n2 * baryCenter.z;
                interpolatedNormal = interpolatedNormal.normalized;

                // Transform local space normals to world space.
                Transform hitTransform = hit.collider.transform;
                interpolatedNormal = hitTransform.TransformDirection(interpolatedNormal);

                return interpolatedNormal;
            }
        }

        protected void CallRayTracerChanged()
        {
            OnRayTracerChanged?.Invoke();
        }


        /// <summary>
        /// Get the current <see cref="UnityRayTracer"/> instance.
        /// </summary>
        /// <returns> The current <see cref="UnityRayTracer"/> instance. </returns>
        public static UnityRayTracer Get()
        {
            return _instance;
        }

        /// <summary>
        /// Render the current <see cref="Scripts.RTSceneManager"/>'s <see cref="RTScene"/> while building up a list of ray trees.
        /// </summary>
        /// <returns> The list of ray trees that were traced to render the image. </returns>
        public List<TreeNode<RTRay>> Render()
        {
            AccelerationPrep();

            List<TreeNode<RTRay>> rayTrees = new();
            Scene = RTSceneManager.Scene;
            Camera = Scene.Camera;

            int width = Camera.ScreenWidth;
            int height = Camera.ScreenHeight;
            float aspectRatio = (float)width / height;
            float halfScreenHeight = Camera.ScreenDistance * Mathf.Tan(Mathf.Deg2Rad * Camera.FieldOfView / 2.0f);
            float halfScreenWidth = aspectRatio * halfScreenHeight;
            float pixelWidth = halfScreenWidth * 2.0f / width;
            float pixelHeight = halfScreenHeight * 2.0f / height;
            int ssFactor = superSamplingVisual ? SuperSamplingFactor : 1;
            int ssSquared = ssFactor * ssFactor;
            Vector3 origin = Camera.transform.position;
            float step = 1f / ssFactor;

            // Trace a ray for each pixel. 
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    Color color = Color.black;

                    // Set a base Ray with a zero-distance as the main ray of the pixel
                    float centerPixelX = -halfScreenWidth + pixelWidth * (x + 0.5f);
                    float centerPixelY = -halfScreenHeight + pixelHeight * (y + 0.5f);
                    Vector3 centerPixel = new(centerPixelX, centerPixelY, Camera.ScreenDistance);
                    TreeNode<RTRay> rayTree = new(new RTRay());
                    rayTree.Data = new RTRay(origin, centerPixel / centerPixel.magnitude, 0f, Color.black,
                        RTRay.RayType.Normal);

                    for (int supY = 0; supY < ssFactor; supY++)
                    {
                        float pixelY = centerPixelY + pixelHeight * (step * (0.5f + supY) - 0.5f);

                        for (int supX = 0; supX < ssFactor; supX++)
                        {
                            float pixelX = centerPixelX + pixelWidth * (step * (0.5f + supX) - 0.5f);

                            // Create and rotate the pixel location. Note that the camera looks along the positive z-axis.
                            Vector3 pixel = new(pixelX, pixelY, Camera.ScreenDistance);
                            pixel = Camera.transform.rotation * pixel;

                            // This is the distance between the pixel on the screen and the origin. We need this to compensate
                            // for the length of the returned RTRay. Since we have this factor we also use it to normalize this
                            // vector to make the code more efficient.
                            float pixelDistance = pixel.magnitude;

                            // Compensate for the location of the screen so we don't render objects that are behind the screen.
                            TreeNode<RTRay> subRayTree = traceFunc(origin + pixel,
                                pixel / pixelDistance, // Division by magnitude == .normalized.
                                MaxDepth, RTRay.RayType.Normal);


                            // Fix the origin and the length so we visualize the right ray.
                            subRayTree.Data.Origin = origin;
                            subRayTree.Data.Length += pixelDistance;

                            // Add the ray as a child of the main ray of the pixel.
                            subRayTree.Data.Contribution = 1.0f / ssSquared;
                            rayTree.AddChild(subRayTree);

                            color += subRayTree.Data.Color;
                        }
                    }

                    // Divide by superSamplingFactorSquared and set alpha levels back to 1. It should always be 1!
                    color /= ssSquared;
                    color.a = 1.0f;

                    rayTree.Data.Color = color;
                    SetContributions(rayTree);
                    rayTrees.Add(rayTree);
                }
            }

            AccelerationCleanupTree();
            return rayTrees;
        }

        private static void SetContributions(TreeNode<RTRay> parent)
        {
            parent.Children.ForEach(child =>
            {
                child.Data.Contribution *= parent.Data.Contribution;
                SetContributions(child);
            });
        }

        // TODO: add Henyey-Greenstein phase function
        private static float Phase()
        {
            return 1 / (4 * Mathf.PI);
        }

        // TODO: add documentation
        private TreeNode<RTRay> VolumeRayMarch_SingleScattering(TreeNode<RTRay> rayTree, RTVolume volume,
            RaycastHit hit, Vector3 direction, int depth)
        {
            Vector3 hitPointOffset = hit.point + direction * 0.001f;
            hit.collider.Raycast(new Ray(hitPointOffset, direction), out RaycastHit exitHit,
                Mathf.Infinity); // ignores overlapping volumes/objects

            float entryExitDistance = (exitHit.point - hit.point).magnitude;
            int nrSteps = Mathf.CeilToInt(entryExitDistance / stepSizeSS);
            float stride = entryExitDistance / nrSteps;

            float transparency = 1; // fully transparent
            Color result = Color.black;

            // ray march from enter to exit hit
            TreeNode<RTRay> prevNode = rayTree;
            for (int step = 0; step < nrSteps; step++)
            {
                float distanceFromEntry = stride * step;
                Vector3 rayOrigin = hit.point + (distanceFromEntry * direction);
                // TODO: jitter sample position to avoid banding?
                Vector3 samplePos = rayOrigin + (0.5f * stride * direction);

                RTRay newRay = new(rayOrigin, direction, stepSizeSS, Color.black, RTRay.RayType.Volume);
                newRay.AddSample(fracOfLength: 0f);
                TreeNode<RTRay> newRayNode = new(newRay);
                prevNode.AddChild(newRayNode);

                // evaluate density at sample position
                float density = volume.DensityAt(samplePos);
                float sampleAttenuation = Mathf.Exp(-stepSizeSS * density * volume.ExtinctionAt(samplePos));
                transparency *= sampleAttenuation; // attenuation due to absorption and out-scattering

                // in-scattering
                // TODO: extend to handle multiple lights
                // TODO: extend to handle objects in the way, to let them cast shadows on the volume
                // TODO: extend to handle overlapping volumes
                RTPointLight light = Scene.PointLights[0];
                Vector3 lightDirection = (light.Position - samplePos).normalized;
                hit.collider.Raycast(new Ray(samplePos, lightDirection), out RaycastHit exitHitLight,
                    Mathf.Infinity); // ignores overlapping volumes/objects

                if (density > 0)
                {
                    // TODO: these are light rays, so instead of coding them again, we should probably make all light
                    // rays in the scene attenuate and just shoot one of those here.

                    int nrStepsLight = Mathf.CeilToInt(exitHitLight.distance / stepSizeSS);
                    float strideLight = exitHitLight.distance / nrStepsLight;
                    float lightTransmittance = 1f;

                    TreeNode<RTRay> prevLightRayNode = newRayNode;
                    for (int stepLight = 0; stepLight < nrStepsLight; stepLight++)
                    {
                        float distanceFromSample = strideLight * stepLight;
                        Vector3 lightRayOrigin = samplePos + (lightDirection * distanceFromSample);
                        Vector3 lightSamplePos = lightRayOrigin + (0.5f * strideLight * lightDirection);

                        RTRay newLightRay = new(lightRayOrigin, lightDirection, strideLight,
                            Color.black, RTRay.RayType.VolumeLight);
                        newLightRay.AddLightSample();
                        TreeNode<RTRay> newLightRayNode = new(newLightRay);
                        prevLightRayNode.AddChild(newLightRayNode);

                        float extinctionLight = volume.ExtinctionAt(lightSamplePos);
                        if (extinctionLight > 0)
                            lightTransmittance *= Mathf.Exp(-strideLight * extinctionLight);

                        if (lightTransmittance < 0.001f)
                        {
                            newRay.AddAbsorption();
                            lightTransmittance = 0f;
                            break;
                        }

                        prevLightRayNode = newLightRayNode;
                    }

                    float scatteringAtSample = volume.ScatteringAt(samplePos);
                    result += light.Color * lightTransmittance * Phase() * scatteringAtSample * transparency * stride *
                              density;

                    // final ray to light
                    float distanceExitToLight = (light.Position - exitHitLight.point).magnitude;
                    prevLightRayNode.AddChild(new RTRay(exitHitLight.point, lightDirection, distanceExitToLight,
                        Color.black, RTRay.RayType.Light));
                }

                // TODO: russian roulette?

                prevNode = newRayNode;
            }

            Vector3 exitHitPointOffset = exitHit.point + direction * 0.001f;
            TreeNode<RTRay> afterVolumeRay = Trace(exitHitPointOffset, direction, depth, RTRay.RayType.Normal);
            prevNode.AddChild(afterVolumeRay);

            Color afterVolumeColor = afterVolumeRay.Data.Color;
            Color color = (afterVolumeColor * transparency) + result;
            rayTree.Data.Color = color;
            return rayTree;
        }

        private Vector3 UpdateDirectionRandom()
        {
            float z = 1f - 2f * Random.value; // cos(theta) from -1 to 1
            float phi = 2f * Mathf.PI * Random.value;

            float r = Mathf.Sqrt(1f - z * z);
            float x = r * Mathf.Cos(phi);
            float y = r * Mathf.Sin(phi);

            return new Vector3(x, y, z);
        }

        private Vector3 UpdateDirectionMaxAngle(Vector3 direction, int maxAngleDegrees)
        {
            direction.Normalize();
            float maxAngleRad = maxAngleDegrees * Mathf.Deg2Rad;

            // generate vector in random direction around z-axis
            float cosTheta = Mathf.Lerp(Mathf.Cos(maxAngleRad), 1f, Random.value);
            float sinTheta = Mathf.Sqrt(1f - cosTheta * cosTheta);
            float phi = Random.Range(0f, 2f * Mathf.PI);
            Vector3 localDirection = new(
                sinTheta * Mathf.Cos(phi),
                sinTheta * Mathf.Sin(phi),
                cosTheta);

            // rotate from z-axis to input direction
            Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, direction);
            return rotation * localDirection;
        }

        private Vector3 UpdateDirectionHG(Vector3 direction, float g)
        {
            // sample scattering angle cos(θ) from HG function
            float cosTheta;
            if (g == 0)
                cosTheta = 1f - 2f * Random.value;
            else
            {
                float sqr = (1f - g * g) / (1f - g + 2f * g * Random.value);
                cosTheta = (1f + g * g - sqr * sqr) / (2f * g);
            }

            // sample azimuthal angle φ
            float phi = 2f * Mathf.PI * Random.value;

            // construct sample direction in local coordinates
            // x,y,z is a vector in the coordinate system aligned with `direction`
            float sinTheta = Mathf.Sqrt(1f - cosTheta * cosTheta);
            float x = sinTheta * Mathf.Cos(phi);
            float y = sinTheta * Mathf.Sin(phi);
            float z = cosTheta;

            // build local orthonormal basis
            Vector3 w = direction.normalized;
            Vector3 up = Mathf.Abs(w.y) < 0.999f ? Vector3.up : Vector3.right;
            Vector3 u = Vector3.Cross(up, w).normalized;
            Vector3 v = Vector3.Cross(w, u);

            // convert from local to world (rotate x,y,z into world space)
            return x * u + y * v + z * w;
        }

        private TreeNode<RTRay> VolumeRayMarch_MultipleScattering(TreeNode<RTRay> entryRayNode, RTVolume volume,
            RaycastHit hit, Vector3 inDirection, int depth)
        {
            Color accumulatedColor = Color.black;

            for (int walk = 0; walk < nrRandomWalksMSScene; walk++)
                accumulatedColor += MSSingleRandomWalk(entryRayNode, volume, hit.point, inDirection, depth);

            accumulatedColor /= nrRandomWalksMSScene;
            accumulatedColor.a = 1f;
            entryRayNode.Data.Color = accumulatedColor;

            return entryRayNode;
        }

        private Color MSSingleRandomWalk(TreeNode<RTRay> entryNode, RTVolume volume, Vector3 entryPos,
            Vector3 inDirection, int depth)
        {
            const int maxSteps = 100;

            // offset first entry point to not hit it again due to floating point errors
            Vector3 prevPos = entryPos + inDirection * 0.001f;
            Vector3 currentDirection = inDirection;
            float pathTransmittance = 1f;

            TreeNode<RTRay> prevSegment = entryNode;
            for (int step = 0; step < maxSteps; step++)
            {
                float stepSize = volume.GetStepLength(prevPos, currentDirection);
                Vector3 currentPos = prevPos + (stepSize * currentDirection);

                if (!volume.IsInBounds(currentPos))
                {
                    // exited volume
                    bool didHitExit = volume.Intersect(prevPos, currentDirection, out RaycastHit exitHit,
                        Mathf.Infinity); // ignores overlapping volumes/objects

                    if (!didHitExit)
                    {
                        Debug.LogWarning("couldn't find exit point of random walk in volume, "
                                         + "starting at {prevPos} in direction {direction}");
                        return Color.black;
                    }

                    Vector3 exitHitOffset = exitHit.point + currentDirection * 0.001f;
                    TreeNode<RTRay> afterVolumeRayNode =
                        Trace(exitHitOffset, currentDirection, depth, RTRay.RayType.Normal);
                    Color afterVolumeColor = afterVolumeRayNode.Data.Color;
                    Color walkContributionColor = afterVolumeColor * pathTransmittance;

                    TreeNode<RTRay> lastSegment = new(new RTRay(prevPos, currentDirection, exitHit.distance,
                        walkContributionColor, RTRay.RayType.Volume));
                    lastSegment.Data.AddSample();
                    lastSegment.AddChild(afterVolumeRayNode);
                    prevSegment.AddChild(lastSegment);

                    return walkContributionColor;
                }

                TreeNode<RTRay> newRaySegment = new(new RTRay(prevPos, currentDirection, stepSize, Color.black,
                    RTRay.RayType.Volume));
                newRaySegment.Data.AddSample();
                prevSegment.AddChild(newRaySegment);

                // absorption
                float albedo = volume.AlbedoAt(currentPos);
                float absorptionProbability = 1f - albedo;
                if (Random.value < absorptionProbability)
                {
                    newRaySegment.Data.AddAbsorption();
                    return Color.black;
                }

                // scale transmittance to keep simulation unbiased
                pathTransmittance *= albedo;

                // update direction
                currentDirection = phaseFunctionMS switch
                {
                    PhaseFunctionMSType.Isotropic => UpdateDirectionRandom(),
                    PhaseFunctionMSType.HenyeyGreenstein => UpdateDirectionHG(currentDirection, volume.G),
                    _ => throw new ArgumentOutOfRangeException(nameof(phaseFunctionMS), phaseFunctionMS,
                        "invalid enum value")
                };

                prevPos = currentPos;
                prevSegment = newRaySegment;
            }

            // max steps reached, consider absorbed
            return Color.black;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin">Origin of the ray in world space.</param>
        /// <param name="direction">Direction of the ray, normalized.</param>
        /// <param name="depth">Number of remaining reflecting/refracting rays to compute.</param>
        /// <param name="type">Type of the ray.</param>
        /// <returns></returns>
        private TreeNode<RTRay> Trace(Vector3 origin, Vector3 direction, int depth, RTRay.RayType type)
        {
            int mask = rayMarchAlgorithm == RayMarchAlgorithmType.NoScattering ? RayTracerLayer : _rayTracerVolumeLayer;
            // If we did not hit anything we return a no hit ray whose result color is the backgroundcolor.
            if (!Physics.Raycast(origin, direction, out RaycastHit hit, Mathf.Infinity, mask))
                return new TreeNode<RTRay>(new RTRay(origin, direction, Mathf.Infinity, BackgroundColor,
                    RTRay.RayType.NoHit));

            RTMesh mesh = hit.transform.GetComponent<RTMesh>();
            RTRay ray = new(origin, direction, hit.distance, Color.black, type);
            TreeNode<RTRay> rayTree = new(ray);
            HitInfo hitInfo = new(hit, direction, mesh);

            if (mesh is RTVolume volume)
            {
                switch (rayMarchAlgorithm)
                {
                    case RayMarchAlgorithmType.NoScattering:
                        break;
                    case RayMarchAlgorithmType.SingleScattering: // TODO: add ambient component?
                        return VolumeRayMarch_SingleScattering(rayTree, volume, hit, direction, depth);
                    case RayMarchAlgorithmType.MultipleScattering:
                        return VolumeRayMarch_MultipleScattering(rayTree, volume, hit, direction, depth);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(rayMarchAlgorithm), rayMarchAlgorithm,
                            "invalid enum value");
                }
            }

            // Add the ambient component once, regardless of the number of lights.
            Color color = hitInfo.Ambient * hitInfo.Color;

            // Add diffuse and specular components for area, spot and pointlights.
            //TODO move this to it's own partial class
            Scene.PointLights.ForEach(pointLight => TracePointSpotLight(ref rayTree, pointLight, hitInfo));
            Scene.SpotLights.ForEach(spotLight => TracePointSpotLight(ref rayTree, spotLight, hitInfo));
            Scene.AreaLights.ForEach(areaLight => TraceAreaLight(ref rayTree, areaLight, in hitInfo));

            // Cast reflection and refraction rays.
            if (depth > 0)
                TraceReflectionAndRefraction(depth, hitInfo).ForEach(newRay => rayTree.AddChild(newRay));

            // Add the child ray colors to the parent ray.
            foreach (var child in rayTree.Children)
                color += child.Data.Color;

            // Calculate contribution to the parent.
            float rgb = ColorSumRGB(color);
            foreach (var child in rayTree.Children)
                child.Data.Contribution = rgb > 0f ? ColorSumRGB(child.Data.Color) / rgb : 0f;

            rayTree.Data.Color = ClampColor(color);

            return rayTree;
        }

        private RTRay TraceLight(ref Vector3 lightVector, Vector3 point, RTLight light, in HitInfo hitInfo)
        {
            // Determine the distance to the light source. Note the clever use of the dot product.
            float lightDistance = Vector3.Dot(lightVector, point - hitInfo.Point);

            // If we render shadows, check whether a shadow ray first meets the light or an object.
            if (RenderShadows)
            {
                Vector3 shadowOrigin = hitInfo.Point + epsilon * hitInfo.Normal;

                // Trace a ray until we reach the light source. If we hit something return a shadow ray.
                if (Physics.Raycast(shadowOrigin, lightVector, out RaycastHit shadowHit, lightDistance, RayTracerLayer))
                    return new RTRay(hitInfo.Point, lightVector, shadowHit.distance, Color.black, RTRay.RayType.Shadow);
            }

            // calculate attenuation influence
            float attenuation;
            if (0 == (attenuation = CalculateAttenuation(lightDistance, light, hitInfo)))
                return new RTRay(hitInfo.Point, lightVector, lightDistance - 0.01f, Color.black, RTRay.RayType.Shadow);

            // We either don't render shadows or nothing is between the object and the light source.

            // Calculate the color influence of this light.
            Vector3 reflectionVector = Vector3.Reflect(-lightVector, hitInfo.Normal);
            Color color = Color.black;
            color += Vector3.Dot(hitInfo.Normal, lightVector) * hitInfo.Diffuse * light.Diffuse *
                     light.Color * hitInfo.Color * light.Intensity; // Id
            color += Mathf.Pow(Mathf.Max(Vector3.Dot(reflectionVector, hitInfo.View), 0.0f), hitInfo.Shininess) *
                     hitInfo.Specular * light.Specular * light.Color * light.Intensity; // Is

            // Add attenuation
            color *= attenuation;

            // Lastly add ambient so it doesn't get attenuated
            color += light.Ambient * light.Color * hitInfo.Color;

            // Subtract 0.01f to not go through the light.
            return new RTRay(hitInfo.Point, lightVector, lightDistance - 0.01f, ClampColor(color), RTRay.RayType.Light);
        }

        private List<TreeNode<RTRay>> TraceReflectionAndRefraction(int depth, in HitInfo hitInfo)
        {
            List<TreeNode<RTRay>> rays = new();
            TreeNode<RTRay> node;

            // The object is transparent, and thus refracts and reflects light.
            if (hitInfo.IsTransparent)
            {
                // Calculate the refractive index.
                float nint = hitInfo.InversedNormal ? hitInfo.RefractiveIndex : 1.0f / hitInfo.RefractiveIndex;

                // Use Schlick's approximation to determine the ratio between refraction and reflection.
                float kr0 = Mathf.Pow((nint - 1.0f) / (nint + 1.0f), 2);
                float kr = kr0 + (1.0f - kr0) * Mathf.Pow(1.0f - Vector3.Dot(hitInfo.Normal, hitInfo.View), 5);
                float kt = 1.0f - kr;

                // Reflect.
                node = Trace(hitInfo.Point + hitInfo.Normal * epsilon,
                    Vector3.Reflect(-hitInfo.View, hitInfo.Normal),
                    depth - 1, RTRay.RayType.Reflect);
                node.Data.Color *= kr;
                rays.Add(node);

                // Refract.
                node = Trace(hitInfo.Point - hitInfo.Normal * epsilon,
                    Refract(-hitInfo.View, hitInfo.Normal, nint),
                    depth - 1, RTRay.RayType.Refract);
                node.Data.Color *= kt;
                rays.Add(node);

                return rays;
            }

            // The object is not transparent, so we only reflect (provided it has a nonzero specular component).
            if (hitInfo.Specular <= 0.0f) return rays;

            node = Trace(hitInfo.Point + hitInfo.Normal * epsilon,
                Vector3.Reflect(-hitInfo.View, hitInfo.Normal),
                depth - 1, RTRay.RayType.Reflect);
            node.Data.Color *= hitInfo.Specular;
            rays.Add(node);

            return rays;
        }

        /// <summary>
        /// Render the current <see cref="Scripts.RTSceneManager"/>'s <see cref="RTScene"/> while building up a "high resolution"
        /// image. Saved in <see cref="UnityRayTracer.Image"/>.
        /// </summary>
        public IEnumerator RenderImage()
        {
            AccelerationPrep();

            RenderedImageWindow renderedImageWindow = UIManager.Get().RenderedImageWindow;
            Scene = RTSceneManager.Scene;
            Camera = Scene.Camera;

            int width = Camera.ScreenWidth;
            int height = Camera.ScreenHeight;
            float aspectRatio = (float)width / height;

            // Scale width and height in such a way that the image has around a total of 160,000 pixels.
            int scaleFactor = Mathf.RoundToInt(Mathf.Sqrt(160000f / (width * height)));
            width = scaleFactor * width;
            height = scaleFactor * height;

            Image = new Texture2D(width, height, TextureFormat.RGBA32, false);

            // Calculate the other variables.
            float halfScreenHeight = Camera.ScreenDistance * Mathf.Tan(Mathf.Deg2Rad * Camera.FieldOfView / 2.0f);
            float halfScreenWidth = aspectRatio * halfScreenHeight;
            float pixelWidth = halfScreenWidth * 2.0f / width;
            float pixelHeight = halfScreenHeight * 2.0f / height;
            int superSamplingSquared = SuperSamplingFactor * SuperSamplingFactor;
            Vector3 origin = Camera.transform.position;
            float step = 1f / SuperSamplingFactor;

            // Trace a ray for each pixel.
            int percentage = 0;
            float start = Time.realtimeSinceStartup;
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    Color color = Color.black;

                    for (int supY = 0; supY < SuperSamplingFactor; supY++)
                    {
                        float difY = pixelHeight * (y + step * (0.5f + supY));

                        for (int supX = 0; supX < SuperSamplingFactor; supX++)
                        {
                            float difX = pixelWidth * (x + step * (0.5f + supX));

                            // Create and rotate the pixel location. Note that the camera looks along the positive z-axis.
                            Vector3 pixel = new(-halfScreenWidth + difX, -halfScreenHeight + difY,
                                Camera.ScreenDistance);
                            pixel = Camera.transform.rotation * pixel;

                            // Compensate for the location of the screen so we don't render objects that are behind the screen.
                            color += imageTraceFunc(origin + pixel, pixel.normalized, MaxDepth);
                        }
                    }

                    // Divide by supersamplingFactor squared and set alpha levels back to 1. It should always be 1!
                    color /= superSamplingSquared;
                    color.a = 1.0f;
                    Image.SetPixel(x, y, ClampColor(color));
                }

                // Update progress bar
                if (100 * y / height > percentage) // Update only when the percentage changes to limit yields.
                {
                    percentage = 100 * y / height;
                    renderedImageWindow.UpdateProgressBar(percentage);
                    yield return null; // yield to update UI and give the ability to cancel
                }
            }

            // Debug.Log("Triangle tests: " + trianglesTests);
            // Debug.Log(Time.realtimeSinceStartup - start);

            AccelerationCleanupImage();

            Image.Apply(); // Very important.

            yield return null;
        }

        // TODO: add documentation
        private Color VolumeRayMarch_SingleScattering_Image(RTVolume volume,
            RaycastHit hit, Vector3 direction, int depth)
        {
            Vector3 hitPointOffset = hit.point + direction * 0.001f;
            hit.collider.Raycast(new Ray(hitPointOffset, direction), out RaycastHit exitHit,
                Mathf.Infinity); // ignores overlapping volumes/objects

            float entryExitDistance = (exitHit.point - hit.point).magnitude;
            int nrSteps = Mathf.CeilToInt(entryExitDistance / stepSizeSS);
            float stride = entryExitDistance / nrSteps;

            float transmittance = 1; // fully transparent
            Color result = Color.black;

            // ray march from enter to exit hit
            for (int step = 0; step < nrSteps; step++)
            {
                float distanceFromEntry = stride * step;
                Vector3 rayOrigin = hit.point + (distanceFromEntry * direction);
                // TODO: jitter sample position to avoid banding?
                Vector3 samplePos = rayOrigin + (0.5f * stride * direction);

                // absorption and out-scattering
                float density = volume.DensityAt(samplePos);
                float sampleAttenuation = Mathf.Exp(-stepSizeSS * density * volume.ExtinctionAt(samplePos));
                transmittance *= sampleAttenuation; // attenuation due to extinction (absorption and out-scattering)

                // in-scattering
                // TODO: extend to handle multiple lights
                // TODO: extend to handle objects in the way, to let them cast shadows on the volume
                // TODO: extend to handle overlapping volumes
                RTPointLight light = Scene.PointLights[0];
                Vector3 lightDirection = (light.Position - samplePos).normalized;
                hit.collider.Raycast(new Ray(samplePos, lightDirection), out RaycastHit exitHitLight,
                    Mathf.Infinity); // ignores overlapping volumes/objects

                if (density > 0)
                {
                    // TODO: these are light rays, so instead of coding them again, we should probably make all light
                    // rays in the scene attenuate and just shoot one of those here.

                    int nrStepsLight = Mathf.CeilToInt(exitHitLight.distance / stepSizeSS);
                    float strideLight = exitHitLight.distance / nrStepsLight;
                    float lightTransmittance = 1f;

                    for (int stepLight = 0; stepLight < nrStepsLight; stepLight++)
                    {
                        float distanceFromSample = strideLight * stepLight;
                        Vector3 lightRayOrigin = samplePos + (lightDirection * distanceFromSample);
                        Vector3 lightSamplePos = lightRayOrigin + (0.5f * strideLight * lightDirection);

                        float extinctionLight = volume.ExtinctionAt(lightSamplePos);
                        if (extinctionLight > 0)
                            lightTransmittance *= Mathf.Exp(-strideLight * extinctionLight);

                        if (lightTransmittance < 0.001f)
                        {
                            lightTransmittance = 0f;
                            break;
                        }
                    }

                    float scatteringAtSample = volume.ScatteringAt(samplePos);
                    result += light.Color * lightTransmittance * Phase() * scatteringAtSample * transmittance * stride *
                              density;
                }

                // TODO: russian roulette?
            }

            Vector3 exitHitPointOffset = exitHit.point + direction * 0.001f;
            TreeNode<RTRay> afterVolumeRay = Trace(exitHitPointOffset, direction, depth, RTRay.RayType.Normal);

            Color afterVolumeColor = afterVolumeRay.Data.Color;
            Color color = (afterVolumeColor * transmittance) + result;
            return color;
        }

        private Color VolumeRayMarch_MultipleScattering_Image(RTVolume volume, RaycastHit hit, Vector3 inDirection,
            int depth)
        {
            Color accumulatedColor = Color.black;

            for (int walk = 0; walk < nrRandomWalksMSRender; walk++)
                accumulatedColor += MSSingleRandomWalk_Image(volume, hit.point, inDirection, depth);

            accumulatedColor /= nrRandomWalksMSScene;
            accumulatedColor.a = 1f;

            return accumulatedColor;
        }

        private Color MSSingleRandomWalk_Image(RTVolume volume, Vector3 entryPos, Vector3 inDirection, int depth)
        {
            const int maxSteps = 100;

            // offset first entry point to not hit it again due to floating point errors
            Vector3 prevPos = entryPos + inDirection * 0.001f;
            Vector3 currentDirection = inDirection;
            float pathTransmittance = 1f;

            for (int step = 0; step < maxSteps; step++)
            {
                float stepSize = volume.GetStepLength(prevPos, currentDirection);
                Vector3 currentPos = prevPos + (stepSize * currentDirection);

                if (!volume.IsInBounds(currentPos))
                {
                    // exited volume
                    volume.Intersect(prevPos, currentDirection, out RaycastHit exitHit,
                        Mathf.Infinity); // ignores overlapping volumes/objects

                    Vector3 exitHitOffset = exitHit.point + currentDirection * 0.001f;
                    Color afterVolumeColor = TraceImage(exitHitOffset, currentDirection, depth);

                    return afterVolumeColor * pathTransmittance;
                }

                // absorption
                float albedo = volume.AlbedoAt(currentPos);
                float absorptionProbability = 1f - albedo;
                if (Random.value < absorptionProbability)
                    return Color.black;

                // scale transmittance to keep simulation unbiased
                pathTransmittance *= albedo;

                // update direction
                currentDirection = phaseFunctionMS switch
                {
                    PhaseFunctionMSType.Isotropic => UpdateDirectionRandom(),
                    PhaseFunctionMSType.HenyeyGreenstein => UpdateDirectionHG(currentDirection, volume.G),
                    _ => throw new ArgumentOutOfRangeException(nameof(phaseFunctionMS), phaseFunctionMS,
                        "invalid enum value")
                };

                prevPos = currentPos;
            }

            // max steps reached, consider absorbed
            return Color.black;
        }

        protected virtual Color TraceImage(Vector3 origin, Vector3 direction, int depth)
        {
            int mask = rayMarchAlgorithm == RayMarchAlgorithmType.NoScattering ? RayTracerLayer : _rayTracerVolumeLayer;
            // If we did not hit anything we return the background color.
            if (!Physics.Raycast(origin, direction, out RaycastHit hit, Mathf.Infinity, mask))
                return BackgroundColor;

            RTMesh mesh = hit.transform.GetComponent<RTMesh>();
            HitInfo hitInfo = new(hit, direction, mesh);

            if (mesh is RTVolume volume)
            {
                switch (rayMarchAlgorithm)
                {
                    case RayMarchAlgorithmType.NoScattering:
                        break;
                    case RayMarchAlgorithmType.SingleScattering: // TODO: add ambient component?
                        return VolumeRayMarch_SingleScattering_Image(volume, hit, direction, depth);
                    case RayMarchAlgorithmType.MultipleScattering:
                        return VolumeRayMarch_MultipleScattering_Image(volume, hit, direction, depth);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(rayMarchAlgorithm), rayMarchAlgorithm,
                            "invalid enum value");
                }
            }

            // Add the ambient component once, regardless of the number of lights.
            Color color = hitInfo.Ambient * hitInfo.Color;

            // Add diffuse and specular components.
            Scene.PointLights.ForEach(pointLight => color += TracePointSpotLightImage(pointLight, in hitInfo));
            Scene.SpotLights.ForEach(spotLight => color += TracePointSpotLightImage(spotLight, in hitInfo));
            Scene.AreaLights.ForEach(areaLight => color += TraceAreaLightImage(areaLight, in hitInfo));

            // Cast reflection and refraction rays.
            if (depth > 0)
                color += TraceReflectionAndRefractionImage(depth, hitInfo);

            return ClampColor(color);
        }

        private Color TraceLightImage(ref Vector3 lightVector, Vector3 point, RTLight light, in HitInfo hitInfo)
        {
            // Determine the distance to the light source. Note the clever use of the dot product.
            float lightDistance = Vector3.Dot(lightVector, point - hitInfo.Point);

            // If we render shadows, check whether a shadow ray first meets the light or an object.
            if (RenderShadows)
            {
                Vector3 shadowOrigin = hitInfo.Point + epsilon * hitInfo.Normal;

                // Trace a ray until we reach the light source. If we hit something return a shadow ray.
                if (Physics.Raycast(shadowOrigin, lightVector, out _, lightDistance, RayTracerLayer))
                    return Color.black;
            }

            // calculate attenuation influence
            float attenuation;
            if (0 == (attenuation = CalculateAttenuation(lightDistance, light, hitInfo)))
                return Color.black;

            // We either don't render shadows or nothing is between the object and the light source.

            // Calculate the color influence of this light.
            Vector3 reflectionVector = Vector3.Reflect(-lightVector, hitInfo.Normal);
            Color color = Color.black;
            color += Vector3.Dot(hitInfo.Normal, lightVector) * hitInfo.Diffuse * light.Diffuse *
                     light.Color * hitInfo.Color * light.Intensity; // Id
            color += Mathf.Pow(Mathf.Max(Vector3.Dot(reflectionVector, hitInfo.View), 0.0f), hitInfo.Shininess) *
                     hitInfo.Specular * light.Specular * light.Color * light.Intensity; // Is

            // Add attenuation
            color *= attenuation;

            // Lastly add ambient so it doesn't get attenuated
            color += light.Ambient * light.Color * hitInfo.Color;

            return ClampColor(color);
        }

        private Color TraceReflectionAndRefractionImage(int depth, in HitInfo hitInfo)
        {
            // The object is transparent, and thus refracts and reflects light.
            if (hitInfo.IsTransparent)
            {
                Color color;

                // Calculate the refractive index.
                float nint = hitInfo.InversedNormal ? hitInfo.RefractiveIndex : 1.0f / hitInfo.RefractiveIndex;

                // Use Schlick's approximation to determine the ratio between refraction and reflection.
                float kr0 = Mathf.Pow((nint - 1.0f) / (nint + 1.0f), 2);
                float kr = kr0 + (1.0f - kr0) * Mathf.Pow(1.0f - Vector3.Dot(hitInfo.Normal, hitInfo.View), 5);
                float kt = 1.0f - kr;

                // Reflect.
                color = kr * TraceImage(hitInfo.Point + hitInfo.Normal * epsilon,
                    Vector3.Reflect(-hitInfo.View, hitInfo.Normal),
                    depth - 1);

                // Refract.
                color += kt * TraceImage(hitInfo.Point - hitInfo.Normal * epsilon,
                    Refract(-hitInfo.View, hitInfo.Normal, nint),
                    depth - 1);

                return ClampColor(color);
            }

            // The object is not transparent, so we only reflect (provided it has a non zero specular component).
            if (hitInfo.Specular > 0.0f)
                return hitInfo.Specular * TraceImage(hitInfo.Point + hitInfo.Normal * epsilon,
                    Vector3.Reflect(-hitInfo.View, hitInfo.Normal),
                    depth - 1);

            return Color.black;
        }

        // TODO: make methods like this in their class
        private static Vector3 Refract(Vector3 incident, Vector3 normal, float refractiveIndex)
        {
            float inputDot = Vector3.Dot(incident, normal);
            float root = 1.0f - (1.0f - inputDot * inputDot) * refractiveIndex * refractiveIndex;
            Vector3 refraction = (incident - inputDot * normal) * refractiveIndex;
            if (root < 0.0) return Vector3.Reflect(incident, normal);
            return refraction - normal * Mathf.Sqrt(root);
        }

        // TODO: make methods like this in their class
        protected static Color ClampColor(Color color)
        {
            float r = Mathf.Clamp01(color.r);
            float g = Mathf.Clamp01(color.g);
            float b = Mathf.Clamp01(color.b);
            return new Color(r, g, b);
        }

        // TODO: make methods like this in their class
        private static float ColorSumRGB(Color color)
        {
            return color.r + color.g + color.b;
        }

        protected virtual void Awake()
        {
            _instance = this;
            RayTracerLayer = LayerMask.GetMask("Ray Tracer Objects");
            _rayTracerVolumeLayer = LayerMask.GetMask("Ray Tracer Objects", "Volumes");
            AccelerationAwake();
        }

        private void Start()
        {
            RTSceneManager = RTSceneManager.Get();
            UnityEngine.Camera.main.backgroundColor = backgroundColor;
        }
    }
}