using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Task3Lootbox
{
    public class LootboxItemView : MonoBehaviour
    {
        [SerializeField] Image icon;
        [SerializeField] RectTransform rectTransform;
        [SerializeField] Image background;

        public RectTransform RT => rectTransform;

        public void SetData(ItemData itemData)
        {
            icon.sprite = itemData.Icon;
            background.color = itemData.RarityColor;
        }
    }
}