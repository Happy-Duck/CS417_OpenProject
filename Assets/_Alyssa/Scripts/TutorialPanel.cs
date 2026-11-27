using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Alyssa
{
    public class TutorialPanel : MonoBehaviour
    {
        [System.Serializable]
        public class PanelStyle
        {
            public Color background = new Color(0.07f, 0.09f, 0.14f, 0.95f);
            public Color accent = new Color(1f, 0.82f, 0.2f, 1f);   // border, title, button
            public Color textColor = Color.white;
            public Color buttonTextColor = new Color(0.07f, 0.09f, 0.14f, 1f);
            [Tooltip("Body text wraps at this many characters")]
            public int maxCharsPerLine = 32;
            public string nextLabel = "Next";
            public string confirmLabel = "Got it";

            public static PanelStyle Warning()
            {
                PanelStyle s = new PanelStyle();
                s.accent = new Color(1f, 0.45f, 0.25f, 1f);
                return s;
            }

            public static PanelStyle Tag()
            {
                PanelStyle s = new PanelStyle();
                s.accent = new Color(0.35f, 0.9f, 0.5f, 1f);
                s.maxCharsPerLine = 20;
                return s;
            }
        }

        // Panels that need a confirm click. Hints and proximity popups wait while one is open.
        public static int ModalCount;
        public static bool ModalOpen { get { return ModalCount > 0; } }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { ModalCount = 0; }

        // Rendered at a higher resolution, with Character Size compensated so authored sizes match the old popup
        const int FontSize = 96;
        const float FontScale = 64f / FontSize;

        class Shape
        {
            public Transform t;
            public Mesh mesh;
            public Material mat;
            float w = -1f, h = -1f, r = -1f;

            public void Set(float width, float height, float radius)
            {
                if (Mathf.Abs(width - w) < 0.0001f && Mathf.Abs(height - h) < 0.0001f && Mathf.Abs(radius - r) < 0.0001f) return;
                w = width; h = height; r = radius;
                BuildRounded(mesh, width, height, radius);
            }

            public void Free()
            {
                if (mesh != null) UnityEngine.Object.Destroy(mesh);
                if (mat != null) UnityEngine.Object.Destroy(mat);
            }
        }

        string title;
        string[] pages;
        Transform target;
        Vector3 offset, anchor;
        bool confirm;
        float closeAt;
        float cs;
        PanelStyle style;
        System.Action<bool> onClosed;

        int page;
        bool closing, counted, snapped;
        float anim, panelH;
        Camera cam;

        Shape shadow, border, bg, btn;
        TextMesh titleText, bodyText, btnLabel, pageText;
        BoxCollider btnCollider;
        XRSimpleInteractable interactable;
        readonly List<Shape> shapes = new List<Shape>();

        // lifetime <= 0 means it stays until confirmed or dismissed. onClosed receives true if the player confirmed.
        public static TutorialPanel Create(string title, string[] pages, Transform target, Vector3 offset,
            bool confirm, float lifetime, float characterSize, PanelStyle style, System.Action<bool> onClosed)
        {
            TutorialPanel p = new GameObject("TutorialPanel").AddComponent<TutorialPanel>();
            p.Init(title, pages, target, offset, confirm, lifetime, characterSize, style, onClosed);
            return p;
        }

        public void Dismiss(bool instant = false)
        {
            if (instant)
            {
                ReleaseModal();
                Destroy(gameObject);
                return;
            }
            Close(false);
        }

        void Init(string title, string[] pages, Transform target, Vector3 offset, bool confirm,
            float lifetime, float characterSize, PanelStyle style, System.Action<bool> onClosed)
        {
            this.title = title;
            this.pages = (pages != null && pages.Length > 0) ? pages : new string[] { "" };
            this.target = target;
            this.offset = offset;
            this.confirm = confirm;
            this.style = style != null ? style : new PanelStyle();
            this.onClosed = onClosed;

            cs = characterSize * FontScale;
            anchor = target != null ? target.position + offset : offset;
            closeAt = lifetime > 0f ? Time.time + lifetime : float.PositiveInfinity;

            if (confirm) { ModalCount++; counted = true; }

            Build();
            SetText();
            transform.position = anchor;
            transform.localScale = Vector3.one * 0.0001f;
        }

        // ---------- building ----------

        void Build()
        {
            shadow = MakeShape("Shadow", new Color(0f, 0f, 0f, 0.35f), -1);
            border = MakeShape("Border", style.accent, 0);
            bg = MakeShape("Background", style.background, 1);

            if (!string.IsNullOrEmpty(title))
                titleText = MakeText("Title", cs * 1.25f, style.accent, TextAnchor.UpperLeft, FontStyle.Bold, 2);
            bodyText = MakeText("Body", cs, style.textColor, TextAnchor.UpperLeft, FontStyle.Normal, 2);

            if (!confirm) return;

            btn = MakeShape("Button", style.accent, 3);
            btnLabel = MakeText("ButtonLabel", cs, style.buttonTextColor, TextAnchor.MiddleCenter, FontStyle.Bold, 4);

            if (pages.Length > 1)
            {
                Color dim = style.textColor;
                dim.a = 0.6f;
                pageText = MakeText("Pages", cs * 0.8f, dim, TextAnchor.MiddleLeft, FontStyle.Normal, 2);
            }

            // Non-trigger collider so ray interactors can hit it; collider goes on before the interactable
            btnCollider = btn.t.gameObject.AddComponent<BoxCollider>();
            interactable = btn.t.gameObject.AddComponent<XRSimpleInteractable>();
            interactable.selectEntered.AddListener(args => Advance());
            interactable.hoverEntered.AddListener(args => SetHover(true));
            interactable.hoverExited.AddListener(args => SetHover(false));
        }

        Shape MakeShape(string n, Color c, int order)
        {
            GameObject g = new GameObject(n);
            g.transform.SetParent(transform, false);

            Shape s = new Shape();
            s.t = g.transform;
            s.mesh = new Mesh();
            s.mesh.name = n;
            s.mesh.MarkDynamic();
            g.AddComponent<MeshFilter>().sharedMesh = s.mesh;

            Shader sh = Shader.Find("Sprites/Default");
            if (sh == null) sh = Shader.Find("UI/Default");
            s.mat = new Material(sh);
            s.mat.color = c;

            MeshRenderer mr = g.AddComponent<MeshRenderer>();
            mr.sharedMaterial = s.mat;
            mr.sortingOrder = order;
            mr.shadowCastingMode = ShadowCastingMode.Off;
            mr.receiveShadows = false;

            shapes.Add(s);
            return s;
        }

        static Font cachedFont;
        static Font GetFont()
        {
            if (cachedFont != null) return cachedFont;
            cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (cachedFont == null) cachedFont = Font.CreateDynamicFontFromOSFont("Arial", 64);
            return cachedFont;
        }

        TextMesh MakeText(string n, float size, Color c, TextAnchor anchorMode, FontStyle fs, int order)
        {
            GameObject g = new GameObject(n);
            g.transform.SetParent(transform, false);

            TextMesh tm = g.AddComponent<TextMesh>();
            tm.font = GetFont();
            tm.fontSize = FontSize;
            tm.fontStyle = fs;
            tm.characterSize = size;
            tm.anchor = anchorMode;
            tm.alignment = anchorMode == TextAnchor.MiddleCenter ? TextAlignment.Center : TextAlignment.Left;
            tm.richText = true;
            tm.color = c;

            MeshRenderer mr = g.GetComponent<MeshRenderer>();
            mr.sharedMaterial = tm.font.material;
            mr.sortingOrder = order;
            mr.shadowCastingMode = ShadowCastingMode.Off;
            mr.receiveShadows = false;
            return tm;
        }

        static void BuildRounded(Mesh m, float w, float h, float r)
        {
            const int seg = 6;
            r = Mathf.Max(0.0001f, Mathf.Min(r, Mathf.Min(w, h) * 0.5f));

            int n = (seg + 1) * 4;
            Vector3[] v = new Vector3[n + 1];
            Color32[] col = new Color32[n + 1];
            int[] tri = new int[n * 3];

            Vector2[] centers =
            {
                new Vector2(w * 0.5f - r, h * 0.5f - r),
                new Vector2(-w * 0.5f + r, h * 0.5f - r),
                new Vector2(-w * 0.5f + r, -h * 0.5f + r),
                new Vector2(w * 0.5f - r, -h * 0.5f + r)
            };

            int k = 0;
            for (int c = 0; c < 4; c++)
            {
                for (int i = 0; i <= seg; i++)
                {
                    float a = (c * 90f + i * 90f / seg) * Mathf.Deg2Rad;
                    v[k++] = new Vector3(centers[c].x + Mathf.Cos(a) * r, centers[c].y + Mathf.Sin(a) * r, 0f);
                }
            }

            v[n] = Vector3.zero;
            for (int i = 0; i <= n; i++) col[i] = new Color32(255, 255, 255, 255);

            for (int i = 0; i < n; i++)
            {
                tri[i * 3] = n;
                tri[i * 3 + 1] = (i + 1) % n;
                tri[i * 3 + 2] = i;
            }

            m.Clear();
            m.vertices = v;
            m.colors32 = col;
            m.triangles = tri;
            m.RecalculateBounds();
        }

        // ---------- text ----------

        void SetText()
        {
            if (titleText != null) titleText.text = title;

            string body = confirm ? pages[page] : string.Join("\n", pages);
            bodyText.text = Wrap(body, style.maxCharsPerLine);

            if (!confirm) return;
            btnLabel.text = page >= pages.Length - 1 ? style.confirmLabel : style.nextLabel;
            if (pageText != null) pageText.text = (page + 1) + " / " + pages.Length;
        }

        // Wraps by visible characters, so rich text tags like <b> don't count
        static string Wrap(string text, int maxChars)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            string[] paragraphs = text.Split('\n');

            for (int p = 0; p < paragraphs.Length; p++)
            {
                int line = 0;
                foreach (string word in paragraphs[p].Split(' '))
                {
                    int len = VisibleLength(word);
                    if (line > 0 && line + 1 + len > maxChars) { sb.Append('\n'); line = 0; }
                    else if (line > 0) { sb.Append(' '); line++; }

                    sb.Append(word);
                    line += len;
                }
                if (p < paragraphs.Length - 1) sb.Append('\n');
            }
            return sb.ToString();
        }

        static int VisibleLength(string s)
        {
            int n = 0;
            bool tag = false;
            foreach (char ch in s)
            {
                if (ch == '<') tag = true;
                else if (ch == '>') tag = false;
                else if (!tag) n++;
            }
            return n;
        }

        static Vector2 Size(TextMesh tm)
        {
            if (tm == null || string.IsNullOrEmpty(tm.text)) return Vector2.zero;
            Bounds b = tm.GetComponent<MeshRenderer>().localBounds;
            return new Vector2(b.size.x, b.size.y);
        }

        // ---------- layout ----------

        void LateUpdate()
        {
            Layout();
        }

        void Layout()
        {
            // Spacing is based on the text size, so it scales with Character Size
            float em = cs * FontSize * 0.1f;
            float pad = em * 0.6f;
            float gap = em * 0.45f;

            Vector2 ts = Size(titleText);
            Vector2 bs = Size(bodyText);

            float contentW = Mathf.Max(ts.x, bs.x);
            float btnW = 0f, btnH = 0f;

            if (confirm)
            {
                Vector2 ls = Size(btnLabel);
                btnW = Mathf.Max(ls.x + em * 1.8f, em * 4f);
                btnH = Mathf.Max(ls.y + em * 0.8f, em * 1.4f);
                float pageW = Size(pageText).x;
                contentW = Mathf.Max(contentW, btnW + (pageW > 0f ? pageW + gap * 2f : 0f));
            }

            float w = Mathf.Max(contentW, em * 8f) + pad * 2f;
            float h = pad * 2f + bs.y;
            if (titleText != null) h += ts.y + gap;
            if (confirm) h += gap + btnH;
            panelH = h;

            float radius = em * 0.6f;
            float bw = em * 0.1f;

            bg.Set(w, h, radius);
            bg.t.localPosition = new Vector3(0f, 0f, 0.001f);
            border.Set(w + bw * 2f, h + bw * 2f, radius + bw);
            border.t.localPosition = new Vector3(0f, 0f, 0.002f);
            shadow.Set(w + bw * 2f, h + bw * 2f, radius + bw);
            shadow.t.localPosition = new Vector3(em * 0.12f, -em * 0.12f, 0.003f);

            float left = -w * 0.5f + pad;
            float y = h * 0.5f - pad;

            if (titleText != null)
            {
                titleText.transform.localPosition = new Vector3(left, y, -0.001f);
                y -= ts.y + gap;
            }
            bodyText.transform.localPosition = new Vector3(left, y, -0.001f);

            if (!confirm) return;

            float cy = -h * 0.5f + pad + btnH * 0.5f;
            float cx = w * 0.5f - pad - btnW * 0.5f;
            btn.Set(btnW, btnH, btnH * 0.5f);
            btn.t.localPosition = new Vector3(cx, cy, -0.001f);
            btnLabel.transform.localPosition = new Vector3(cx, cy, -0.002f);
            btnCollider.size = new Vector3(btnW, btnH, 0.06f);
            if (pageText != null) pageText.transform.localPosition = new Vector3(left, cy, -0.001f);
        }

        // ---------- behavior ----------

        Camera Cam()
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) cam = FindAnyObjectByType<Camera>();
            return cam;
        }

        void Update()
        {
            // If the target is destroyed (e.g. a fish after its final cut), the panel stays where it was
            if (target != null) anchor = target.position + offset;

            Vector3 pos = anchor + Vector3.up * (panelH * 0.5f);
            transform.position = pos;

            Camera c = Cam();
            if (c != null)
            {
                Vector3 away = pos - c.transform.position;
                away.y *= 0.35f;   // stay mostly upright instead of tilting at the player
                if (away.sqrMagnitude > 0.0001f)
                {
                    Quaternion want = Quaternion.LookRotation(away, Vector3.up);
                    transform.rotation = snapped
                        ? Quaternion.Slerp(transform.rotation, want, 1f - Mathf.Exp(-14f * Time.deltaTime))
                        : want;
                    snapped = true;
                }
            }

            if (!closing && Time.time >= closeAt) Close(false);

            anim = Mathf.MoveTowards(anim, closing ? 0f : 1f, Time.deltaTime / (closing ? 0.15f : 0.22f));
            float s = closing ? anim : EaseOutBack(anim);
            transform.localScale = Vector3.one * Mathf.Max(s, 0.0001f);

            if (closing && anim <= 0f) Destroy(gameObject);
        }

        static float EaseOutBack(float x)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            float p = x - 1f;
            return 1f + c3 * p * p * p + c1 * p * p;
        }

        void Advance()
        {
            if (closing) return;

            if (page < pages.Length - 1)
            {
                page++;
                SetText();
            }
            else
            {
                Close(true);
            }
        }

        void SetHover(bool hover)
        {
            if (btn == null) return;
            btn.mat.color = hover ? Color.Lerp(style.accent, Color.white, 0.45f) : style.accent;
        }

        void Close(bool confirmed)
        {
            if (closing) return;
            closing = true;

            ReleaseModal();
            if (interactable != null) interactable.enabled = false;

            System.Action<bool> cb = onClosed;
            onClosed = null;
            if (cb != null) cb(confirmed);
        }

        void ReleaseModal()
        {
            if (!counted) return;
            counted = false;
            ModalCount = Mathf.Max(0, ModalCount - 1);
        }

        void OnDestroy()
        {
            ReleaseModal();
            foreach (Shape s in shapes) s.Free();
        }
    }
}