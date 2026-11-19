using UnityEngine;
using UnityEngine.Events;

namespace Shared
{
    [CreateAssetMenu(fileName = "Cooking Events", menuName = "Events/Cooking Events")]
    public class CookingEvents : ScriptableObject
    {
        [SerializeField] private UnityEvent dishCompleted = new();

        public UnityEvent DishCompleted => dishCompleted;
    }
}
