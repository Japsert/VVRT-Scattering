using System;
using _Project.Ray_Tracer.Scripts.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Project.Ray_Tracer.Scripts.RT_Scene.Volumes
{
    public abstract class RTHeterogeneousVolume : RTVolume
    {
        public abstract VolumeManager.VolumeType VolumeType { get; }

        // Grid
        protected abstract float[,,] Grid { get; }
        protected abstract IntVector3 Size { get; }
        protected bool IsLoaded { get; set; }
        // public abstract void Preload();

        private float GridAt(int x, int y, int z)
        {
            // Debug.Log(Grid.Length);
            // return Grid[x + y * Size.x + z * Size.x * Size.y];
            try
            {
                return Grid[x, y, z];
            }
            catch (Exception)
            {
                // Debug.LogError($"{VolumeType}, {x}, {y}, {z}, {e}");
                return 0f;
            }
        }

        // [SerializeField] protected VolumeAsset volumeAsset;

        private Vector3 WorldPosToGridPos(Vector3 worldPoint)
        {
            // First, transform the position from world (scene) space to local (scene) space.
            // In local space, the center of the cube is at (0,0,0).
            // Vector3 localPoint = transform.InverseTransformPoint(worldPoint); // TODO: why doesn't this work?
            Vector3 localPoint = worldPoint - Position;
            localPoint = Quaternion.Inverse(transform.rotation) * localPoint;
            // To normalize the point to be in [0,1] for each dimension, we translate, then scale.
            // This means, for a cube of size 2x2x2 (but it works for any size) and thus a Scale of 2:
            // (-1,-1,-1) (bottom left) -T> (0,0,0) -S> (  0,   0,   0)
            // ( 0, 0, 0) (center)      -T> (1,1,1) -S> (0.5, 0.5, 0.5)
            // ( 1, 1, 1) (top right)   -T> (2,2,2) -S> (  1,   1,   1)
            Vector3 normalized = new(
                (localPoint.x + Scale.x / 2) / Scale.x,
                (localPoint.y + Scale.y / 2) / Scale.y,
                (localPoint.z + Scale.z / 2) / Scale.z
            );
            // Finally, we get the (non-interpolated) coordinates in the grid.
            Vector3 gridCoords = new(
                normalized.x * Size.x,
                normalized.y * Size.y,
                normalized.z * Size.z
            );
            return gridCoords;
        }

        private float NearestNeighbor(Vector3 gridCoords)
        {
            return GridAt((int)gridCoords.x, (int)gridCoords.y, (int)gridCoords.z);
        }

        private float TriLinearInterpolation(Vector3 point)
        {
            // Get the bottom left corner of the cube surrounding the point
            int x = point.x <= 0 ? 0 : (int)point.x;
            int y = point.y <= 0 ? 0 : (int)point.y;
            int z = point.z <= 0 ? 0 : (int)point.z;
            // edge cases in case the point is exactly on the far edge of the voxelgrid and original cube would be out of bound
            x = x < Size.x - 1 ? x : Size.x - 2;
            y = y < Size.y - 1 ? y : Size.y - 2;
            z = z < Size.z - 1 ? z : Size.z - 2;
            // // xD, yD, and zD are the differences between each of x, y, z and the smaller coordinate related
            float xD = point.x - x;
            float yD = point.y - y;
            float zD = point.z - z;
            // Interpolate the x so we are left with a plane
            float[,] plane = new float[2, 2];
            plane[0, 0] = GridAt(x, y, z) * (1 - xD) + GridAt(x + 1, y, z) * xD;
            plane[0, 1] = GridAt(x, y, z + 1) * (1 - xD) + GridAt(x + 1, y, z + 1) * xD;
            plane[1, 0] = GridAt(x, y + 1, z) * (1 - xD) + GridAt(x + 1, y + 1, z) * xD;
            plane[1, 1] = GridAt(x, y + 1, z + 1) * (1 - xD) + GridAt(x + 1, y + 1, z + 1) * xD;
            // Interpolate the y so we are left with the z line
            float[] line = new float[2];
            line[0] = plane[0, 0] * (1 - yD) + plane[1, 0] * yD;
            line[1] = plane[0, 1] * (1 - yD) + plane[1, 1] * yD;
            // Interpolate the z so we are left with the final value
            return line[0] * (1 - zD) + line[1] * zD;
        }

        public override float DensityAt(Vector3 worldPos)
        {
            Vector3 gridPos = WorldPosToGridPos(worldPos);
            return VolumeManager.Instance.Interpolation switch
            {
                VolumeManager.InterpolationType.NearestNeighbor => NearestNeighbor(gridPos),
                VolumeManager.InterpolationType.Trilinear => TriLinearInterpolation(gridPos),
                _ => throw new ArgumentOutOfRangeException(nameof(VolumeManager.Interpolation),
                    VolumeManager.Instance.Interpolation, "invalid enum value")
            };
        }

        protected new void Awake()
        {
            base.Awake();
        }

        // uses delta tracking
        // we loop until we find the step length, instead of just returning the random step size
        // this is so that we don't get a lot of fragmented rays in the ray tree, but instead one ray between each event
        public override float GetStepLength(Vector3 pos, Vector3 direction)
        {
            const int maxSteps = 1000;
            float distance = 0f;

            for (int step = 0; step < maxSteps; step++)
            {
                // get a random step size based on the maximum extinction
                float stepSize = -Mathf.Log(1f - Random.value) / MaxExtinction;

                distance += stepSize;
                Vector3 newPos = pos + direction * distance;

                if (!IsInBounds(newPos))
                    return distance; // exited volume

                float extinction = ExtinctionAt(newPos);
                float acceptance = extinction / MaxExtinction;

                if (Random.value < acceptance)
                    return distance; // accepted: scattering event

                // null-scattered, continue looping
            }

            Debug.LogWarning("delta tracking max iterations exceeded");
            return distance;
        }
    }
}