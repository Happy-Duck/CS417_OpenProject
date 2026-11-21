using Shared;
using TMPro;
using UnityEngine;

namespace Dylan
{
    public sealed class ShipSpeedMeter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI readout;
        [SerializeField] private RectTransform track;
        [SerializeField] private RectTransform indicator;
        [SerializeField] private Vector3 viewOffset = new(0.47f, -0.255f, 0.88f);

        private Stat shipSpeed;
        private Transform view;

        internal void Show(Stat speed, Transform cameraTransform)
        {
            shipSpeed = speed;
            view = cameraTransform;
            FollowView();
            gameObject.SetActive(true);
        }

        internal void Hide() => gameObject.SetActive(false);

        void OnEnable()
        {
            shipSpeed.OnStatChanged += Refresh;
            Application.onBeforeRender += FollowView;
            Refresh(shipSpeed.Value);
        }

        void OnDisable()
        {
            shipSpeed.OnStatChanged -= Refresh;
            Application.onBeforeRender -= FollowView;
        }

        void LateUpdate() => FollowView();

        [BeforeRenderOrder(10000)]
        private void FollowView() => transform.SetPositionAndRotation(view.TransformPoint(viewOffset), view.rotation);

        private void Refresh(float speed)
        {
            int percent = Mathf.RoundToInt(speed);
            readout.text = percent switch
            {
                > 0 => $"+{percent}%",
                < 0 => $"−{-percent}%",
                _ => "0%"
            };

            indicator.anchoredPosition = new Vector2(Mathf.Clamp(speed / 100f, -1f, 1f) * track.rect.width * 0.5f, 0f);
        }
    }
}
