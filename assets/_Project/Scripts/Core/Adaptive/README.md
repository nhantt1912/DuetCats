# Adaptive View cho Playable Ads (Tiếng Việt)

Tài liệu này mô tả hệ thống detect xoay màn hình (Portrait/Landscape) đã tạo trong `assets/_Project/Scripts/Core/Adaptive/`.

Mục tiêu: hỗ trợ xoay ngang cho playable ads nhưng **không sửa flow gameplay cũ** trong `GameplayController` và các class core hiện tại.

## 1) Các class và trách nhiệm

### `AdaptiveCameraController`
- **File:** `assets/_Project/Scripts/Core/Adaptive/AdaptiveCameraController.cs`
- **Handle gì:**
  - Detect mode `Portrait`/`Landscape` dựa trên tỉ lệ `Camera.pixelRect`.
  - Phát sự kiện `OnViewModeChanged` khi mode đổi.
  - Cung cấp debug state qua `debugModeLabel`.
- **Ai dùng:** `AdaptiveViewOrchestrator` subscribe sự kiện để apply thay đổi.

### `AdaptiveViewProfile`
- **File:** `assets/_Project/Scripts/Core/Adaptive/AdaptiveViewProfile.cs`
- **Handle gì:**
  - Chứa thông số cấu hình theo mode (PT/LS): camera, tile layout, cat bounds.
  - Là nguồn dữ liệu để orchestrator apply đồng bộ.
- **Ai dùng:** `AdaptiveViewOrchestrator` đọc profile khi mode đổi.

### `AdaptiveViewOrchestrator`
- **File:** `assets/_Project/Scripts/Core/Adaptive/AdaptiveViewOrchestrator.cs`
- **Handle gì:**
  - Nhận mode mới và điều phối toàn bộ cập nhật.
  - Bật/tắt `Background PT` / `Background LS`.
  - Set thông số camera theo profile.
  - Scale background để cover toàn viewport (không hở viền).
  - Gọi adapters cập nhật Tile/Cat/Input.
- **Ai bị ảnh hưởng:** camera, background, layout gameplay, input mapping.

### `AdaptiveRuntimeBootstrap`
- **File:** `assets/_Project/Scripts/Core/Adaptive/AdaptiveRuntimeBootstrap.cs`
- **Handle gì:**
  - Tự động thêm/wire adaptive components lúc runtime (`AfterSceneLoad`).
  - Tìm object theo scene hiện tại, không bắt buộc bạn phải drag reference thủ công.
- **Ai dùng:** chạy tự động khi scene load.

### `TileLayoutAdapter`
- **File:** `assets/_Project/Scripts/Core/Adaptive/Adapters/TileLayoutAdapter.cs`
- **Handle gì:** cập nhật runtime các thông số lane/spawn/hit/despawn của `TileManager`.
- **Lưu ý:** dùng reflection để không sửa class gameplay cũ.

### `CatBoundsAdapter`
- **File:** `assets/_Project/Scripts/Core/Adaptive/Adapters/CatBoundsAdapter.cs`
- **Handle gì:** cập nhật runtime `limitLeft/limitRight` của `CatController`.
- **Lưu ý:** dùng reflection để tránh thay đổi logic sẵn có.

### `InputViewportAdapter`
- **File:** `assets/_Project/Scripts/Core/Adaptive/Adapters/InputViewportAdapter.cs`
- **Handle gì:** cập nhật `_midX` / `_halfWidth` của `DualInputHandler` khi viewport đổi.
- **Ý nghĩa:** giữ chia vùng chạm trái/phải đúng sau khi xoay.

## 2) Nằm ở đâu trong scene `GamePlay`

Hệ thống được bootstrap và gắn runtime theo các điểm sau:

- `Main Camera`:
  - `AdaptiveCameraController`
  - `AdaptiveViewOrchestrator`
- `TileManager`:
  - `TileLayoutAdapter`
- `LeftCat` và `RightCat` (con của `CatManager`):
  - `CatBoundsAdapter`
- `InputHandler` (object có `DualInputHandler`):
  - `InputViewportAdapter`
- Background được tìm theo tên trong scene:
  - `Background PT`
  - `Background LS`

## 3) Vì sao có các thông số này

### Nhóm detect mode (`AdaptiveCameraController`)
- `checkInterval`:
  - **Vì sao có:** tránh check mỗi frame để giảm chi phí.
  - **Dùng để làm gì:** tần suất re-check aspect.
  - **Sửa sẽ đổi gì:**
    - Giảm giá trị -> phản hồi xoay nhanh hơn, tốn CPU hơn một chút.
    - Tăng giá trị -> phản hồi chậm hơn, nhẹ hơn.
- `landscapeThreshold` + `portraitThreshold`:
  - **Vì sao có:** tạo hysteresis để tránh nhấp nháy mode quanh tỉ lệ ~1.0.
  - **Dùng để làm gì:** quyết định ngưỡng chuyển PT -> LS và LS -> PT.
  - **Sửa sẽ đổi gì:**
    - Ngưỡng gần nhau -> dễ đổi mode nhanh nhưng có nguy cơ flicker.
    - Ngưỡng xa nhau -> ổn định hơn nhưng cần đổi tỉ lệ nhiều hơn mới chuyển mode.
- `debugMode`, `logModeChanges`, `debugModeLabel`:
  - **Vì sao có:** phục vụ QA/test playable ads nhanh.
  - **Dùng để làm gì:** hiển thị/log trạng thái hiện tại.
  - **Ảnh hưởng:** chỉ ảnh hưởng quan sát/debug, không đổi gameplay logic.

### Nhóm apply mode (`AdaptiveViewOrchestrator`)
- `autoFitBackgroundToViewport`:
  - **Vì sao có:** đảm bảo background luôn full màn hình.
  - **Dùng để làm gì:** bật/tắt auto scale background theo viewport camera.
  - **Sửa sẽ đổi gì:**
    - Tắt -> background giữ scale gốc, có thể hở viền ở LS/PT.
- `backgroundOverscan`:
  - **Vì sao có:** bù sai số pixel/rounding gây lộ mép.
  - **Dùng để làm gì:** scale dư nhẹ để cover chắc chắn.
  - **Sửa sẽ đổi gì:**
    - Tăng -> ít nguy cơ hở viền, nhưng crop background nhiều hơn.
    - Giảm -> thấy nhiều ảnh hơn, nhưng có nguy cơ lộ mép.

### Nhóm dữ liệu PT/LS (`AdaptiveViewProfile`)
- `portraitCamera` / `landscapeCamera`:
  - **Dùng để:** set `orthographicSize` và `localPosition` camera theo mode.
  - **Ảnh hưởng:** vùng nhìn thấy game và cảm giác zoom.
- `portraitTiles` / `landscapeTiles`:
  - **Dùng để:** set `laneLeftX`, `laneRightX`, `spawnY`, `hitY`, `despawnY`.
  - **Ảnh hưởng:** vị trí lane, nhịp rơi tile, điểm chạm và vùng thua.
- `portraitLeftCat` / `portraitRightCat` / `landscapeLeftCat` / `landscapeRightCat`:
  - **Dùng để:** giới hạn vùng di chuyển mèo mỗi mode.
  - **Ảnh hưởng:** khả năng chạm đúng lane sau khi xoay.

## 4) Sửa thông số nào thì ảnh hưởng tới ai

- Sửa thông số detect trong `AdaptiveCameraController`:
  - Ảnh hưởng QA cảm nhận chuyển mode và ổn định mode.
  - Không đổi trực tiếp score/rule gameplay.
- Sửa camera trong `AdaptiveViewProfile`:
  - Ảnh hưởng toàn bộ vùng hiển thị, background fit, vị trí tương đối object.
  - Người bị ảnh hưởng: player (trải nghiệm nhìn), QA (kết quả framing).
- Sửa tile layout trong `AdaptiveViewProfile`:
  - Ảnh hưởng `TileManager`, hit window thực tế, cảm giác khó/dễ.
  - Người bị ảnh hưởng: player và cân bằng gameplay.
- Sửa cat bounds trong `AdaptiveViewProfile`:
  - Ảnh hưởng `CatController`, khả năng bắt tile ở hai lane.
  - Người bị ảnh hưởng: player (điều khiển), QA (test trúng lane).
- Sửa `backgroundOverscan`:
  - Ảnh hưởng visual background (crop/hở mép).
  - Người bị ảnh hưởng: artist/QA visual.

## 5) Checklist test nhanh sau khi chỉnh thông số

1. Mở `assets/_Project/Scenes/GamePlay.unity`.
2. Vào Play Mode, đổi tỉ lệ Game View qua lại 9:16 và 16:9.
3. Xác nhận:
   - Background đổi đúng PT/LS và không hở viền.
   - Mèo vẫn điều khiển đúng nửa trái/phải.
   - Lane tile khớp vị trí mèo ở cả PT và LS.
   - Không có rung mode liên tục gần tỉ lệ vuông.

## 6) Ghi chú bảo trì

- Adapters dùng reflection để không đụng code gameplay cũ.
- Nếu đổi tên private field trong `TileManager` / `CatController` / `DualInputHandler`, cần update tên field tương ứng trong adapters.


