using System;
using _Project.Ray_Tracer.Scripts;
using _Project.Ray_Tracer.Scripts.RT_Scene;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Camera;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Light;
using _Project.Ray_Tracer.Scripts.RT_Scene.Volumes;
using TMPro;
using UnityEngine;

namespace _Project.UI.Scripts.Control_Panel
{
    /// <summary>
    /// A UI class that provides a basic panel for ray tracer, camera, light and mesh properties to be shown on.
    /// </summary>
    public class ControlPanel : MonoBehaviour
    {
        public enum SignalType
        {
            RayTracer,
            Camera,
            Object
        }

        public virtual void Subscribe(Action<SignalType> function)
        {
            rayTracerButton.onClick.AddListener(() => function(SignalType.RayTracer));
            cameraButton.onClick.AddListener(() => function(SignalType.Camera));
            objectButton.onClick.AddListener(() => function(SignalType.Object));
        }

        [SerializeField] private RayTracerProperties rayTracerProperties;
        [SerializeField] private CameraProperties cameraProperties;
        [SerializeField] private LightProperties lightProperties;
        [SerializeField] private HeterogeneousVolumeProperties heterogeneousVolumeProperties;
        [SerializeField] private HomogeneousVolumeProperties homogeneousVolumeProperties;
        [SerializeField] private MeshProperties meshProperties;
        [SerializeField] private TextMeshProUGUI emptyProperties;
        [SerializeField] private FolderButton rayTracerButton;
        [SerializeField] private FolderButton cameraButton;
        [SerializeField] private FolderButton objectButton;

        /// <summary>
        /// Show the control panel. By default this will not show any properties, just the panel background.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Show the ray tracer properties for the current <see cref="UnityRayTracer"/> and
        /// <see cref="RayManager"/>. These properties can be changed via the shown UI.
        /// </summary>
        public void ShowRayTracerProperties()
        {
            cameraProperties.Hide();
            lightProperties.Hide();
            meshProperties.Hide();
            heterogeneousVolumeProperties.Hide();
            homogeneousVolumeProperties.Hide();
            emptyProperties.gameObject.SetActive(false);
            cameraButton.Conceal();
            objectButton.Conceal();

            Show();
            rayTracerButton.Highlight();
            rayTracerProperties.Show();
        }

        /// <summary>
        /// Show the camera properties for <paramref name="camera"/>. These properties can be changed via the shown UI.
        /// </summary>
        /// <param name="camera"> The <see cref="RTCamera"/> whose properties will be shown. </param>
        public void ShowCameraProperties(RTCamera camera)
        {
            rayTracerProperties.Hide();
            lightProperties.Hide();
            meshProperties.Hide();
            heterogeneousVolumeProperties.Hide();
            homogeneousVolumeProperties.Hide();
            emptyProperties.gameObject.SetActive(false);
            objectButton.Conceal();
            rayTracerButton.Conceal();

            Show();
            cameraButton.Highlight();
            cameraProperties.Show(camera);
        }

        /// <summary>
        /// Show the light properties for <paramref name="light"/>. These properties can be changed via the shown UI.
        /// </summary>
        /// <param name="light"> The <see cref="RTLight"/> whose properties will be shown. </param>
        public void ShowLightProperties(RTLight light)
        {
            rayTracerProperties.Hide();
            cameraProperties.Hide();
            meshProperties.Hide();
            heterogeneousVolumeProperties.Hide();
            homogeneousVolumeProperties.Hide();
            emptyProperties.gameObject.SetActive(false);
            rayTracerButton.Conceal();
            cameraButton.Conceal();

            Show();
            objectButton.Highlight();
            lightProperties.Show(light);
        }

        /// <summary>
        /// Show the volume properties for the given heterogeneous volume.
        /// These properties can be changed via the shown UI.
        /// </summary>
        /// <param name="volume"> The heterogeneous volume whose properties will be shown. </param>
        /// <seealso cref="ShowHomogeneousVolumeProperties"/>
        public void ShowHeterogeneousVolumeProperties(RTHeterogeneousVolume volume)
        {
            rayTracerProperties.Hide();
            cameraProperties.Hide();
            lightProperties.Hide();
            meshProperties.Hide();
            homogeneousVolumeProperties.Hide();
            emptyProperties.gameObject.SetActive(false);
            cameraButton.Conceal();
            rayTracerButton.Conceal();

            Show();
            objectButton.Highlight();
            heterogeneousVolumeProperties.Show(volume);
        }

        /// <summary>
        /// Show the volume properties for the given homogeneous volume.
        /// These properties can be changed via the shown UI.
        /// </summary>
        /// <param name="volume"> The homogeneous volume whose properties will be shown. </param>
        /// <seealso cref="ShowHeterogeneousVolumeProperties"/>
        public void ShowHomogeneousVolumeProperties(RTHomogeneousVolume volume)
        {
            rayTracerProperties.Hide();
            cameraProperties.Hide();
            lightProperties.Hide();
            meshProperties.Hide();
            heterogeneousVolumeProperties.Hide();
            emptyProperties.gameObject.SetActive(false);
            cameraButton.Conceal();
            rayTracerButton.Conceal();

            Show();
            objectButton.Highlight();
            homogeneousVolumeProperties.Show(volume);
        }

        /// <summary>
        /// Show the mesh properties for <paramref name="mesh"/>. These properties can be changed via the shown UI.
        /// </summary>
        /// <param name="mesh"> The <see cref="RTMesh"/> whose properties will be shown. </param>
        public void ShowMeshProperties(RTMesh mesh)
        {
            rayTracerProperties.Hide();
            cameraProperties.Hide();
            lightProperties.Hide();
            emptyProperties.gameObject.SetActive(false);
            cameraButton.Conceal();
            rayTracerButton.Conceal();

            Show();
            objectButton.Highlight();
            meshProperties.Show(mesh);
        }

        public void ShowEmptyProperties()
        {
            rayTracerProperties.Hide();
            cameraProperties.Hide();
            lightProperties.Hide();
            cameraButton.Conceal();
            rayTracerButton.Conceal();

            Show();
            objectButton.Highlight();
            emptyProperties.gameObject.SetActive(true);
        }

        /// <summary>
        /// Hide the control panel and any properties shown on it.
        /// </summary>
        public void Hide()
        {
            rayTracerProperties.Hide();
            cameraProperties.Hide();
            lightProperties.Hide();
            meshProperties.Hide();
            heterogeneousVolumeProperties.Hide();
            homogeneousVolumeProperties.Hide();
            emptyProperties.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }

        private void Start()
        {
            ShowRayTracerProperties();
        }
    }
}