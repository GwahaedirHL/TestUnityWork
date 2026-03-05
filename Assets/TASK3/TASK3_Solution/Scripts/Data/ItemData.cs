using UnityEngine;

namespace Task3Lootbox
{
    [CreateAssetMenu(fileName = "LootboxItemData", menuName = "Lootbox/ItemDataSO")]
    public class ItemData : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private Sprite icon;
        [SerializeField] private Rarity rarity;

        public Rarity Rarity => rarity;
        public string ID => id;
        public Sprite Icon => icon;
        public Color RarityColor { get; set; }
        public int Weight { get; set; }
    }
}