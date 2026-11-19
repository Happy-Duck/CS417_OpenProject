using UnityEngine;

namespace Shared
{
    public class ResourceBinding : MonoBehaviour
    {
        [SerializeField] private Resource resource;

        public Resource Asset => resource;
    }
}
