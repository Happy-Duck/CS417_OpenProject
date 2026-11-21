using Shared;
using TMPro;
using UnityEngine;

namespace Dylan
{
    public sealed class ShipSpeedMeter : MonoBehaviour
    {
        [SerializeField] private Stat shipSpeed;
        [SerializeField] private TextMeshProUGUI readout;
        [SerializeField] private RectTransform track;
        [SerializeField] private RectTransform indicator;

        void OnEnable()
        {
            shipSpeed.OnStatChanged += Refresh;
            Refresh(shipSpeed.Value);
        }

        void OnDisable() => shipSpeed.OnStatChanged -= Refresh;

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
