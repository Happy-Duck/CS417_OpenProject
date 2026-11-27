using System.Collections.Generic;
using UnityEngine;

namespace Alyssa
{
    [RequireComponent(typeof(Cuttable))]
    public class TutorialHint : MonoBehaviour
    {
        [System.Serializable]
        public class Hint
        {
            public bool enabled = true;
            public string title;
            [TextArea] public string message;
            [Tooltip("Stops after this many shows per Play session (0 = no limit)")]
            public int maxShows = 3;
        }

        [Header("Messages ({tool} becomes e.g. 'Skinning Knife')")]
        public Hint wrongTool = new Hint { title = "Wrong knife", message = "This needs the <b>{tool}</b>.\nLook for the green tag.", maxShows = 0 };
        public Hint tooSlow = new Hint { title = "Swing faster", message = "Slice through quickly.\nA slow push won't cut." };
        public Hint notOnBoard = new Hint { title = "Use the board", message = "Place it on the cutting board before you cut." };
        public Hint wrongSpot = new Hint { title = "Cut at the ring", message = "Aim your blade at the glowing ring." };
        public Hint blocked = new Hint { title = "Not yet", message = "Something is in the way.\nFinish the earlier step first." };

        [Header("Display")]
        public Vector3 offset = new Vector3(0, 0.25f, 0);
        public float showSeconds = 4f;
        public float cooldown = 5f;                    // between hints of the same kind
        public float characterSize = 0.008f;
        public bool tagMatchingKnives = true;
        public TutorialPanel.PanelStyle style = TutorialPanel.PanelStyle.Warning();
        public TutorialPanel.PanelStyle tagStyle = TutorialPanel.PanelStyle.Tag();

        static readonly Dictionary<string, int> counts = new Dictionary<string, int>();
        static readonly Dictionary<string, float> lastShown = new Dictionary<string, float>();
        static TutorialPanel active;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            counts.Clear();
            lastShown.Clear();
            active = null;
        }

        public static string ToolName(KnifeType type)
        {
            return type + " Knife";
        }

        Cuttable cut;

        void Awake()
        {
            cut = GetComponent<Cuttable>();
            cut.onWrongTool.AddListener(() => Fire("tool", wrongTool, true));
            cut.onTooSlow.AddListener(() => Fire("slow", tooSlow, false));
            cut.onNotOnBoard.AddListener(() => Fire("board", notOnBoard, false));
            cut.onWrongSpot.AddListener(() => Fire("spot", wrongSpot, false));
            cut.onBlocked.AddListener(() => Fire("blocked", blocked, false));
        }

        void Fire(string kind, Hint h, bool tagKnives)
        {
            if (h == null || !h.enabled || TutorialPanel.ModalOpen) return;

            string key = kind + ":" + cut.requiredTool;

            counts.TryGetValue(key, out int n);
            if (h.maxShows > 0 && n >= h.maxShows) return;

            if (lastShown.TryGetValue(key, out float last) && Time.time - last < cooldown) return;

            counts[key] = n + 1;
            lastShown[key] = Time.time;

            if (active != null) active.Dismiss(true);

            string msg = h.message.Replace("{tool}", ToolName(cut.requiredTool));
            active = TutorialPanel.Create(h.title, new[] { msg }, transform, offset, false,
                showSeconds, characterSize, style, null);

            if (tagKnives && tagMatchingKnives) TagKnives();
        }

        void TagKnives()
        {
            foreach (Knife k in FindObjectsByType<Knife>(FindObjectsSortMode.None))
            {
                if (k.toolType != cut.requiredTool) continue;

                TutorialPanel.Create(ToolName(k.toolType), new[] { "Use this one" }, k.transform,
                    new Vector3(0f, 0.15f, 0f), false, showSeconds, characterSize, tagStyle, null);
            }
        }
    }
}