using UnityEngine;

namespace Alyssa
{
    public class CuttingBoardZone : MonoBehaviour
    {
        void OnTriggerEnter(Collider other)
        {
            Cuttable c = other.GetComponentInParent<Cuttable>();
            if (c != null) c.boardContacts++;
        }

        void OnTriggerExit(Collider other)
        {
            Cuttable c = other.GetComponentInParent<Cuttable>();
            if (c != null) c.boardContacts = Mathf.Max(0, c.boardContacts - 1);
        }
    }
}