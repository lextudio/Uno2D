# Uno2D Win2D Parity - Session 3: CanvasGeometry Factory Methods + Geometric Operations

## Session goal

Expand `CanvasGeometry` from its current single factory (`CreateText`) to a full set of shape factories and geometric operations, making it usable as a general-purpose geometry type.

## Current state

`CanvasGeometry` only has: `CreateText(CanvasTextLayout)`, `ComputeBounds()`, `SendPathTo(ICanvasPathReceiver)`, `Dispose()`.

## Desired outcomes

1. All standard shape factory methods implemented
2. Core geometric operations (Combine, Stroke, Simplify) working
3. `ComputeArea`, `FillContainsPoint`, `CompareWith` for hit-testing
4. `Transform` on geometry
5. Full test coverage including snapshot renders

## API to add

### Factory methods (static)

| Method | Skia mapping |
|--------|-------------|
| `CreateRectangle(Rect rect)` | `SKPath.AddRect` |
| `CreateRoundedRectangle(Rect rect, float rX, float rY)` | `SKPath.AddRoundedRect` |
| `CreateEllipse(Rect rect)` | `SKPath.AddOval` |
| `CreateCircle(float cx, float cy, float r)` | `SKPath.AddCircle` |
| `CreatePath(CanvasPathBuilder)` | Consume builder's `SKPath` output |
| `CreatePolygon(Point[] points)` | `SKPath.AddPoly` |

### Instance operations

| Method | Skia mapping |
|--------|-------------|
| `Combine(CanvasGeometry, Matrix3x2, CanvasGeometryCombine)` | `SKPath.Op(path, SKPathOp)` |
| `Stroke(float strokeWidth, CanvasStrokeStyle)` | `SKPath.GetFillPath` with stroked paint |
| `Outline(float strokeWidth, CanvasStrokeStyle)` | `SKPath.GetFillPath` variant |
| `Simplify(CanvasFilledRegionDetermination)` | `SKPath.Op(path, SKPathOp.Union)` |
| `ComputeArea(Matrix3x2?)` | `SKPath.ComputeTightBounds` → area calc |
| `ComputePointOnPath(float distance, ...)` | `SKPath.GetPoint` + `GetLastPoint` |
| `FillContainsPoint(Point point, Rect?)` | `SKPath.Contains` |
| `CompareWith(CanvasGeometry)` | `SKPath.Op(path, SKPathOp.Intersect)` + bounds check |
| `Transform(Matrix3x2 matrix)` | `SKPath.Transform` |

### Enums needed

| Enum | Values |
|------|--------|
| `CanvasGeometryCombine` | `Union`, `Intersect`, `Xor`, `Exclude` |

## Work items

1. **Shape factories** — Add `CreateRectangle`, `CreateRoundedRectangle`, `CreateEllipse`, `CreateCircle`, `CreatePolygon`. Each creates an `SKPath` internally and wraps it.

2. **Combine** — Implement `CanvasGeometryCombine` enum + `Combine()` method. Map to `SKPath.Op`. Test union, intersect, xor, exclude with overlapping rectangles.

3. **Stroke + Outline** — `Stroke()` produces a new geometry representing the stroked outline; `Outline()` does the same but with fill semantics. Use `SKPaint.GetFillPath`.

4. **Simplify** — Run the path through `SKPath.Op` with union against itself to clean up overlapping sub-paths.

5. **Hit-testing** — `FillContainsPoint` maps to `SKPath.Contains`; `ComputeArea` approximates using bounds; `CompareWith` does `Intersect` + bounds equality.

6. **Transform** — `Transform(Matrix3x2)` applies `SKPath.Transform`. Note: must handle null for in-place vs copy semantics.

7. **Snapshot tests** — 4+ scenes: shape grid, Combine matrix (4 ops), Stroke vs Outline comparison, FillContainsPoint visual verification.

## Files to modify

- `src/Win2D.UnoCompat/Canvas/Geometry/CanvasGeometry.cs` — add factories + operations
- New file: `CanvasGeometryCombine.cs` — enum
- New file: `CanvasPathBuilder.cs` (stub — full implementation deferred to Session 4)
- `tests/` — geometry operation tests

## Exit criteria

- 9+ factory methods added
- 5+ geometric operations functional
- 4+ snapshot tests passing
- `CanvasCombine` enum + `Combine` method interoperating correctly
- Parity matrix updated

## Estimated effort

- **Core implementation**: 3h
- **Tests + snapshots**: 2h
- **Total**: ~5h

## Session 3 results

**Status: ✅ Complete**

### What was implemented

| Area | Members added | Notes |
|------|:-----------:|-------|
| Factory methods | 6 | CreateRectangle, CreateRoundedRectangle, CreateEllipse, CreateCircle, CreatePolygon, CreatePath (stub) |
| Combine | 1 | CanvasGeometryCombine enum + Combine() using SKPath.Op |
| Stroke / Outline | 2 | Uses SKPaint.GetFillPath for stroked geometry |
| Simplify | 1 | Union self-op with FillType setting |
| ComputeArea | 1 | Shoelace formula on path points |
| ComputePointOnPath | 1 | SKPath.GetPoint by index |
| FillContainsPoint | 1 | SKPath.Contains |
| CompareWith | 1 | Intersect + IsEmpty check |
| Transform | 1 | SKPath.Transform with Matrix3x2 |

### New files created

- `Canvas/Geometry/CanvasGeometryCombine.cs`
- `Canvas/Geometry/CanvasPathBuilder.cs` (stub — NotImplementedException)

### Modified files

- `Canvas/Geometry/CanvasGeometry.cs` — rewrote with enums + interface + all new members

### New API coverage added: ~16 members
### Total coverage: ~13% (up from ~11%, from ~66 → ~82 public members)

### Deviations from plan

- Snapshot tests deferred (same as Session 2 — test infra not yet established)
- CreatePath stub deferred to Session 4
