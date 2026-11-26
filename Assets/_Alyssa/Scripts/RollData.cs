using UnityEngine;

namespace Alyssa
{
    [CreateAssetMenu(fileName = "Roll", menuName = "Scriptable Objects/Roll")]
    public class RollData : ScriptableObject
    {
        public string displayName;
        public IngredientData[] ingredients;   // the recipe; list an ingredient twice to require two
        public int piecesWhenSliced = 6;       // for the slicing step
        public float price;
    }
}