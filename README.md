# Lootbox Mini Slot — AxGridTools test task

A vertical "slot" lootbox opening built on **AxGridTools** (Model, Binds, FSM, Path).

▶ **Demo video:** https://disk.yandex.ru/i/H6M8nCItFeX2LQ

## Task

- A vertical slot with 3 visible items and Start / Stop buttons
- Endless downward scroll with acceleration on start
- Stop becomes available only after 3 seconds
- A smooth stop that lands the center item exactly in the middle
- Logic driven by an FSM; buttons work through `UIButtonDataBind` and Model enable/disable flags
- Visual assets and at least one Particle System

## Implementation

### FSM
| State | Behavior |
|---|---|
| `Idle` | Start enabled, Stop disabled |
| `Spinning` | Spin starts; Stop unlocks after 3 seconds |
| `Stopping` | Stop is requested; waits for the stop animation to finish |
| `Finishing` | Plays particles for 3 seconds, then returns to `Idle` |

Buttons send a single `OnBtn` signal; the FSM owns all transitions and visual events. See `States.cs`.

### Buttons and Model
The FSM toggles `BtnStartEnable` / `BtnStopEnable` in the Model, so a button is disabled whenever its action makes no sense. See `Constants.cs`, `States.cs`.

### Endless scroll and precise stop
- A pool of 5 `LootboxItemView` (3 visible + hidden above and below) with wrap positioning: an item leaving the bottom moves to the top and gets a new icon.
- On Stop, the picked item is assigned to the hidden top item *before* it enters the visible area.
- The stop uses a brake zone with a slight overshoot, then snaps exactly to center.

See `LootboxSpinView.cs`.

### Weighted random by rarity
The item is picked at spin start using its rarity weight. See `LootboxController.cs` (`Pick`), `LootboxSettings.cs`, `RaritySettings.cs`.

`RaritySettings` (ScriptableObject) stores weight and color per rarity and builds lookup arrays in `OnEnable` / `OnValidate`:
```csharp
raritySettings.Color[rarity]
raritySettings.Weight[rarity]
```
`LootboxSettings` assigns the rarity color and weight to every `ItemData`.

### Visuals
- `LootboxItemView` shows the icon on a background tinted with the rarity color.
- Particles play at the end of a spin and are disabled after 3 seconds; the instance is created once and reused.

## Data

- `ItemData` — ScriptableObject: id, icon, rarity
- `RaritySettings` — ScriptableObject: weight and color per rarity
- `LootboxSettings` — combines items with rarity data

## Running

- Unity **6000.3.8f1**
- Open `Assets/TASK3/TASK3_Solution/Lootbox.unity` and press Play
- `ExampleFSM` and `ExamplePath` are AxGridTools examples from the original task template
