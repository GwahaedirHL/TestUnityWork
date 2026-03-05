using UnityEngine;

namespace Task3Lootbox
{
    public static class TransformExtensions
    {
        public static void ClearChildren(this Transform transform)
        {
            foreach (Transform child in transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
    }
}