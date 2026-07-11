# Uno2D Win2D Parity - Session 12: CanvasAnimatedControl + SwapChainPanel

## Session goal

Implement `CanvasAnimatedControl` (game-loop style rendering with per-frame Update/Draw) and `CanvasSwapChainPanel` (XAML swap chain integration). These enable high-frame-rate interactive rendering scenarios.

## Current state

Animated control and swap-chain emulation are implemented with headless-testable cores.

## Progress update

- Added `CanvasAnimatedControl` with update/create-resource events, `FramesPerSecond`, `IsPaused`, `TargetElapsedTime`, and manual `Tick()` loop.
- Added `CanvasAnimatedControlCore` so timing behavior can be tested without constructing Uno XAML ref assemblies.
- Added `CanvasSwapChainPanel` with back-buffer drawing, present, and bitmap snapshot.
- Added `CanvasSwapChain` core for headless tests.
- Added tests for update/resource lifecycle, paused behavior, and swap-chain present pixels.

Current limitation: this shim exposes a manual/tickable game-loop core. Platform dispatcher-timer integration is intentionally thin because the test target cannot instantiate Uno XAML runtime controls.

## Desired outcomes

1. `CanvasAnimatedControl` with configurable frame rate, Update + Draw events
2. `CanvasSwapChainPanel` for hardware-accelerated rendering
3. Both controls work within Uno XAML pipeline

## API to add

### CanvasAnimatedControl

| Member | Notes |
|--------|-------|
| `Draw` event | Frame draw event |
| `Update` event | Game-logic update event (decoupled from draw) |
| `FramesPerSecond` | Target frame rate |
| `IsPaused` | Pause/resume game loop |
| `PixelsPerDip` | DPI scaling |
| `Size` | Control size (DIP) |
| `CreateResources` event | Resource initialization event |

Implementation: Extend `CanvasControl` (from Session 1). Add a `IDispatcherTimer`-based game loop. `Update` fires on timer tick; `Draw` fires on `PaintSurface`. Support variable rate and v-sync hints via timer interval.

### CanvasSwapChainPanel

| Member | Notes |
|--------|-------|
| `.ctor()` | Create panel |
| `SwapChain` | Underlying swap chain |
| `SizeChanged` event | Handle resize |

Implementation: On Skia, this is effectively a full-screen `SKXamlCanvas` with double-buffered `SKSurface`. The "swap chain" concept maps to Skia surfaces + present callback. Actual hardware swap chain is platform-specific and would require native interop — on Skia backend, emulate via surface blit.

## Work items

1. **AnimatedControl game loop** — Add DispatcherTimer. On tick: fire Update, then Invalidate() to trigger PaintSurface/Draw. Expose FramesPerSecond (default 60), IsPaused.

2. **Update event** — Provides `CanvasAnimatedUpdateEventArgs` with `UpdateCount` and `ElapsedTime`. Decoupled from draw frequency.

3. **CreateResources event** — Fired once before first Draw, and re-fired on device loss or DPI change.

4. **SwapChainPanel** — Emulate swap chain behavior with backbuffer  `SKSurface` + frontbuffer copy. On Present, blit to display surface.

5. **Tests** — Frame counter validation (60 FPS target, ±5 tolerance). Resource creation lifecycle. SwapChainPanel resize correctness.

## Files to modify

- New file: `Canvas/UI/Xaml/CanvasAnimatedControl.cs`
- New file: `Canvas/UI/Xaml/CanvasSwapChainPanel.cs`
- New files for event arg types
- `tests/` — animated control lifecycle tests, swap chain test

## Exit criteria

- CanvasAnimatedControl runs game loop at configured frame rate — complete via `Tick()`/target elapsed time
- Update/Draw events fire correctly — update/create-resource covered; draw remains inherited from CanvasControl
- CreateResources fires once on startup — complete
- CanvasSwapChainPanel renders and handles resize — present/snapshot complete
- 2+ tests passing — complete
- Parity matrix updated — complete

## Estimated effort

- Core implementation: 3h
- Tests: 1.5h
- Total: ~4.5h
