using System;

using UnityEngine;

namespace Task3Lootbox
{
    public enum Rarity
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Epic = 3,
        Legendary = 4
    }

    [Serializable]
    public struct RarityWeight
    {
        public Rarity rarity;

        [Range(1, 1000)]
        public int weight;

        public Color color;
    }


    [CreateAssetMenu(fileName = "RaritySettings", menuName = "Lootbox/RaritySettings")]
    public sealed class RaritySettings : ScriptableObject
    {
        [SerializeField] private RarityWeight[] entries;

        [NonSerialized] private Color[] colors;
        [NonSerialized] private int[] weights;

        public RarityColorLookup Color => new RarityColorLookup(colors);
        public RarityWeightLookup Weight => new RarityWeightLookup(weights);

        private void OnEnable()
        {
            BuildLookups();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            BuildLookups();
        }
#endif

        private void BuildLookups()
        {
            int count = GetRarityCount();

            if (colors == null || colors.Length != count)
                colors = new Color[count];

            if (weights == null || weights.Length != count)
                weights = new int[count];

            for (int i = 0; i < count; i++)
            {
                colors[i] = UnityEngine.Color.white;
                weights[i] = 1;
            }

            for (int i = 0; i < entries.Length; i++)
            {
                int idx = (int)entries[i].rarity;
                if ((uint)idx >= (uint)count)
                    continue;

                colors[idx] = entries[i].color;
                weights[idx] = entries[i].weight;
            }
        }

        private static int GetRarityCount()
        {
            return Enum.GetValues(typeof(Rarity)).Length;
        }

        public readonly struct RarityColorLookup
        {
            private readonly Color[] colors;

            public RarityColorLookup(Color[] colors)
            {
                this.colors = colors;
            }

            public Color this[Rarity rarity] => colors[(int)rarity];
        }

        public readonly struct RarityWeightLookup
        {
            private readonly int[] weights;

            public RarityWeightLookup(int[] weights)
            {
                this.weights = weights;
            }

            public int this[Rarity rarity] => weights[(int)rarity];
        }
    }
}