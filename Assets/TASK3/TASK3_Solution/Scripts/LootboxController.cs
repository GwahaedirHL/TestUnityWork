using AxGrid;
using AxGrid.Base;
using AxGrid.FSM;
using AxGrid.Model;

using UnityEngine;

using Random = UnityEngine.Random;

namespace Task3Lootbox
{
    public class LootboxController : MonoBehaviourExtBind
    {
        [SerializeField] private LootboxSettings lootboxData;
        [SerializeField] private LootboxSpinView spinView;
        [SerializeField] private Transform vfxRoot;
        [SerializeField] private ParticleSystem particlesPrefab;

        private ItemData[] items;
        private ItemData pickedItem;
        private ParticleSystem activeParticles;

        [OnStart]
        private void StartThis()
        {
            items = lootboxData.BuildItems();
            spinView.SetItems(items);

            Settings.Fsm = new FSM();
            Settings.Fsm.Add(new LootboxIdleState());
            Settings.Fsm.Add(new LootboxSpinningState());
            Settings.Fsm.Add(new LootboxStoppingState());
            Settings.Fsm.Add(new LootboxFinishingState());

            Settings.Fsm.Start(LootboxIdleState.StateName);
        }

        [OnUpdate]
        private void UpdateThis()
        {
            Settings.Fsm.Update(Time.deltaTime);
        }

        [Bind(Constants.START_SPIN_REQUEST)]
        private void StartSpin()
        {
            pickedItem = Pick();
            Debug.Log($"[Lootbox] picked ID: {pickedItem.ID}");
            spinView.Spin(pickedItem);
        }

        [Bind(Constants.PARTICLES_START)]
        private void SpawnParticles()
        {
            if(activeParticles == null)
                activeParticles = Instantiate(particlesPrefab, vfxRoot);

            activeParticles?.Play();
        }

        [Bind(Constants.PARTICLES_END)]
        private void DestroyParticles()
        {
            activeParticles?.Stop();
        }

        private ItemData Pick()
        {
            int totalWeight = 0;
            foreach (var item in items)
                totalWeight += item.Weight;

            int roll = Random.Range(1, totalWeight + 1);

            int cumulative = 0;
            foreach (var item in items)
            {
                cumulative += item.Weight;
                if (roll < cumulative)
                    return item;
            }

            return items[^1];
        }
    }
}