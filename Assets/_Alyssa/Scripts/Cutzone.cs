using UnityEngine;

namespace Alyssa
{
    public class CutZone : MonoBehaviour
    {
        public Cuttable target;

        void Reset()
        {
            target = GetComponentInParent<Cuttable>();
        }

        public void Hit(float knifeSpeed, KnifeType tool)
        {
            if (target != null) target.TryHit(knifeSpeed, tool, true);
        }
    }
}