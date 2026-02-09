---
description: StylusCore Technical Constitution — 04_INPUT_INK_THREADING
---

# 🖊️ 04 — INPUT, INK & THREADING  
(StylusPlugIn · Wet/Dry · ~16ms Pump)

This document defines **strict, engine-grade rules** for StylusCore’s low-latency stylus input on WPF /.NET 10.

**Goal:** An Infinite Canvas (world coordinates) with Deep Zoom (0.1–50x), supporting high sample rates (Surface/Wacom) without jitter, UI freezes, or threading bugs.

This document is written to be directly consumable by both AI agents and humans.

---

## 0) Normative Language
- **MUST**: required
- **MUST NOT / FORBIDDEN**: prohibited
- **SHOULD**: recommended
- **MAY / OPTIONAL**: allowed but not required

---

## 1) RED LINES (Non-Negotiable)

### 1.1 StylusPlugIn Mandate
- **MUST:** Use a custom `StylusPlugIn` for low-latency stylus capture.
- **FORBIDDEN:** Using WPF `InkCanvas` / `InkPresenter` as the editor surface.
- **MUST:** Use the custom engine surface (e.g., `CanvasHostControl`) in the **Engine (WPF bridge)** layer.

---

### 1.2 Pen Thread: UI Access Is Forbidden
- **MUST:** `StylusPlugIn` callbacks (`OnStylusDown/Move/Up`) run on the **PEN THREAD**.
- **FORBIDDEN on the pen thread:**
  - Reading/writing any `DependencyObject` / `DependencyProperty`
  - Accessing or creating any WPF objects:
    `Brush`, `Color`, `Geometry`, `Transform`, `DrawingVisual`, any `Visual`
  - Adding/removing visuals or touching the visual tree
  - Blocking calls to the UI thread (`Dispatcher.Invoke`, synchronous waits)
  - Locks that can block (including contention-prone global locks)
- **MUST:** The pen thread does only minimal arithmetic and **pushes raw input into a thread-safe buffer**.

---

### 1.3 Wet/Dry Pipeline (Required)
- **MUST:** Use a **Wet** (preview) + **Dry** (retained) pipeline.
- **MUST:** Dry ink is retained as **`DrawingVisual`**.
- **FORBIDDEN:** Using `WriteableBitmap` as the primary dry ink layer (zoom blur).
- **FORBIDDEN:** Creating one WPF `Path` per stroke segment at scale (visual tree blow-up).

---

### 1.4 Pan/Zoom
- **MUST:** Pan/zoom via a camera transform (`MatrixTransform` or equivalent pipeline).
- **FORBIDDEN:** `ScrollViewer` on the editor surface.

---

## 2) Goals / Non-goals

### 2.1 Goals
- **MUST:** Pen input capture must not stall even when the UI thread is slow.
- **SHOULD:** Minimize tip-to-ink latency and reduce visible inking gap.
- **MUST:** Precision must remain stable at large world coordinates and at 50x zoom.

### 2.2 Non-goals (for now)
- **NOT PRIORITY:** Multi-pen / multi-user (assume a single active stylus).
- **NOT PRIORITY:** OS-level pen features (flicks, press-and-hold handwriting panel integrations).

---

## 3) WPF Input Model (What Runs Where)

### 3.1 StylusPlugIn Lifecycle
- A `StylusPlugIn` is attached via `UIElement.StylusPlugIns`.
- WPF invokes plugin callbacks through the RTS pipeline on the **pen thread**.
- **MUST:** Plugin output is raw, headless packets only:
  - `strokeId`, `kind` (Down / MoveBatch / Up / Cancel)
  - `(x, y)` in **world coordinates** or “world-ready” coordinates (see 6.4)
  - optional fields: `pressure`, `tiltX`, `tiltY`, `timestamp`, `buttons`, `isEraser`, `deviceId`

> **Rule:** If a coordinate conversion requires UI objects (camera/Visual), it MUST NOT happen on the pen thread.

---

### 3.2 Disable System Gestures (Consistency)
- **MUST:** Disable on the drawing surface:
  - press-and-hold (right-click emulation)
  - pen flicks
  - (optional) system handwriting gestures
- **Rationale:** They cause latency spikes and inconsistent interactions.

---

## 4) End-to-End Pipeline (Required Architecture)

### 4.1 Pen Thread: Capture → Buffer
- **MUST:** Convert incoming stylus input into **batched packets** and enqueue.
- **SHOULD:** Enqueue per RTS event (batch) rather than per point.

### 4.2 UI Thread: ~16ms Frame Pump → Wet Preview
- **MUST:** Consume buffered packets on the UI thread at ~16ms cadence.
- **DEFAULT:** `CompositionTarget.Rendering`.
- **MUST:** Each frame:
  1) Drain all available packets
  2) Update wet preview state
  3) Perform minimal invalidation and draw

### 4.3 Background: Post-Stroke Refinement
- **MUST:** On stylus up, heavy work runs on a background thread:
  - smoothing (Catmull–Rom → Bezier or equivalent)
  - simplification (Douglas–Peucker)
  - bounds computation
- **MUST NOT:** Background thread creates or touches WPF visuals.

### 4.4 UI Thread: Dry Commit
- **MUST:** Create/modify `DrawingVisual` only on the UI thread.
- **MUST:** Freeze all Freezables (`Pen`, `Brush`, `Geometry`, `Transform`) before use.
- **MUST:** Clear wet layer after commit.

---

## 5) Buffering Strategy (Default = SPSC Channel + Batches)

### 5.1 Default Buffer Primitive
- **MUST (Default):** Use `System.Threading.Channels` with **SingleWriter / SingleReader**.
- **DEFAULT:** Unbounded channel with guard rails.
- **MAY:** Use a ring buffer only if proven necessary (higher complexity).

### 5.2 Packet Format (Required)
- **MUST:** Buffer messages are `PointBatch` (not single points):
  - `strokeId`
  - `kind`: Down | MoveBatch | Up | Cancel
  - pooled array (`InkPoint[]`) + `count`
  - `timestampRange`
  - optional pressure/tilt/buttons flags

### 5.3 Backpressure (Never Block Pen Thread)
- **MUST:** Pen thread must never block waiting for UI.
- **MUST:** Use `TryWrite` (or lock-free write semantics).
- **MUST:** If the UI cannot keep up:
  - wet preview may degrade,
  - but **final stroke correctness must be preserved**.

**Recommended safe policy:**
1) Channel carries real-time preview batches.
2) Maintain an overflow collector for final accuracy:
   - if pressure exceeds a threshold or writes fail (bounded mode),
     append raw points to an overflow list (pooled).
3) On `StylusUp`, send a final packet containing all remaining points.

> Coalescing is allowed only for **wet preview**. Never permanently lose input.

---

## 6) Coordinate Systems & Camera Math

### 6.1 World vs Screen
- **MUST:** Store stroke data in **WORLD coordinates**.
- **FORBIDDEN:** Persisting screen coordinates or repeatedly transforming stored points.
- **MUST:** Pan/zoom is applied via camera transform at render time.

### 6.2 Precision Rule
- **MUST:** Use `double` for world coordinates in engine state and persisted/domain payloads.
- **MAY:** Convert to float for transient rendering geometry, but do not persist floats.

### 6.3 Pinch Zoom Pivot
- **MUST:** Pivot = gesture centroid.
- **MUST:** Clamp scale to `[0.1, 50.0]`.
- **SHOULD:** Use scale-at-pivot + translation compensation to keep the centroid stable.

### 6.4 Thread-Safe Camera Snapshot
- **MUST:** The pen thread must not read UI camera objects.
- **SHOULD:** Publish camera as an immutable snapshot (`CameraSnapshot` / `MatrixD`) that is safe to read without UI access.
- **MUST:** Any “screen → world” conversion used by the pen thread must use snapshot math only.

---

## 7) Input Arbitration & Palm Rejection

### 7.1 Arbitration Rules
- **MUST:** Stylus = ink / erase
- **MUST:** Touch = pan / zoom
- **MUST:** Mouse = selection / manipulation

### 7.2 Palm Rejection
- **MUST:** Suppress touch for **300ms** after the last stylus event.
- **SHOULD:** Implement as a small state machine that accounts for:
  - stylus in range
  - stylus down
  - stylus up
  - cancellation

### 7.3 Capture Discipline
- **SHOULD:** Capture input when a stroke starts so strokes don’t get cut off.
- **MUST:** Always release capture on up/cancel.

---

## 8) Stroke Data Representation (Memory Safe)

### 8.1 Active Stroke Storage
- **MUST:** Use pooled buffers (`ArrayPool<T>`) for high-rate point capture.
- **FORBIDDEN:** Retaining `StylusPointCollection` long-term.
- **SHOULD:** Compact point struct, e.g.:
  - `InkPoint { double x,y; float pressure; short tiltX,tiltY; uint t; }`

### 8.2 Pressure / Tilt
- **MAY:** Persist pressure if brush width uses it.
- **MAY:** Persist tilt if brushes require it.
- **SHOULD:** If unused, keep as an optional extension field rather than storing by default.

### 8.3 Eraser
- **SHOULD:** Detect eraser mode and route to erase tool.
- **DEFAULT:** Whole-stroke erase (fast).
- **MAY:** Partial erase (stroke splitting) is future scope.

---

## 9) Stroke Optimization & Rendering Quality

### 9.1 Simplification
- **MUST:** Apply Douglas–Peucker after stroke completion.
- **MUST:** Simplification epsilon MUST be configurable.  
  - **DEFAULT:** `epsilon = 0.5`
- **MUST:** Discard raw input arrays after finalization.

### 9.2 High-Zoom Curves
- **SHOULD:** For zoom > 2.0x, render smoother curves (visual-only):
  - centripetal Catmull–Rom → cubic Bezier
- **MUST:** Do not persist heavy geometry objects; persist compact point data.

### 9.3 Bounds
- **MUST:** Compute stroke AABB for:
  - dirty rect invalidation
  - QuadTree insertion
- **SHOULD:** Inflate bounds by stroke width and AA padding.

---

## 10) Threading & Safety Rules

### 10.1 Pen Thread Forbidden List (Final)
- **FORBIDDEN:** UI access, visual creation, synchronous UI calls, heavy allocations, blocking locks.

### 10.2 UI Thread Boundaries
- **MUST:** All `DrawingVisual` creation/modification occurs on UI thread.
- **MUST:** Freeze Freezables before use.
- **FORBIDDEN:** Creating visuals on background threads.

### 10.3 Shutdown / Cancellation
- **MUST:** On shutdown:
  - unsubscribe frame pump
  - stop reader loops
  - return pooled arrays
- **SHOULD:** If persistence occurs while drawing:
  - snapshot the active stroke safely without blocking input

---

## 11) Performance & Memory Guidance

### 11.1 GC / LOH Protection
- **MUST:** Avoid per-point allocations.
- **MUST:** Reuse buffers (`ArrayPool`, `MemoryPool`).
- **SHOULD:** Avoid LOH allocations (>85KB) by batching/segmenting.

### 11.2 No MVVM Bindings in Move Loop
- **FORBIDDEN:** Per-move ICommand/binding churn.
- **MUST:** Route input to an engine input manager, not ViewModel commands per point.

### 11.3 Telemetry (Recommended)
- **SHOULD:** Track:
  - points/sec
  - queue depth
  - preview drops
  - frame time
  - input-to-render latency

---

## 12) Testing & Validation

### 12.1 Latency
- **SHOULD:** Measure tip-to-ink latency.
- **MUST:** Log `timestamp(pen) → timestamp(render)`.

### 12.2 Stress
- **MUST:** Rapid scribble at high sample rates.
- **MUST:** Long continuous stroke (60s+).
- **MUST:** Fast zoom warp (0.1 ↔ 50).
- **MUST:** Palm rejection scenarios.

### 12.3 Gesture Correctness
- **MUST:** Centroid-pivot pinch zoom must not drift.

---

## 13) Anti-Patterns (Hard Stops)

- **FORBIDDEN:** Resolving `DynamicResource` in hot render loops (use cached resources).
- **FORBIDDEN:** Creating visuals on background threads.
- **FORBIDDEN:** Retaining `StylusPointCollection` indefinitely.
- **FORBIDDEN:** Relying only on OS palm rejection.
- **SHOULD:** Always handle capture; missing capture can cut strokes off.

---

## 14) Reference Pseudocode (High-Level)

### 14.1 Pen Thread (StylusPlugIn)
```csharp
// PEN THREAD
override void OnStylusDown(RawStylusInput input) {
  BeginStroke(++strokeId);
  EnqueueBatch(BatchKind.Down, input.GetStylusPoints());
}

override void OnStylusMove(RawStylusInput input) {
  EnqueueBatch(BatchKind.MoveBatch, input.GetStylusPoints());
}

override void OnStylusUp(RawStylusInput input) {
  EnqueueBatch(BatchKind.Up, input.GetStylusPoints());
  EndStroke(strokeId);
}

void EnqueueBatch(BatchKind kind, StylusPointCollection pts) {
  var batch = pool.RentBatch();
  batch.FillFrom(pts); // copy x,y,pressure,tilt,timestamp,flags

  if (!channel.Writer.TryWrite(batch)) {
    overflowCollector.Append(batch); // preserve final accuracy
  }
}

### 14.2 UI Thread (Frame Pump)
// UI THREAD (~16ms)
void OnRendering() {
  while (channel.Reader.TryRead(out var batch)) {
    wetLayer.Apply(batch);          // transient preview update
    activeStroke.Append(batch);     // pooled accumulation
    pool.Return(batch);
  }
  wetLayer.RenderMinimal();
}   

### 14.3 Finalize (Background + UI Commit)
// BACKGROUND
var simplified = DouglasPeucker(activeStroke.Points, 0.5);
var curve = BuildCurveIfZoomed(simplified);

// UI THREAD
var dv = new DrawingVisual();
using (var dc = dv.RenderOpen()) DrawStroke(dc, curve);
FreezeFreezablesUsedByStroke();
dryLayer.Add(dv);
wetLayer.ClearStroke(activeStroke.Id);
activeStroke.DisposeToPool();
```
