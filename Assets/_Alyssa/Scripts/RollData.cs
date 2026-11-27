using UnityEngine;

namespace Alyssa
{
    [CreateAssetMenu(fileName = "Roll", menuName = "Scriptable Objects/Roll")]
    public class RollData : ScriptableObject
    {
        public string displayName;
        public IngredientData[] ingredients;
        public GameObject rolledPrefab;        // the whole roll
        public GameObject slicePrefab;         // one cut piece of this roll
        public int piecesWhenSliced = 6;
        public float price;
    }
}