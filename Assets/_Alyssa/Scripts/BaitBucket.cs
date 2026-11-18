
using UnityEngine;
using UnityEngine.Events;
using Shared;

namespace Alyssa
{
    public class BaitBucket : MonoBehaviour
    {
        public Resource bait;
        public UnityEvent onDeposit;

        void OnTriggerEnter(Collider other)
        {
            Scrap scrap = other.GetComponentInParent<Scrap>();
            if (scrap == null || scrap.consumed) return;

            scrap.consumed = true;
            if (bait != null) bait.Add(scrap.baitValue);
            onDeposit.Invoke();
            Destroy(scrap.gameObject);
        }
    }
}
