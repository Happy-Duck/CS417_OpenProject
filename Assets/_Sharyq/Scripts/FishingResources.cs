using UnityEngine;
using Shared;

namespace _Sharyq
{
    public class FishingResources : MonoBehaviour
    {
        private readonly ResourceManager rm = ResourceManager.Instance;

        private void Start()
        {
            rm.Add("Bait", 10);
            
            rm.Add("Salmon", 0);
            rm.Add("Tuna", 0);
            rm.Add("Yellow Tail", 0);
            
            rm.Add("Shrimp", 0);
            rm.Add("Scallop", 0);
            rm.Add("Seaweed", 0);
            rm.Add("Secret", 0);
        }
    }
}
