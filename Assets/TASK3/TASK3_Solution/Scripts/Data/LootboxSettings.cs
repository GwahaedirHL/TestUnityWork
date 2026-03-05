using System;

using UnityEngine;

namespace Task3Lootbox
{
    [CreateAssetMenu(fileName = "LootboxSettings", menuName = "Lootbox/LootboxSettings")]
    public class LootboxSettings : ScriptableObject
    {
        [SerializeField] ItemData[] items;
        [SerializeField] RaritySettings raritySettings;

        public ItemData[] BuildItems()
        {
            foreach (var item in items)
            {
                item.RarityColor = raritySettings.Color[item.Rarity];
                item.Weight = raritySettings.Weight[item.Rarity];
            }

            return items;
        }
    }
}