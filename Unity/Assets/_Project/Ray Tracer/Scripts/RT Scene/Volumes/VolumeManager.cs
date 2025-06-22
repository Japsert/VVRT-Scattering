using System;
using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RT_Scene.Volumes
{
    public class VolumeManager : MonoBehaviour
    {
        // [SerializeField] private VolumeAsset bucky;
        // [SerializeField] private VolumeAsset bunny;
        // [SerializeField] private VolumeAsset engine;
        // [SerializeField] private VolumeAsset hazelnut;
        
        public static VolumeManager Instance { get; private set; }

        private VolumeManager()
        {
        }

        public enum VolumeType
        {
            Bucky,
            Bunny,
            Engine,
            Hazelnut,
            Cloud,
        }

        public Type TypeOfVolumeType(VolumeType enumValue)
        {
            return enumValue switch
            {
                VolumeType.Bucky => typeof(Bucky),
                VolumeType.Bunny => typeof(Bunny),
                VolumeType.Engine => typeof(Engine),
                VolumeType.Hazelnut => typeof(Hazelnut),
                VolumeType.Cloud => typeof(Cloud),
                _ => throw new ArgumentOutOfRangeException(nameof(enumValue), enumValue, "invalid enum value")
            };
        }

        public enum InterpolationType
        {
            NearestNeighbor,
            Trilinear,
        }

        public InterpolationType Interpolation => InterpolationType.NearestNeighbor;

        private void Awake()
        {
            if (Instance)
                Debug.LogError($"Instance of {GetType()} instantiated twice!");
            Instance = this;

            // StartCoroutine(PreloadVolumes());
        }

        // private IEnumerator PreloadVolumes()
        // {
        //     Debug.Log("loading volumes...");
        //
        //     bucky.Load();
        //     // bunny.Preload();
        //     // engine.Preload();
        //     // hazelnut.Preload();
        //     yield return null;
        // }
    }
}