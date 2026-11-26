using UnityEngine;

namespace Alyssa
{
    [CreateAssetMenu(fileName = "Dish", menuName = "Scriptable Objects/Dish")]
    public class DishData : ScriptableObject
    {
        public string displayName;
        public int piecesPerDish = 3;
        public float price;
    }
}