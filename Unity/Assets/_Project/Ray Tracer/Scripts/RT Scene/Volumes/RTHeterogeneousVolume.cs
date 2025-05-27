using System;
using _Project.Ray_Tracer.Scripts.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Project.Ray_Tracer.Scripts.RT_Scene.Volumes
{
    public abstract class RTHeterogeneousVolume : RTVolume
    {
        // Coefficients
        public abstract ColorTableEntry[] ColorLookupTable { get; }

        // Grid
        protected abstract float[,,] Grid { get; }
        protected abstract IntVector3 GridSize { get; }
        public abstract bool IsLoaded { get; }

        // TODO: remove when I'm sure color works
        private Vector3 UnityCoordsToGridCoords(Vector3 unityCoords)
        {
            Vector3 gridCoords = unityCoords;
            // We first translate so the unity cube is at (0, 0, 0). We of course do not actually translate the whole 
            // cube, we simply translate the given unityPosition to where it would be if the whole cube had been translated
            gridCoords -= Position;
            // Now that our position has its origin at (0, 0, 0) we can rotate it (since rotation is always around the origin)
            gridCoords = Quaternion.Inverse(transform.rotation) * gridCoords;
            // Now that the cube and grid are both rotated the same way, we can set their bottom left corners to the same position
            // The middle of the cube is currently at (0, 0, 0). We must use the cubes scale to set its bottom left corner to (0,0,0)
            gridCoords += Scale / 2;
            // Now that they both have the same rotation and their bottom left corners are at (0,0,0) we can scale them
            // so they are the same size
            // First divide by the scale of the unity volume, then multiply by the scale of the grid
            gridCoords.x = gridCoords.x / Scale.x * (GridSize.x - 1);
            gridCoords.y = gridCoords.y / Scale.y * (GridSize.y - 1);
            gridCoords.z = gridCoords.z / Scale.z * (GridSize.z - 1);
            // Now we have the grid coordinates equivalent to the given unity coordinates
            return gridCoords;
        }

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
                normalized.x * GridSize.x,
                normalized.y * GridSize.y,
                normalized.z * GridSize.z
            );
            return gridCoords;
        }

        private float NearestNeighbor(Vector3 gridCoords)
        {
            return Grid[(int)gridCoords.x, (int)gridCoords.y, (int)gridCoords.z];
        }

        private float TriLinearInterpolation(Vector3 point)
        {
            // Get the bottom left corner of the cube surrounding the point
            int x = point.x <= 0 ? 0 : (int)point.x;
            int y = point.y <= 0 ? 0 : (int)point.y;
            int z = point.z <= 0 ? 0 : (int)point.z;
            // edge cases in case the point is exactly on the far edge of the voxelgrid and original cube would be out of bound
            x = x < GridSize.x - 1 ? x : GridSize.x - 2;
            y = y < GridSize.y - 1 ? y : GridSize.y - 2;
            z = z < GridSize.z - 1 ? z : GridSize.z - 2;
            // // xD, yD, and zD are the differences between each of x, y, z and the smaller coordinate related
            float xD, yD, zD;
            xD = point.x - x;
            yD = point.y - y;
            zD = point.z - z;
            // Interpolate the x so we are left with a plane
            float[,] plane = new float[2, 2];
            plane[0, 0] = Grid[x, y, z] * (1 - xD) + Grid[x + 1, y, z] * xD;
            plane[0, 1] = Grid[x, y, z + 1] * (1 - xD) + Grid[x + 1, y, z + 1] * xD;
            plane[1, 0] = Grid[x, y + 1, z] * (1 - xD) + Grid[x + 1, y + 1, z] * xD;
            plane[1, 1] = Grid[x, y + 1, z + 1] * (1 - xD) + Grid[x + 1, y + 1, z + 1] * xD;
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
            return VolumeManager.Instance.interpolation switch
            {
                VolumeManager.InterpolationType.NearestNeighbor => NearestNeighbor(gridPos),
                VolumeManager.InterpolationType.Trilinear => TriLinearInterpolation(gridPos),
                _ => throw new ArgumentOutOfRangeException(nameof(VolumeManager.interpolation),
                    VolumeManager.Instance.interpolation, "invalid enum value")
            };
        }

        public struct ColorTableEntry
        {
            public float Density;
            public Color ColorAlpha;

            public ColorTableEntry(float density, Color colorAlpha)
            {
                Density = density;
                ColorAlpha = colorAlpha;
            }
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