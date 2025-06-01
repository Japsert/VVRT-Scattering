using System;
using _Project.Ray_Tracer.Scripts.RT_Scene.Volumes;
using _Project.UI.Scripts.Control_Panel.Property_Editors;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace _Project.UI.Scripts.Control_Panel
{
    /// <summary>
    /// A UI class that provides access to the properties of an <see cref="RTVolume"/>. Any changes made to the shown
    /// properties will be applied to the volume.
    /// </summary>
    public class HomogeneousVolumeProperties : MonoBehaviour
    {
        private RTHomogeneousVolume _volume;
        private Type _currentVolumeType;

        [SerializeField] private Vector3Edit positionEdit;
        [SerializeField] private Vector3Edit rotationEdit;
        [SerializeField] private Vector3Edit scaleEdit;

        [SerializeField] private FloatEdit uniformDensityEdit;
        [SerializeField] private FloatEdit absorptionEdit;
        [SerializeField] private FloatEdit scatteringEdit;
        [SerializeField] private FloatEdit emissionEdit;
        [SerializeField] private FloatEdit gEdit;
        [SerializeField] private FloatDisplay extinctionDisplay;
        [SerializeField] private FloatDisplay albedoDisplay;

        [Serializable]
        public class ExternalChange : UnityEvent
        {
        };

        public ExternalChange OnExternalTranslationChange, OnExternalRotationChange, OnExternalScaleChange;

        /// <summary>
        /// Show the volume properties for <paramref name="volume"/>. These properties can be changed via the shown UI.
        /// </summary>
        /// <param name="volume"> The <see cref="RTVolume"/> whose properties will be shown. </param>
        public void Show(RTHomogeneousVolume volume)
        {
            gameObject.SetActive(true);
            _volume = volume;
            volume.transform.hasChanged = false;

            positionEdit.Value = volume.Position;
            rotationEdit.Value = volume.Rotation;
            scaleEdit.Value = volume.Scale;

            uniformDensityEdit.Value = _volume.UniformDensity;
            absorptionEdit.Value = _volume.Absorption;
            scatteringEdit.Value = _volume.Scattering;
            emissionEdit.Value = _volume.Emission;
            gEdit.Value = _volume.G;
            extinctionDisplay.Value = _volume.Extinction;
            albedoDisplay.Value = _volume.Albedo;
        }

        /// <summary>
        /// Hide the shown volume properties.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            _volume = null;
        }

        private void Awake()
        {
            positionEdit.OnValueChanged.AddListener((value) => { _volume.Position = value; });
            rotationEdit.OnValueChanged.AddListener((value) => { _volume.Rotation = value; });
            scaleEdit.OnValueChanged.AddListener((value) => { _volume.Scale = value; });
            
            uniformDensityEdit.OnValueChanged.AddListener(value => _volume.UniformDensity = value);
            absorptionEdit.OnValueChanged.AddListener(value => _volume.Absorption = value);
            scatteringEdit.OnValueChanged.AddListener(value => _volume.Scattering = value);
            emissionEdit.OnValueChanged.AddListener(value => _volume.Emission = value);
            gEdit.OnValueChanged.AddListener(value => _volume.G = value);
        }

        private void FixedUpdate()
        {
            // Update the UI based on external changes to the mesh transform (e.g. through the transformation gizmos).
            bool inUI = EventSystem.current.currentSelectedGameObject != null; // Only update if we are not in the UI.
            bool draggingEdit = positionEdit.IsDragging() || rotationEdit.IsDragging() || scaleEdit.IsDragging();
            if (gameObject.activeSelf && _volume.transform.hasChanged && !inUI && !draggingEdit)
            {
                if (positionEdit.Value != _volume.transform.position)
                {
                    positionEdit.Value = _volume.transform.position;
                    OnExternalTranslationChange?.Invoke();
                }

                if (rotationEdit.Value != _volume.transform.eulerAngles)
                {
                    rotationEdit.Value = _volume.transform.eulerAngles;
                    OnExternalRotationChange?.Invoke();
                }

                if (scaleEdit.Value != _volume.transform.localScale)
                {
                    scaleEdit.Value = _volume.transform.localScale;
                    OnExternalScaleChange?.Invoke();
                }

                _volume.transform.hasChanged = false;
            }
        }

        private void Update()
        {
            _volume.transform.hasChanged = false; // Do this in Update to let other scripts also check
        }
    }
}