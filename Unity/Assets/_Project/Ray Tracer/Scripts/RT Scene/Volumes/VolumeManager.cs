using System;
using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RT_Scene.Volumes
{
    public class VolumeManager : MonoBehaviour
    {
        public static VolumeManager Instance { get; private set; }

        private VolumeManager()
        {
        }

        private readonly VolumeLoader _volumeLoader = new();

        public delegate void ActiveVolumesLoaded();

        public event ActiveVolumesLoaded OnActiveVolumesLoaded;

        public bool AreAllActiveVolumesLoaded() => _volumeLoader.AreAllActiveVolumesLoaded();

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

        [SerializeField] public InterpolationType interpolation;

        public enum InterpolationType
        {
            NearestNeighbor,
            Trilinear
        }
        
        private void Awake()
        {
            if (Instance)
                Debug.LogError($"Instance of {GetType()} instantiated twice!");
            Instance = this;
        }

        private void Start()
        {
            Debug.Log("loading volumes...");
            _volumeLoader.Load();
            _volumeLoader.OnActiveVolumesLoaded += () => { OnActiveVolumesLoaded?.Invoke(); };
        }
    }
}