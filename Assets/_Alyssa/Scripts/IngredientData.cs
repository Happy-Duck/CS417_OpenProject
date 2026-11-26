using UnityEngine;

namespace Alyssa
{
    [CreateAssetMenu(fileName = "Ingredient", menuName = "Scriptable Objects/Ingredient")]
    public class IngredientData : ScriptableObject
    {
        public string displayName;
    }
}