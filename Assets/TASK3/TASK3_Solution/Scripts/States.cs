using AxGrid.FSM;

using AxGrid.Model;

using UnityEngine;

namespace Task3Lootbox
{
    [State(StateName)]
    public class LootboxIdleState : FSMState
    {
        public const string StateName = "Idle";

        [Enter]
        private void EnterThis()
        {
            Model.Set(Constants.START_BUTTON_ENABLE, true);
            Model.Set(Constants.STOP_BUTTON_ENABLE, false);
        }

        [Bind(Constants.ON_BTN)]
        private void OnStartClick(string name)
        {
            if (name != Constants.START_BUTTON_NAME)
                return;

            Model.Set(Constants.START_BUTTON_ENABLE, false);
            Parent.Change(LootboxSpinningState.StateName);
        }

    }

    [State(StateName)]
    public class LootboxSpinningState : FSMState
    {
        public const string StateName = "Spinning";

        [Enter]
        private void EnterThis()
        {
            Model.EventManager.Invoke(Constants.START_SPIN_REQUEST);
        }

        [One(3f)]
        private void EnableStop()
        {
            Model.Set(Constants.STOP_BUTTON_ENABLE, true);
        }

        [Bind(Constants.ON_BTN)]
        private void OnStopClick(string name)
        {
            if (name != Constants.STOP_BUTTON_NAME)
                return;

            Model.Set(Constants.STOP_BUTTON_ENABLE, false);
            Parent.Change(LootboxStoppingState.StateName);
        }
    }

    [State(StateName)]
    public class LootboxStoppingState : FSMState
    {
        public const string StateName = "Stopping";

        [Enter]
        private void EnterThis()
        {
            Model.EventManager.Invoke(Constants.STOP_SPIN_REQUEST);
        }

        [Bind(Constants.SPIN_FINISHED)]
        private void SpinFinished(string pickedId)
        {
            Debug.Log($"[FROM STATE] Spin finished, picked id: {pickedId}");
            Parent.Change(LootboxFinishingState.StateName);
        }
    }

    [State(StateName)]
    public class LootboxFinishingState : FSMState
    {
        public const string StateName = "Finishing";

        [Enter]
        private void EnterThis()
        {
            Model.EventManager.Invoke(Constants.PARTICLES_START);
        }

        [One(3f)]
        private void StopParticles()
        {
            Model.EventManager.Invoke(Constants.PARTICLES_END);
            Parent.Change(LootboxIdleState.StateName);
        }
    }
}