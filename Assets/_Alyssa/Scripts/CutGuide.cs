using UnityEngine;
using UnityEngine.Rendering;

namespace Alyssa
{
    [RequireComponent(typeof(LineRenderer))]
    public class CutGuide : MonoBehaviour
    {
        public Cuttable gate;
        public Material material;
        public SliceAxis axis = SliceAxis.Z;
        public Vector2 size = new Vector2(0.24f, 0.17f);
        public Vector2 center;
        public float squareness = 3f;
        public int segments = 48;
        public float lineWidth = 0.008f;
        public float dashLength = 0.03f;
        public Color color = new Color(1f, 0.9f, 0.2f, 1f);
        public float pulseSpeed = 3f;
        public float minBrightness = 0.4f;

        LineRenderer line;
        Material mat;
        Texture2D dashTexture;
        bool hasGate;

        void Awake()
        {
            hasGate = gate != null;

            line = GetComponent<LineRenderer>();
            ConfigureLine(line);

            mat = material != null ? new Material(material) : new Material(Shader.Find("Sprites/Default"));
            dashTexture = MakeDashTexture();
            mat.SetTexture("_BaseMap", dashTexture);
            mat.SetTexture("_MainTex", dashTexture);
            line.material = mat;
        }

        void OnValidate()
        {
            LineRenderer lr = GetComponent<LineRenderer>();
            if (lr == null) return;

            ConfigureLine(lr);
            if (material != null) lr.sharedMaterial = material;
        }

        void ConfigureLine(LineRenderer lr)
        {
            transform.localRotation = AxisRotation();

            lr.useWorldSpace = false;
            lr.loop = true;
            lr.textureMode = LineTextureMode.Tile;
            lr.alignment = LineAlignment.View;
            lr.shadowCastingMode = ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.widthMultiplier = lineWidth;
            lr.textureScale = new Vector2(1f / (Mathf.Max(dashLength, 0.001f) * 2f), 1f);

            int count = Mathf.Max(segments, 3);
            lr.positionCount = count;
            for (int i = 0; i < count; i++) lr.SetPosition(i, RingPoint(i));
        }

        Quaternion AxisRotation()
        {
            switch (axis)
            {
                case SliceAxis.X: return Quaternion.LookRotation(Vector3.right, Vector3.up);
                case SliceAxis.Y: return Quaternion.LookRotation(Vector3.up, Vector3.forward);
                default: return Quaternion.identity;
            }
        }

        Texture2D MakeDashTexture()
        {
            Texture2D tex = new Texture2D(16, 1, TextureFormat.RGBA32, false);
            for (int x = 0; x < 16; x++)
                tex.SetPixel(x, 0, x < 8 ? Color.white : new Color(1f, 1f, 1f, 0f));
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Point;
            tex.Apply();
            return tex;
        }

        Vector3 RingPoint(int i)
        {
            // Superellipse: squareness 2 is an ellipse, higher values give a rounded rectangle
            float e = 2f / Mathf.Max(squareness, 1f);
            float a = i / (float)Mathf.Max(segments, 3) * Mathf.PI * 2f;
            float c = Mathf.Cos(a);
            float s = Mathf.Sin(a);
            float x = Mathf.Sign(c) * Mathf.Pow(Mathf.Abs(c), e) * size.x * 0.5f + center.x;
            float y = Mathf.Sign(s) * Mathf.Pow(Mathf.Abs(s), e) * size.y * 0.5f + center.y;
            return new Vector3(x, y, 0f);
        }

        void OnDrawGizmosSelected()
        {
            int count = Mathf.Max(segments, 3);
            Quaternion parentRotation = transform.parent != null ? transform.parent.rotation : Quaternion.identity;
            Matrix4x4 m = Matrix4x4.TRS(transform.position, parentRotation * AxisRotation(), Vector3.one);

            Gizmos.color = color;
            Vector3 previous = m.MultiplyPoint3x4(RingPoint(count - 1));
            for (int i = 0; i < count; i++)
            {
                Vector3 point = m.MultiplyPoint3x4(RingPoint(i));
                Gizmos.DrawLine(previous, point);
                previous = point;
            }
        }

        void Update()
        {
            bool visible = !hasGate || (gate != null && gate.Ready);
            line.enabled = visible;
            if (!visible) return;

            float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            float brightness = Mathf.Lerp(minBrightness, 1f, t);

            Color c = color * brightness;
            c.a = color.a;
            mat.SetColor("_BaseColor", c);
            mat.SetColor("_Color", c);
            line.startColor = c;
            line.endColor = c;
        }

        void OnDestroy()
        {
            if (mat != null) Destroy(mat);
            if (dashTexture != null) Destroy(dashTexture);
        }
    }
}