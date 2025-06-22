using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RT_Scene.Volumes
{
    public class VolumeLoader : MonoBehaviour
    {
        public static VolumeLoader Instance { get; private set; }

        private VolumeLoader()
        {
        }

        public void Load()
        {
            LoadActiveVolumes();
            StartCoroutine(LoadAllVolumesInBackground());
        }

        private List<RTVolume> ActiveVolumes => RTSceneManager.Get().Scene.Volumes;

        // public bool AreAllActiveVolumesLoaded()
        // {
        //     return ActiveVolumes.All(volume => volume is not RTHeterogeneousVolume { IsLoaded: false });
        // }

        public delegate void ActiveVolumesLoaded();

        public event ActiveVolumesLoaded OnActiveVolumesLoaded;

        public delegate void AllVolumesLoaded();

        public event AllVolumesLoaded OnAllVolumesLoaded;

        private static readonly List<Type> AllVolumeTypes = new()
        {
            typeof(Bucky),
            typeof(Bunny),
            typeof(Engine),
            typeof(Hazelnut),
        };

        private void LoadActiveVolumes()
        {
            // Construct set of types of volumes active in scene
            HashSet<Type> activeVolumeTypes = new();
            foreach (RTHeterogeneousVolume activeVolume in ActiveVolumes.OfType<RTHeterogeneousVolume>())
                activeVolumeTypes.Add(activeVolume.GetType());

            // Call static Load method on each of the active types
            // List<Task> activeLoadTasks = new();
            const string loadMethodName = "Load";
            foreach (Type volumeType in activeVolumeTypes)
            {
                MethodInfo loadMethod = volumeType.GetMethod(loadMethodName, BindingFlags.Public | BindingFlags.Static);
                if (loadMethod == null)
                    throw new NotImplementedException($"{loadMethodName}() method not found on class {volumeType}!");
                loadMethod.Invoke(null, null);
            }

            // Wait until loading is done
            // if (activeLoadTasks.Any())
            //     Task.WaitAll(activeLoadTasks.ToArray());

            Debug.Log("active volumes loaded!");
            OnActiveVolumesLoaded?.Invoke();
        }

        private IEnumerator<object> LoadAllVolumesInBackground()
        {
            List<Task> backgroundLoadTasks = new();
            const string loadMethodName = "Load";
            foreach (Type volumeType in AllVolumeTypes)
            {
                MethodInfo loadMethod = volumeType.GetMethod(loadMethodName, BindingFlags.Public | BindingFlags.Static);
                if (loadMethod == null)
                    throw new NotImplementedException($"{loadMethodName}() method not found on class {volumeType}!");
                backgroundLoadTasks.Add((Task)loadMethod.Invoke(null, null));
            }

            Task allBackground = Task.WhenAll(backgroundLoadTasks);
            while (!allBackground.IsCompleted)
                yield return null;

            if (allBackground.IsFaulted)
                Debug.LogError($"Error(s) while loading volumes in background: {allBackground.Exception}");
            else
            {
                Debug.Log("all volumes loaded!");
                OnAllVolumesLoaded?.Invoke(); // see TODO in Awake()
            }
        }

        private void Awake()
        {
            if (Instance)
                Debug.LogError($"Instance of {GetType()} instantiated twice!");
            Instance = this;
        }
    }
}