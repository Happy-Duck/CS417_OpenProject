using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Dylan
{
    public sealed class MapTable : MonoBehaviour
    {
        [SerializeField] private XRControls xrControls;
        [SerializeField] private GameObject mapHUD;
        [SerializeField] private Camera mapCamera;
        [SerializeField] private Transform playerHead;
        [SerializeField] private RectTransform mapViewport;
        [SerializeField] private PortLabel[] portLabels;

        [Header("Map Camera")]
        [SerializeField, Min(1f)] private float startingHeight = 24f;
        [SerializeField, Min(1f)] private float minimumHeight = 12f;
        [SerializeField, Min(1f)] private float maximumHeight = 45f;
        [SerializeField, Min(0f)] private float zoomSpeed = 14f;
        [SerializeField, Min(0f)] private float panSpeed = 9f;

        [Serializable]
        private struct PortLabel
        {
            public Transform icon;
            public RectTransform label;
        }

        private InputAction pan;
        private InputAction zoomOut;
        private InputAction zoomIn;
        private InputAction close;
        private float startingSize;
        private float openingHeadHeight;
        private float height;

        internal bool CloseRequested => close.triggered;

        void Awake()
        {
            var mapActions = xrControls.MapActions;
            pan = mapActions.FindAction("Pan", true);
            zoomOut = mapActions.FindAction("Zoom Out", true);
            zoomIn = mapActions.FindAction("Zoom In", true);
            close = mapActions.FindAction("Close Map", true);
            startingSize = mapCamera.orthographicSize;
        }

        void OnDisable() => Close();

        internal void UpdateCamera()
        {
            Vector2 move = pan.ReadValue<Vector2>();
            float zoom = zoomOut.ReadValue<float>() - zoomIn.ReadValue<float>();
            if (move == Vector2.zero && zoom == 0f) return;

            height = Mathf.Clamp(
                height + zoom * zoomSpeed * Time.deltaTime, 
                minimumHeight, maximumHeight
            );
            Vector3 position = mapCamera.transform.position + 
                new Vector3(move.x, 0f, move.y) * panSpeed * Time.deltaTime;
            UpdateView(position);
        }

        internal void Open()
        {
            openingHeadHeight = playerHead.position.y;
            height = Mathf.Clamp(startingHeight, minimumHeight, maximumHeight);
            UpdateView(playerHead.position);
            mapCamera.enabled = true;
            mapHUD.SetActive(true);
        }

        internal void Close()
        {
            mapCamera.enabled = false;
            mapHUD.SetActive(false);
        }

        private void UpdateView(Vector3 position)
        {
            position.y = openingHeadHeight + height;
            mapCamera.transform.position = position;
            mapCamera.orthographicSize = startingSize * height / startingHeight;
        }

        internal void UpdatePortLabels()
        {
            Rect area = mapViewport.rect;
            foreach (var port in portLabels)
            {
                Vector3 viewport = mapCamera.WorldToViewportPoint(port.icon.position);
                Vector2 point = area.min + Vector2.Scale((Vector2)viewport, area.size);
                bool visible = viewport.z > 0f && area.Contains(point);
                port.label.gameObject.SetActive(visible);
                if (!visible) continue;

                Vector2 halfLabel = port.label.rect.size * 0.5f;
                float offset = halfLabel.y + 18f;
                float y = point.y + offset;
                if (y > area.yMax - halfLabel.y) y = point.y - offset;
                port.label.anchoredPosition = new Vector2(
                    Mathf.Clamp(point.x, area.xMin + halfLabel.x, area.xMax - halfLabel.x),
                    Mathf.Clamp(y, area.yMin + halfLabel.y, area.yMax - halfLabel.y)
                );
            }
        }
    }
}
