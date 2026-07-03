# PLAN.md
# 重力迷宮パズル Unity開発計画書

## 0. 前提
本開発計画書は `SPEC.md` を仕様書として参照し、Unityでゲームを完成させるためのタスクリストを整理したものです。

Prefab、画像素材、ボタン、Hierarchy配置は開発者がUnity上で作成します。
Codexには主に以下を依頼します。

- C#スクリプト作成
- クラス設計
- Inspector設定案
- Hierarchy構成案
- 実装手順の提示
- 不具合修正
- リファクタリング

## 現在の進捗 (自動記録)

- **コアスクリプト**: `GameManager.cs`, `GridManager.cs`, `PlayerController.cs`, `BoxController.cs`, `EnemyController.cs`, `StageGenerator.cs`, `StageData.cs`, `Solver.cs`, `SaveManager.cs`, `AdManager.cs` (stub), `RankingManager.cs` (stub), `UIController.cs` を作成しました（Assets/Scripts に配置）。
- **シーン**: `Game.unity` はエディタでの配置を想定しています。シーンファイルは手動で配置してください（手順はこのファイル下部に記載）。
- **注意**: スクリプトは最小実装で、Prefab（Player/Wall/Box/Goal/Enemy）とUIはユーザーがUnity Editor上で作成・配置する必要があります。


---

## 1. 開発全体フェーズ

| Phase | 内容 | 優先度 |
|---|---|---|
| Phase 1 | 最小プレイ実装 | 最高 |
| Phase 2 | Box実装 | 最高 |
| Phase 3 | Enemy実装 | 高 |
| Phase 4 | ステージ生成 | 高 |
| Phase 5 | 星評価・セーブ | 高 |
| Phase 6 | UI整備 | 中 |
| Phase 7 | 広告実装 | 中 |
| Phase 8 | ランキング実装 | 低〜中 |
| Phase 9 | 調整・デバッグ | 最高 |

---

# Phase 1：最小プレイ実装

## 目的
3×3ステージで、Playerが上下左右に滑り、Goalに到達したらクリアできる状態を作る。

## タスク

- [ ] Unityプロジェクト作成
- [ ] GameScene作成
- [ ] 3×3グリッド表示
- [ ] PlayerPrefab作成
- [ ] WallPrefab作成
- [ ] GoalPrefab作成
- [ ] GameManager.cs作成
- [ ] GridManager.cs作成
- [ ] PlayerController.cs作成
- [ ] 上下左右ボタン作成
- [ ] 入力方向にPlayerを滑らせる処理作成
- [ ] Wallまたはステージ端で停止する処理作成
- [ ] Goal到達判定作成
- [ ] ClearPanel表示

## 完了条件

- [ ] Playerが上下左右に滑る
- [ ] Wallで止まる
- [ ] ステージ外に出ない
- [ ] Goalに到達するとクリアになる
- [ ] 3×3ステージを1つ手動で遊べる

## Codex依頼例

```text
SPEC.mdを読んでください。
Phase 1として、3×3グリッドでPlayerが上下左右に滑り、Wallまたはステージ端で停止し、Goalに到達したらClearPanelを表示する最小実装を作ってください。
Prefabとボタンは私がUnity上で配置します。
必要なC#スクリプト、Inspector設定、Hierarchy構成案を提示してください。
```

---

# Phase 2：Box実装

## 目的
BoxをPlayerと同じように滑らせ、壁・停止位置調整・道作りに使えるようにする。

## タスク

- [ ] BoxPrefab作成
- [ ] BoxController.cs作成
- [ ] Boxのグリッド座標管理
- [ ] Player入力時にBoxも同方向へ移動
- [ ] BoxがWall・Stage外・他Boxで停止する処理
- [ ] PlayerがBoxを通過できない処理
- [ ] BoxをWall代替として判定する処理
- [ ] 複数Box対応

## 完了条件

- [ ] BoxがPlayerと同じ方向へ滑る
- [ ] BoxがWallや端で止まる
- [ ] PlayerがBoxをすり抜けない
- [ ] Boxを使ってPlayerの停止位置を調整できる

## Codex依頼例

```text
Phase 2としてBoxを実装してください。
BoxはPlayerと同じように重力方向へ滑ります。
PlayerはBoxを通過できず、BoxはWallやステージ外、他Boxで停止します。
既存のGridManagerとPlayerControllerにどう組み込むべきか提案し、必要なスクリプトを作成してください。
```

---

# Phase 3：Enemy実装

## 目的
敵1・敵2を実装し、Player移動後にEnemyが1マス移動する仕組みを作る。

## タスク

- [ ] EnemyPrefab作成
- [ ] EnemyController.cs作成
- [ ] EnemyType enum作成
- [ ] Enemy1：上下左右の指定1方向へ1マス移動
- [ ] Enemy2：斜め4方向の指定1方向へ1マス移動
- [ ] Player移動後にEnemy移動
- [ ] Enemyは重力で滑らない
- [ ] EnemyがWall・Box・Stage外・他Enemyにぶつかる場合は移動しない
- [ ] Playerが移動してEnemyに接触した場合のみ失敗
- [ ] Enemy側からPlayerに突っ込んでも失敗にしない

## 完了条件

- [ ] Player移動後にEnemyが動く
- [ ] Enemyは指定方向に1マスだけ動く
- [ ] PlayerがEnemyへ接触した場合だけ失敗する
- [ ] EnemyがPlayerへ移動してもゲームオーバーにならない

## Codex依頼例

```text
Phase 3としてEnemyを実装してください。
Enemyは重力では動かず、Player移動後に1マスだけ移動します。
Enemy1は上下左右の指定方向、Enemy2は斜めの指定方向に動きます。
失敗判定はPlayerが移動してEnemyに接触した場合のみです。
EnemyからPlayerに突っ込んでも失敗にしないようにしてください。
```

---

# Phase 4：ステージ生成

## 目的
3×3、4×4、5×5、ランダムステージを生成できるようにする。

## タスク

- [ ] StageData.cs作成
- [ ] StageGenerator.cs作成
- [ ] サイズ別生成処理
- [ ] Player位置生成
- [ ] Goal位置生成
- [ ] Wall生成
- [ ] Box生成
- [ ] Enemy生成
- [ ] Wall最大数制限
- [ ] Box数制限
- [ ] Enemy数制限
- [ ] 角の斜めWall禁止ルール実装
- [ ] 到達可能チェック実装
- [ ] クリア不能なら再生成
- [ ] Seed生成対応

## サイズ別制約

| サイズ | Wall最大数 | Box数 | Enemy数 |
|---|---:|---:|---:|
| 3×3 | 2 | 1〜2 | 0 |
| 4×4 | 4 | 3〜4 | 0〜1 |
| 5×5 | 6 | 4〜6 | 1〜2 |

## 完了条件

- [ ] サイズ別にランダム生成できる
- [ ] Wall数制限が守られる
- [ ] Box数制限が守られる
- [ ] Enemy数制限が守られる
- [ ] 角の斜めWall禁止ルールが守られる
- [ ] クリア不能ステージが再生成される

## Codex依頼例

```text
Phase 4としてStageGeneratorを実装してください。
SPEC.mdの制約に従って、3×3、4×4、5×5のランダムステージを生成してください。
Wall最大数、Box数、Enemy数、角の斜めWall禁止ルールを守ってください。
生成後はPlayerからGoalへ到達可能かチェックし、不可なら再生成してください。
```

---

# Phase 5：Solver・星評価・セーブ

## 目的
最短手数判定、星評価、進行状況保存を実装する。

## タスク

- [ ] Solver.cs作成
- [ ] BFSでGoal到達可能チェック
- [ ] 最短手数算出
- [ ] 手数カウント
- [ ] 星評価計算
- [ ] ResultPanel表示
- [ ] SaveManager.cs作成
- [ ] PlayerPrefs保存
- [ ] ステージ別星数保存
- [ ] 累積星数保存
- [ ] デイリー挑戦回数保存

## 星評価

- ★★★ = 最短手数以内
- ★★ = 最短手数 × 1.5以内
- ★ = クリア

## 完了条件

- [ ] クリア時に手数が表示される
- [ ] 星評価が表示される
- [ ] 獲得星数が保存される
- [ ] 累積星数が保存される
- [ ] アプリ再起動後も進行状況が残る

## Codex依頼例

```text
Phase 5としてSolver、星評価、SaveManagerを実装してください。
SolverはBFSで最短手数を計算し、クリア時の手数と比較して星評価を出してください。
ステージごとの星数と累積星数はPlayerPrefsに保存してください。
```

---

# Phase 6：UI整備

## 目的
ユーザーが遊びやすい画面構成を作る。

## タスク

- [ ] TitleScene作成
- [ ] StageSelectPanel作成
- [ ] GameScene UI作成
- [ ] 上下左右ボタン作成
- [ ] 手数表示
- [ ] リトライボタン
- [ ] スキップボタン
- [ ] 新規生成ボタン
- [ ] ResultPanel作成
- [ ] SettingsPanel作成
- [ ] BGM/SE設定
- [ ] デイリーステージ入口
- [ ] ランキングボタン

## 完了条件

- [ ] タイトルからゲーム開始できる
- [ ] ステージ選択できる
- [ ] プレイ中に手数が見える
- [ ] リトライできる
- [ ] クリア後に次のステージへ進める
- [ ] タイトルへ戻れる

## Codex依頼例

```text
Phase 6としてUI構成案を作成してください。
TitleScene、GameScene、ResultPanel、StageSelectPanelのHierarchy構成案と、各ButtonのOnClickに接続するメソッド名を提案してください。
UIのPrefabや配置は私がUnity上で行います。
```

---

# Phase 7：広告実装

## 目的
リワード広告によるスキップ、新規生成、追加3ステージを実装する。

## タスク

- [ ] AdManager.cs作成
- [ ] リワード広告SDK導入
- [ ] ステージスキップ
- [ ] 新しいランダム面生成
- [ ] 追加3ステージ解放
- [ ] 広告失敗時の処理
- [ ] 広告視聴完了時のコールバック処理

## 完了条件

- [ ] リワード広告を表示できる
- [ ] 視聴完了後にスキップできる
- [ ] 視聴完了後に新しいステージを生成できる
- [ ] 視聴完了後に追加3ステージが遊べる

## Codex依頼例

```text
Phase 7としてAdManagerの設計をしてください。
リワード広告で、ステージスキップ、新規ランダム生成、追加3ステージ解放を行います。
広告SDKの初期化、視聴完了コールバック、失敗時処理を分けた設計にしてください。
```

---

# Phase 8：ランキング実装

## 目的
Game Center / Google Play Gamesで累積星数ランキングを実装する。

## タスク

- [ ] RankingManager.cs作成
- [ ] iOS Game Center連携調査
- [ ] Android Google Play Games連携調査
- [ ] 累積星数送信
- [ ] ランキング表示
- [ ] ログイン失敗時の処理
- [ ] ランキング非対応時の処理

## 優先順位
Ver1.0では累積星数ランキングを優先する。
デイリーランキングは後日追加でもよい。

## 完了条件

- [ ] 累積星数をランキングへ送信できる
- [ ] ランキング画面を開ける
- [ ] iOS/Androidで分岐できる

## Codex依頼例

```text
Phase 8としてRankingManagerの設計をしてください。
Firebaseではなく、iOSはGame Center、AndroidはGoogle Play Gamesを想定します。
まずは累積星数ランキングを送信・表示できる構成を提案してください。
```

---

# Phase 9：調整・デバッグ

## 目的
ゲームとして遊べる品質に仕上げる。

## タスク

- [ ] Player移動のバグ確認
- [ ] Box移動順の確認
- [ ] Enemy接触判定確認
- [ ] クリア不能ステージ生成確認
- [ ] Solverの無限ループ対策
- [ ] ランダム生成の偏り調整
- [ ] 星評価の難易度調整
- [ ] UI表示崩れ確認
- [ ] 広告表示後の復帰確認
- [ ] セーブデータ破損対策
- [ ] iOS/Androidビルド確認

## 完了条件

- [ ] 主要バグがない
- [ ] 3×3、4×4、5×5が正常に遊べる
- [ ] リトライ・クリア・保存が正常
- [ ] 広告後にゲームが壊れない
- [ ] ランキングなしでもゲーム進行できる

---

# 推奨実装順序

1. Phase 1：Player移動
2. Phase 2：Box
3. Phase 3：Enemy
4. Phase 4：StageGenerator
5. Phase 5：Solver・星評価・セーブ
6. Phase 6：UI
7. Phase 7：広告
8. Phase 8：ランキング
9. Phase 9：デバッグ

---

# 最初にCodexへ渡すプロンプト

```text
SPEC.mdが仕様書です。
Unityでこのゲームを完成させるための開発計画書PLAN.mdを確認してください。

まずPhase 1から進めます。
3×3グリッド上でPlayerが上下左右に滑り、Wallまたはステージ端で停止し、Goalに到達したらClearPanelを表示する最小実装を作ってください。

Prefab、画像、ボタン、Hierarchy配置は私がUnity上で行います。
必要なC#スクリプト、Inspector設定、Hierarchy構成案、作業手順を提示してください。
```

---

# 注意事項

## Codexに一度に全部作らせない
一気に全機能を依頼するとバグが増えやすい。
必ずPhaseごとに進める。

## 各Phaseで動作確認する
次のPhaseへ進む前に、必ずUnity上で再生して動作確認する。

## 仕様変更はSPEC.mdに反映する
開発中にルールを変えた場合、必ずSPEC.mdも更新する。

## PLAN.mdは進捗管理に使う
完了したタスクはチェックを入れて管理する。

---

以上。
