# Uno2D Win2D Parity - Session 1 Plan

## Session goal

Establish a repeatable parity workflow and land the first set of high-confidence P0 validations.

## Desired outcomes

1. A baseline parity matrix is reviewed and agreed on.
2. First P0 semantic tests are added and passing.
3. First render snapshot test pipeline is runnable locally (and ideally in CI).
4. Known gaps are converted into prioritized backlog items with owners.

## Timebox (suggested: 2.5 to 3 hours)

1. 20 min: Kickoff and scope lock
2. 35 min: Parity matrix triage (P0 only)
3. 60 min: Implement first semantic tests
4. 35 min: Add first render snapshot tests
5. 20 min: Review results, assign backlog, define Session 2 targets

## Scope for Session 1 (P0 only)

- `CanvasDevice.GetSharedDevice()`
- `CanvasRenderTarget` basic lifecycle (`CreateDrawingSession`, `GetPixelBytes`, `SaveAsync`)
- `CanvasDrawingSession` primitives:
  - `Clear`
  - `DrawLine`
  - `DrawRectangle` / `FillRectangle`
  - `DrawCircle` / `FillCircle`
- `CanvasStrokeStyle` basic cap/join/dash mapping validation
- `CanvasControl` DIP scaling sanity checks

## Work items

1. Confirm matrix baseline in `docs/design.md`
- Mark each in-scope P0 row as `Supported` or `Partial` based on current behavior.
- Add one sentence of known difference for each `Partial` row.

2. Add semantic unit tests
- `CanvasDevice`: shared instance behavior and disposal safety expectations.
- `CanvasRenderTarget`: drawing session creation, non-empty pixel buffer after draw, save-to-stream success.
- `CanvasDrawingSession`: deterministic assertions for primitive calls (no exceptions, expected bounds/pixels where practical).
- `CanvasStrokeStyle`: verify cap/join/dash mappings produce expected Skia paint configuration behavior.

3. Add first render snapshot tests
- Create 2-3 deterministic scenes:
  - Primitive shapes with varying stroke widths.
  - Dash/cap/join sample grid.
  - Text baseline/alignment smoke case (if deterministic fonts available).
- Define tolerance policy for anti-aliasing differences.

4. Wire tests for repeatability
- Ensure local command to run semantic + render tests is documented.
- If CI update is feasible, add a basic job that runs tests and uploads snapshot artifacts on failure.

5. Record outputs
- Update parity matrix statuses and coverage fields.
- Add short "Session 1 results" note at bottom of this document.

## Risks and mitigations

- Font/render nondeterminism across platforms.
  - Mitigation: start with geometry/shape-heavy snapshots; keep text assertions lightweight.
- Anti-aliasing differences causing flaky snapshots.
  - Mitigation: use tolerance thresholds and stable canvas sizes/colors.
- Scope creep into P1/P2 APIs.
  - Mitigation: strict P0-only session boundary.

## Exit criteria

- At least 5 P0 semantic tests merged/passing.
- At least 2 render snapshots validated and stable.
- `docs/design.md` parity matrix updated with real statuses.
- Session 2 backlog created with top 3 next gaps.

## Suggested Session 2 targets

1. `CanvasTextLayout` parity expansion (wrapping, trimming, metrics)
2. `CanvasGeometry` constructor coverage (`CreateRectangle`, etc.)
3. `CanvasPathBuilder` initial implementation spike
