using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;

namespace Dylan
{
    [DefaultExecutionOrder(-10)]
    public class ShipSpeedVignette : MonoBehaviour, ITunnelingVignetteProvider
    {
        [SerializeField] private SteeringController steering;
        [SerializeField, Range(0f, 1f)] private float fullSpeedAperture = 0.70f;
        [SerializeField, Min(0f)] private float transitionDuration = 0.3f;

        public VignetteParameters vignetteParameters { get; } = new();

        private TunnelingVignetteController vignette;
        private MeshRenderer meshRenderer;
        private float startAperture;
        private float targetAperture;
        private float elapsed;

        void Awake()
        {
            vignette = GetComponent<TunnelingVignetteController>();
            meshRenderer = GetComponent<MeshRenderer>();
            vignetteParameters.CopyFrom(vignette.defaultParameters);
            vignetteParameters.easeInTime = 0f;
            vignetteParameters.easeOutTime = 0f;
        }

        void OnEnable()
        {
            vignetteParameters.apertureSize = 1f;
            targetAperture = 1f;
            meshRenderer.enabled = false;
            vignette.BeginTunnelingVignette(this);
        }

        void OnDisable()
        {
            meshRenderer.enabled = false;
            vignette.EndTunnelingVignette(this);
        }

        void Update()
        {
            float speed = steering.CurrentMode == SteeringController.Mode.Steering
                ? Mathf.Abs(steering.SpeedPercent) / 100f
                : 0f;
            float target = Mathf.Lerp(1f, fullSpeedAperture, speed);
            if (targetAperture != target)
            {
                startAperture = vignetteParameters.apertureSize;
                targetAperture = target;
                elapsed = 0f;
            }

            if (vignetteParameters.apertureSize == targetAperture) return;

            elapsed += Time.unscaledDeltaTime;
            
            float t = elapsed / transitionDuration;
            float aperture = Mathf.SmoothStep(startAperture, targetAperture, t);
            vignetteParameters.apertureSize = aperture;
            meshRenderer.enabled = aperture < 1f;
        }
    }
}
