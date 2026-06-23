# Game Scene Setup (Beginner-friendly)

This file explains how to create `Game.unity` and place prefabs so the provided scripts work.

1) Create a new Scene
- In Unity Editor: File → New Scene. Save it as `Assets/Scenes/Game.unity`.

2) Create an empty GameObject `GridManager`
- Add component `GridManager` (script provided).
- Set `width` and `height` to `3` and `cellSize` to `1`.
- Create an empty child named `GridOrigin` at (0,0,0) or leave origin 0,0.

3) Create Prefabs
- Player: create a simple Cube or Sprite, add `PlayerController` component, tag it `Player`, and make a prefab `PlayerPrefab`.
  - In `PlayerController`, set `cell` to the desired start (e.g., X=0,Y=0) and assign `GridManager`.
- Wall: create Cube, tag `Wall`, make prefab `WallPrefab`.
- Box: create Cube, add `BoxController`, tag `Box`, make prefab `BoxPrefab`.
- Enemy: create simple object, add `EnemyController`, tag `Enemy`, set `cell` and `moveDir`, make prefab `EnemyPrefab`.
- Goal: create object, tag `Goal`, make prefab `GoalPrefab`.

4) Create `StageGenerator` object
- Create an empty GameObject `StageGenerator`, add the `StageGenerator` script.
- Assign `GridManager`, and drag `PlayerPrefab`, `GoalPrefab`, `WallPrefab`, `BoxPrefab` into the inspector slots.
- Click `GenerateDefault3x3()` from the script context menu or call it from an editor script to populate the scene.

5) Create `GameManager`
- Create empty GameObject `GameManager`, add `GameManager` script.
- Assign references: `GridManager`, `Player` (drag the Player instance), `StageGenerator`.
- For `boxes` and `enemies`, drag existing Box and Enemy instances into the lists.

6) UI Buttons
- Create Canvas → Buttons for Up/Down/Left/Right.
- Add `UIController` component to a GameObject and assign `GameManager`.
- On each Button's OnClick, add the UIController object and select `UIController.OnUp/OnDown/OnLeft/OnRight` accordingly.

7) Tags
- Make sure tags exist in Unity: add tags `Player`, `Wall`, `Box`, `Enemy`, `Goal` via the Tag Manager, and assign them to prefabs/instances.

8) Play
- Enter Play mode. Use UI buttons to move the player. The console will show `Clear!` when player reaches goal.

Notes and next steps
- The current scripts implement a minimal playable flow (Phase 1 + basic Box and Enemy movement). They assume boxes/enemies are present as instances in the scene and registered in `GameManager` lists.
- For full random stage generation, inspect `StageGenerator` and `Solver.cs`—these are minimal and can be extended to follow SPEC.md rules.

Troubleshooting
- If objects snap to wrong positions, ensure `cell` coordinates are set correctly on components and `cellSize`/`origin` values match.
- If movement doesn't happen, check that `GridManager` is assigned on `PlayerController` and `BoxController`.

