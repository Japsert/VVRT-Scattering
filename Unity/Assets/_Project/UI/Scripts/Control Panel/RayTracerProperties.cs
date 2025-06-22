using System;
using System.Collections;
using _Project.Ray_Tracer.Scripts;
using _Project.Scripts;
using _Project.UI.Scripts.Control_Panel.Property_Editors;
using _Project.UI.Scripts.Tutorial;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.UI.Scripts.Control_Panel
{
    /// <summary>
    /// A UI class that provides access to the properties of the current <see cref="UnityRayTracer"/> and
    /// <see cref="RayManager"/>. Any changes made to the shown properties will be applied to the ray tracer and ray
    /// manager.
    /// </summary>
    public class RayTracerProperties : MonoBehaviour
    {
        protected UnityRayTracer rayTracer;
        protected RayManager rayManager;
        private UIManager uiManager;
        private RTSceneManager rtSceneManager;

        [SerializeField] private BoolEdit renderShadowsEdit;
        [SerializeField] private FloatEdit recursionDepthEdit;
        [SerializeField] private ColorEdit backgroundColorEdit;

        [SerializeField] private BoolEdit showRaysEdit;
        [SerializeField] private BoolEdit hideNoHitRaysEdit;
        [SerializeField] private BoolEdit hideNegligibleRaysEdit;
        [SerializeField] private FloatEdit rayHideThresholdEdit;
        [SerializeField] private BoolEdit rayTransparencyEnabled;
        [SerializeField] private BoolEdit rayDynamicRadiusEnabled;
        [SerializeField] private BoolEdit rayColorContributionEnabled;
        [SerializeField] private FloatEdit rayTransThresholdEdit;
        [SerializeField] private FloatEdit rayTransExponentEdit;
        [SerializeField] private FloatEdit rayRadiusEdit;
        [SerializeField] private FloatEdit rayMinRadiusEdit;
        [SerializeField] private FloatEdit rayMaxRadiusEdit;

        [SerializeField] private BoolEdit animateEdit;
        [SerializeField] private BoolEdit animateSequentiallyEdit;
        [SerializeField] private BoolEdit loopEdit;
        [SerializeField] private FloatEdit speedEdit;

        [SerializeField] private IntEdit superSamplingFactorEdit;
        [SerializeField] private BoolEdit superSamplingVisualEdit;
        [SerializeField] private BoolEdit enablePointLightsEdit;
        [SerializeField] private BoolEdit enableSpotLightsEdit;
        [SerializeField] private BoolEdit enableAreaLightsEdit;

        [SerializeField] private DropdownEdit rayMarchAlgorithmEdit;
        [SerializeField] private FloatEdit stepSizeEdit;
        [SerializeField] private IntEdit nrRandomWalksSceneEdit;
        [SerializeField] private IntEdit nrRandomWalksRenderEdit;
        [SerializeField] private DropdownEdit phaseFunctionEdit;

        [SerializeField] private Button renderImageButton;
        [SerializeField] private Button openImageButton;
        [SerializeField] protected Button flyRoRTCameraButton;

        /// <summary>
        /// Show the ray tracer properties for the current <see cref="UnityRayTracer"/> and <see cref="RayManager"/>.
        /// These properties can be changed via the shown UI.
        /// </summary>
        public virtual void Show()
        {
            gameObject.SetActive(true);
            rayTracer = UnityRayTracer.Get();
            rayManager = RayManager.Get();
            uiManager = UIManager.Get();
            rtSceneManager = RTSceneManager.Get();

            // Ray tracer
            renderShadowsEdit.IsOn = rayTracer.RenderShadows;
            recursionDepthEdit.Value = rayTracer.MaxDepth;
            backgroundColorEdit.Color = rayTracer.BackgroundColor;

            // Visualization
            showRaysEdit.IsOn = rayManager.ShowRays;
            hideNoHitRaysEdit.IsOn = rayManager.HideNoHitRays;
            hideNegligibleRaysEdit.IsOn = rayManager.HideNegligibleRays;
            rayHideThresholdEdit.Value = rayManager.RayHideThreshold;
            rayTransparencyEnabled.IsOn = rayManager.RayTransparencyEnabled;
            rayTransExponentEdit.Value = rayManager.RayTransExponent;
            rayDynamicRadiusEnabled.IsOn = rayManager.RayDynamicRadiusEnabled;
            rayColorContributionEnabled.IsOn = rayManager.RayColorContributionEnabled;

            // Ray thickness
            rayRadiusEdit.Value = rayManager.RayRadius;
            rayMinRadiusEdit.Value = rayManager.RayMinRadius;
            rayMaxRadiusEdit.Value = rayManager.RayMaxRadius;

            // Animation
            animateEdit.IsOn = rayManager.Animate;
            speedEdit.Value = rayManager.Speed;
            animateSequentiallyEdit.IsOn = rayManager.AnimateSequentially;
            loopEdit.IsOn = rayManager.Loop;

            // Super sampling
            superSamplingFactorEdit.Value = rayTracer.SuperSamplingFactor;
            superSamplingVisualEdit.IsOn = rayTracer.SuperSamplingVisual;

            // Lights (???)
            enablePointLightsEdit.IsOn = rtSceneManager.Scene.EnablePointLights;
            enableSpotLightsEdit.IsOn = rtSceneManager.Scene.EnableSpotLights;
            enableAreaLightsEdit.IsOn = rtSceneManager.Scene.EnableAreaLights;

            // Volume marching
            rayMarchAlgorithmEdit.Value = (int)rayTracer.RayMarchAlgorithm;
            stepSizeEdit.Value = rayTracer.StepSizeSS;
            nrRandomWalksSceneEdit.Value = rayTracer.NrRandomWalksMSScene;
            nrRandomWalksRenderEdit.Value = rayTracer.NrRandomWalksMSRender;
            phaseFunctionEdit.Value = (int)rayTracer.PhaseFunction;
        }

        /// <summary>
        /// Hide the shown ray tracer properties.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private IEnumerator RunRenderImage()
        {
            yield return new WaitForFixedUpdate();
            yield return rayTracer.RenderImage();
            uiManager.RenderedImageWindow.SetImageTexture(rayTracer.Image);
            yield return null;
        }

        private void RenderImage()
        {
            uiManager.RenderedImageWindow.Show();
            uiManager.RenderedImageWindow.SetLoading();
            StartCoroutine(RunRenderImage());
        }

        private void ToggleImage()
        {
            uiManager.RenderedImageWindow.Toggle();
        }

        // TODO overhaul object order in levels and dependencies. It's becoming a bit difficult to get the right order 
        // TODO code wise. Objects should ideally set there own values on awake and do everything else on start.
        private void Start()
        {
            renderShadowsEdit.OnValueChanged.AddListener((value) => { RTSceneManager.Get().SetShadows(value); });
        }

        // TODO (JT): it's better design to not have each property set their dependants' interactivity,
        // but instead have each dependant set their own interactivity based on their dependencies,
        // whenever any of those dependencies' value changes.
        protected virtual void Awake()
        {
            // Ray tracer
            renderShadowsEdit.OnValueChanged.AddListener(value => { rayTracer.RenderShadows = value; });
            recursionDepthEdit.OnValueChanged.AddListener(value => { rayTracer.MaxDepth = (int)value; });
            backgroundColorEdit.OnValueChanged.AddListener(value => { rayTracer.BackgroundColor = value; });

            // Visualization
            showRaysEdit.OnValueChanged.AddListener(value =>
            {
                rayManager.ShowRays = value;
                hideNoHitRaysEdit.gameObject.SetActive(value);
                hideNegligibleRaysEdit.gameObject.SetActive(value);
                rayHideThresholdEdit.gameObject.SetActive(value);
                rayTransparencyEnabled.gameObject.SetActive(value);
                rayTransExponentEdit.gameObject.SetActive(value);
                rayDynamicRadiusEnabled.gameObject.SetActive(value);
                rayRadiusEdit.gameObject.SetActive(value);
                rayMinRadiusEdit.gameObject.SetActive(value);
                rayMaxRadiusEdit.gameObject.SetActive(value);
                rayColorContributionEnabled.gameObject.SetActive(value);
                animateEdit.gameObject.SetActive(value);
                speedEdit.gameObject.SetActive(value);
                animateSequentiallyEdit.gameObject.SetActive(value);
                loopEdit.gameObject.SetActive(value);
                superSamplingVisualEdit.gameObject.SetActive(value);
                nrRandomWalksSceneEdit.gameObject.SetActive(value);
            });
            hideNoHitRaysEdit.OnValueChanged.AddListener(value => { rayManager.HideNoHitRays = value; });
            hideNegligibleRaysEdit.OnValueChanged.AddListener(value =>
            {
                rayManager.HideNegligibleRays = value;
                rayHideThresholdEdit.Interactable = value;
            });
            rayHideThresholdEdit.OnValueChanged.AddListener(value => { rayManager.RayHideThreshold = value; });
            rayTransparencyEnabled.OnValueChanged.AddListener(value =>
            {
                rayManager.RayTransparencyEnabled = value;
                rayTransExponentEdit.Interactable = value;
            });
            rayTransExponentEdit.OnValueChanged.AddListener(value => { rayManager.RayTransExponent = value; });
            rayDynamicRadiusEnabled.OnValueChanged.AddListener(value =>
            {
                rayManager.RayDynamicRadiusEnabled = value;
                rayRadiusEdit.Interactable = !value;
                rayMinRadiusEdit.Interactable = value;
                rayMaxRadiusEdit.Interactable = value;
            });
            rayRadiusEdit.OnValueChanged.AddListener(value => { rayManager.RayRadius = value; });
            rayMinRadiusEdit.OnValueChanged.AddListener(value => { rayManager.RayMinRadius = value; });
            rayMaxRadiusEdit.OnValueChanged.AddListener(value => { rayManager.RayMaxRadius = value; });
            rayColorContributionEnabled.OnValueChanged.AddListener(value =>
            {
                rayManager.RayColorContributionEnabled = value;
            });

            // Animation
            animateEdit.OnValueChanged.AddListener(value =>
            {
                rayManager.Animate = value;
                speedEdit.Interactable = value;
                animateSequentiallyEdit.Interactable = value;
                loopEdit.Interactable = value;
            });
            speedEdit.OnValueChanged.AddListener(value => { rayManager.Speed = value; });
            animateSequentiallyEdit.OnValueChanged.AddListener(value => { rayManager.AnimateSequentially = value; });
            loopEdit.OnValueChanged.AddListener(value => { rayManager.Loop = value; });

            // Super sampling
            superSamplingFactorEdit.OnValueChanged.AddListener(value =>
            {
                rayTracer.SuperSamplingFactor = value;
                superSamplingVisualEdit.Interactable = value > 1;
            });
            superSamplingVisualEdit.OnValueChanged.AddListener(value => { rayTracer.SuperSamplingVisual = value; });

            // Lights (???)
            enablePointLightsEdit.OnValueChanged.AddListener(value =>
            {
                rtSceneManager.Scene.EnablePointLights = value;
            });
            enableSpotLightsEdit.OnValueChanged.AddListener(value =>
            {
                rtSceneManager.Scene.EnableSpotLights = value;
            });
            enableAreaLightsEdit.OnValueChanged.AddListener(value =>
            {
                rtSceneManager.Scene.EnableAreaLights = value;
            });

            // Volume marching
            Debug.Log("adding listener to ray march algorithm change");
            rayMarchAlgorithmEdit.onValueChanged.AddListener(RayMarchAlgorithmChanged);
            stepSizeEdit.OnValueChanged.AddListener(value => rayTracer.StepSizeSS = value);
            nrRandomWalksSceneEdit.OnValueChanged.AddListener(value => rayTracer.NrRandomWalksMSScene = value);
            nrRandomWalksRenderEdit.OnValueChanged.AddListener(value => rayTracer.NrRandomWalksMSRender = value);
            phaseFunctionEdit.onValueChanged.AddListener(PhaseFunctionChanged);

            // Buttons
            renderImageButton.onClick.AddListener(RenderImage);
            openImageButton.onClick.AddListener(ToggleImage);
            flyRoRTCameraButton.onClick.AddListener(() =>
            {
                showRaysEdit.IsOn = false; // This invokes the OnValueChanged event as well.
                FindObjectOfType<CameraController>().FlyToRTCamera(); // There should only be 1 CameraController.
            });
        }

        /// <summary>
        /// Enables or disables other properties' interactivity based on the currently selected
        /// volume marching algorithm.
        /// </summary>
        /// <param name="value">The new enum value. Can be any of
        /// <see cref="UnityRayTracer.RayMarchAlgorithmType"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException">If the provided enum value is undeclared.</exception>
        /// <remarks>This needs to be done in code, and not in the editor, because in the editor there's no way to
        /// set the interactivity of other properties based on the current enum value, as far as I know.</remarks>
        private void RayMarchAlgorithmChanged(int value)
        {
            Debug.Log("heard that ray march algorithm changed!");
            UnityRayTracer.RayMarchAlgorithmType enumValue = (UnityRayTracer.RayMarchAlgorithmType)value;
            rayTracer.RayMarchAlgorithm = enumValue;
            switch (enumValue)
            {
                case UnityRayTracer.RayMarchAlgorithmType.NoScattering:
                    stepSizeEdit.Interactable = false;
                    nrRandomWalksSceneEdit.Interactable = false;
                    nrRandomWalksRenderEdit.Interactable = false;
                    phaseFunctionEdit.Interactable = false;
                    break;
                case UnityRayTracer.RayMarchAlgorithmType.SingleScattering:
                    stepSizeEdit.Interactable = true;
                    nrRandomWalksSceneEdit.Interactable = false;
                    nrRandomWalksRenderEdit.Interactable = false;
                    phaseFunctionEdit.Interactable = true;
                    // complete task here instead of in the editor, because we don't have switch statements there
                    TutorialManager.CompleteTask("singleScattering");
                    break;
                case UnityRayTracer.RayMarchAlgorithmType.MultipleScattering:
                    stepSizeEdit.Interactable = false;
                    nrRandomWalksSceneEdit.Interactable = true;
                    nrRandomWalksRenderEdit.Interactable = true;
                    phaseFunctionEdit.Interactable = true;
                    // complete task here instead of in the editor, because we don't have switch statements there
                    TutorialManager.CompleteTask("multipleScattering");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(enumValue), enumValue, "invalid enum value");
            }
        }

        private void PhaseFunctionChanged(int value)
        {
            UnityRayTracer.PhaseFunctionType enumValue = (UnityRayTracer.PhaseFunctionType)value;
            rayTracer.PhaseFunction = enumValue;
            switch (enumValue)
            {
                case UnityRayTracer.PhaseFunctionType.Isotropic:
                    break;
                case UnityRayTracer.PhaseFunctionType.HenyeyGreenstein:
                    TutorialManager.CompleteTask("phaseFunction");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(enumValue), enumValue, "invalid enum value");
            }
        }
    }
}