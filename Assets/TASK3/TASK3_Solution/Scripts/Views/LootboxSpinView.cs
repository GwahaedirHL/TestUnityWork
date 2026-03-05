using AxGrid;
using AxGrid.Base;
using AxGrid.Model;
using AxGrid.Path;

using System.Collections.Generic;

using UnityEngine;

namespace Task3Lootbox
{
    public class LootboxSpinView : MonoBehaviourExtBind
    {
        [SerializeField] private LootboxItemView itemViewTemplate;
        [SerializeField] private Transform itemsRoot;

        [Tooltip("How many ItemViews to spawn")]
        [SerializeField] private int itemsCount = 5;

        [Header("Scroll Settings")]
        [SerializeField] private float maxSpeed = 1500f;
        [SerializeField] private float accelerationDuration = 0.8f;
        [SerializeField] private float snapDuration = 0.2f;

        [Header("Slot Stop Feel")]
        [SerializeField, Range(0f, 1f)]
        private float overshootByItemHeight = 0.18f;

        [SerializeField, Range(0.01f, 2f)]
        private float brakeZoneByItemHeight = 0.65f;

        [SerializeField, Min(0f)]
        private float minStopSpeed = 60f;

        [SerializeField, Min(0f)]
        private float stopEpsilon = 0.05f;

        private float TopY => (itemsCount - 1) * itemHeight * 0.5f;
        private float BottomBound => -TopY - itemHeight;

        private readonly List<LootboxItemView> itemViews = new List<LootboxItemView>(5);

        private ItemData[] items;
        private ItemData picked;
        private LootboxItemView pickedView;

        private bool isSpinning;
        private bool stopRequested;
        private bool pickedPending;
        
        private float itemHeight;
        private float stopRemainingDist;

        private Vector2[] snapStartPositions;
        private Vector2[] snapTargetPositions;

        public void SetItems(ItemData[] items)
        {
            this.items = items;

            itemsRoot.ClearChildren();
            itemViews.Clear();

            itemHeight = itemViewTemplate.RT.rect.height;

            float topY = TopY;
            for (int i = 0; i < itemsCount; i++)
            {
                var view = Instantiate(itemViewTemplate, itemsRoot);
                view.RT.anchoredPosition = new Vector2(0f, topY - i * itemHeight);
                view.SetData(GetRandomItem());
                itemViews.Add(view);
            }
        }

        public void Spin(ItemData picked)
        {
            if (isSpinning)
                return;

            this.picked = picked;

            isSpinning = true;
            stopRequested = false;

            pickedPending = false;
            pickedView = null;
            stopRemainingDist = 0f;

            Path = new CPath();

            Path
                .EasingQuadEaseOut(accelerationDuration, 0f, maxSpeed, speed =>
                {
                    ScrollItems(speed * Time.deltaTime);
                })
                .Add(_ =>
                {
                    ScrollItems(maxSpeed * Time.deltaTime);
                    return stopRequested ? Status.OK : Status.Continue;
                })
                .Add(_ =>
                {
                    if (pickedView == null)
                    {
                        ScrollItems(maxSpeed * Time.deltaTime);
                        return Status.Continue;
                    }

                    if (stopRemainingDist <= stopEpsilon)
                    {
                        stopRemainingDist = 0f;
                        return Status.OK;
                    }

                    float brakeZone = Mathf.Max(1f, itemHeight * brakeZoneByItemHeight);

                    float speed;
                    if (stopRemainingDist > brakeZone)
                    {
                        speed = maxSpeed;
                    }
                    else
                    {
                        float t = Mathf.Clamp01(stopRemainingDist / brakeZone); // 1..0
                        speed = maxSpeed * (t * t);
                        speed = Mathf.Max(speed, minStopSpeed);
                    }

                    float delta = speed * Time.deltaTime;

                    if (delta <= 0f)
                        delta = stopRemainingDist;

                    delta = Mathf.Min(delta, stopRemainingDist);

                    stopRemainingDist -= delta;
                    ScrollItems(delta);

                    return stopRemainingDist <= stopEpsilon ? Status.OK : Status.Continue;
                })
                .Add(_ =>
                {
                    PrepareSnap();
                    return Status.Immediately;
                })
                .EasingQuadEaseOut(snapDuration, 0f, 1f, t =>
                {
                    for (int i = 0; i < itemViews.Count; i++)
                    {
                        itemViews[i].RT.anchoredPosition = Vector2.Lerp(
                            snapStartPositions[i],
                            snapTargetPositions[i],
                            t
                        );
                    }
                })
                .Action(() =>
                {
                    for (int i = 0; i < itemViews.Count; i++)
                        itemViews[i].RT.anchoredPosition = snapTargetPositions[i];

                    isSpinning = false;
                    pickedPending = false;
                    pickedView = null;
                    
                    Settings.Fsm?.Invoke(Constants.SPIN_FINISHED, picked.ID);
                });
        }

        [Bind(Constants.STOP_SPIN_REQUEST)]
        private void StopSpin()
        {
            if (!isSpinning)
                return;

            stopRequested = true;
            pickedPending = true;
        }

        private void ScrollItems(float deltaY)
        {
            float wrapHeight = itemsCount * itemHeight;

            for (int i = 0; i < itemViews.Count; i++)
            {
                var view = itemViews[i];
                var rt = view.RT;

                rt.anchoredPosition += Vector2.down * deltaY;

                if (rt.anchoredPosition.y >= BottomBound)
                    continue;

                rt.anchoredPosition = new Vector2(0f, rt.anchoredPosition.y + wrapHeight);

                if (pickedPending && pickedView == null)
                {
                    view.SetData(picked);
                    pickedView = view;
                    pickedPending = false;

                    float overshoot = itemHeight * overshootByItemHeight;
                    
                    stopRemainingDist = rt.anchoredPosition.y + overshoot;
                    continue;
                }
                
                if (view == pickedView)
                    continue;

                view.SetData(GetRandomItem());
            }
        }

        private ItemData GetRandomItem()
        {
            return items[Random.Range(0, items.Length)];
        }

        private void PrepareSnap()
        {
            int count = itemViews.Count;

            snapStartPositions = new Vector2[count];
            snapTargetPositions = new Vector2[count];

            for (int i = 0; i < count; i++)
                snapStartPositions[i] = itemViews[i].RT.anchoredPosition;

            float snapOffsetY = pickedView != null
                ? pickedView.RT.anchoredPosition.y
                : FindClosestToCenterY(snapStartPositions);

            for (int i = 0; i < count; i++)
                snapTargetPositions[i] = snapStartPositions[i] - new Vector2(0f, snapOffsetY);
        }

        private float FindClosestToCenterY(Vector2[] positions)
        {
            int closestIndex = 0;
            float minAbsY = float.MaxValue;

            for (int i = 0; i < positions.Length; i++)
            {
                float absY = Mathf.Abs(positions[i].y);
                if (absY < minAbsY)
                {
                    minAbsY = absY;
                    closestIndex = i;
                }
            }

            return positions[closestIndex].y;
        }
    }
}