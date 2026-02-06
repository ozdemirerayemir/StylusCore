---
description: StylusCore Technical Constitution — 04_INPUT_INK_THREADING (v1.1)
---

# 🖊️ 04 — INPUT, INK & THREADING (StylusPlugIn + Wet/Dry + 16ms Pump)

This document defines the STRICT, engine-grade rules for StylusCore’s low-latency stylus input on WPF/.NET 10.
It is written to be directly consumable by AI agents (Antigravity/Cursor) and humans.

**Goal:** Infinite Canvas in world coordinates with Deep Zoom (0.1–50x), supporting high sample rates (Surface/Wacom) without jitter, UI freezes, or threading bugs.

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
- **FORBIDDEN:** Use WPF `InkCanvas` or `InkPresenter` control as the editor surface. Use custom `CanvasHostControl` in Engine.Wpf instead.

### 1.2 Pen Thread UI Access Is Forbidden
- **MUST:** `StylusPlugIn.OnStylusDown/Move/Up` runs on the **PEN THREAD**.
- **FORBIDDEN on the pen thread:**
  - Reading/writing any `DependencyObject` / `DependencyProperty`
  - Accessing `Brush`, `Color`, `Geometry`, `Transform`, `DrawingVisual`, or any Visual Tree object
  - Adding/removing visuals
  - Blocking calls to UI thread (`Dispatcher.Invoke`, synchronous calls)
- **MUST:** Pen thread only performs minimal arithmetic and **pushes raw input into a thread-safe buffer**.

### 1.3 Wet/Dry Pipeline (Required)
- **MUST:** Use a **Wet** (preview) + **Dry** (retained) pipeline.
- **MUST:** Dry ink is retained as **`DrawingVisual`**.
- **FORBIDDEN:** Using `WriteableBitmap` as the primary dry ink layer (zoom blur).
- **FORBIDDEN:** Creating one WPF `Path` per stroke segment at scale (visual tree blowup).

### 1.4 Pan/Zoom
- **MUST:** Pan/zoom via **camera `MatrixTransform`**.
- **FORBIDDEN:** `ScrollViewer` on the editor surface.

---

## 2) Goals / Non-goals

### 2.1 Goals
- **MUST:** Pen input capture must not stall even if the UI thread is slow.
- **SHOULD:** Minimize tip-to-ink latency and reduce visible “inking gap”.
- **MUST:** Precision must remain stable at large world coordinates and at 50x zoom.

### 2.2 Non-goals (for now)
- **NOT PRIORITY:** Multi-pen / multi-user (assume a single active stylus).
- **NOT PRIORITY:** Complex OS gesture features (flicks, press-and-hold, handwriting panel).

---

## 3) WPF Input Model (What Runs Where)

### 3.1 StylusPlugIn Lifecycle
- A `StylusPlugIn` is attached to the drawing surface via `UIElement.StylusPlugIns`.
- WPF invokes StylusPlugIn callbacks on the **pen thread** via the RTS pipeline.
- **MUST:** The plugin outputs only raw data packets:
  - `strokeId`, `kind` (Down/Move/Up)
  - `(x, y)` in **world coordinates** or “world-ready” coordinates
  - optional: `pressure`, `tiltX`, `tiltY`, `timestamp`, `buttons`, `isEraser`, `deviceId`

### 3.2 Disable System Gestures (Consistency)
- **MUST:** Disable on the drawing surface:
  - press-and-hold (right-click emulation)
  - pen flicks
  - (optional) system handwriting gestures
- **Rationale:** These cause latency spikes and inconsistent interactions.

---

## 4) End-to-End Pipeline (Required Architecture)

### 4.1 Pen Thread: Capture → Buffer
- **MUST:** Convert incoming stylus points into **batched packets** and enqueue them.
- **SHOULD:** Enqueue per RTS event (batch), not per point (overhead reduction).

### 4.2 UI Thread: ~16ms Frame Pump → Wet Preview
- **MUST:** Consume buffered packets on the UI thread at ~16ms cadence.
- **DEFAULT:** `CompositionTarget.Rendering`.
- **MUST:** Each frame:
  1) Drain all available packets
  2) Update wet preview state
  3) Perform minimal invalidation and draw

### 4.3 Background: Post-Stroke Refinement
- **MUST:** On stylus up, heavy work runs on a background thread:
  - smoothing (Catmull-Rom → Bezier or equivalent)
  - simplification (Douglas–Peucker, epsilon = 0.5)
  - bounding box computation
- **MUST:** Background thread does **not** create WPF visuals.

### 4.4 UI Thread: Dry Commit
- **MUST:** Create/modify `DrawingVisual` on the UI thread only.
- **MUST:** Freeze all Freezables (`Pen`, `Brush`, `Geometry`, `Transform`) before use.
- **MUST:** Clear wet layer after commit.

---

## 5) Buffering Strategy (Default = SPSC Channel + Batches)

### 5.1 Default Buffer Primitive
- **MUST (Default):** `System.Threading.Channels` with **SingleWriter / SingleReader**.
- **DEFAULT:** Unbounded channel + guard rails.
- **OPTIONAL:** Ring buffer (only if proven needed; complexity is higher).

### 5.2 Packet Format (Required)
- **MUST:** Buffer messages are `PointBatch` (not single points):
  - `strokeId`
  - `kind`: Down | MoveBatch | Up
  - pooled array for points (`InkPoint[]` + `count`)
  - `timestampRange`
  - optional: pressure/tilt/button flags

### 5.3 Backpressure (Never Block Pen Thread)
- **MUST:** Pen thread must never block waiting on the UI.
- **MUST:** Use `TryWrite` (or lock-free write semantics).
- **MUST:** If the UI cannot keep up:
  - preview may degrade,
  - but **final stroke correctness must be preserved**.

**Recommended safe policy:**
1) Use channel for real-time preview packets.
2) Maintain an “overflow collector” for final accuracy:
   - If channel write fails (bounded mode) or queue pressure exceeds threshold,
     append raw points to an overflow list (pooled).
3) On `StylusUp`, send a final “Up + full remainder” packet so the stroke can be finalized correctly.

> NOTE: Coalescing is allowed only for **wet preview**. Do not permanently lose input data.

---

## 6) Coordinate Systems & Camera Math

### 6.1 World vs Screen
- **MUST:** Store stroke data in **WORLD COORDS**.
- **FORBIDDEN:** Storing screen coords or repeatedly transforming points.
- **MUST:** All pan/zoom is applied via the camera matrix at render time.

### 6.2 Precision Rule
- **MUST:** Use `double` for world coordinates in domain/engine state (precision safety).
- **MAY:** Convert to float for transient rendering geometry if needed, but persist/domain remains double.

### 6.3 Pinch Zoom Pivot
- **MUST:** Pivot = gesture centroid.
- **MUST:** Clamp scale to `[0.1, 50.0]`.
- **SHOULD:** Use ScaleAt(pivot) + translation compensation to keep centroid stable.

### 6.4 Thread-Safe Camera Snapshot
- **MUST:** The pen thread must not compute transforms.
- **SHOULD:** Publish camera state as an immutable snapshot (`MatrixD` struct) readable without UI access.

---

## 7) Input Arbitration & Palm Rejection

### 7.1 Arbitration Rules
- **MUST:** Stylus = ink (draw/erase).
- **MUST:** Touch = pan/zoom.
- **MUST:** Mouse = selection/manipulation.

### 7.2 Palm Rejection
- **MUST:** Suppress touch for **300ms** after the last stylus event.
- **SHOULD:** Implement as a small state machine (accounts for “stylus in range” vs “stylus down”).

### 7.3 Capture
- **SHOULD:** Capture input when stroke starts so the stroke doesn’t get cut off if the pen leaves the surface.
- **MUST:** Always release capture on up/cancel.

---

## 8) Stroke Data Representation (Memory Safe)

### 8.1 Active Stroke Storage
- **MUST:** Use pooled buffers (`ArrayPool<T>`) for high-rate point capture.
- **FORBIDDEN:** Keeping `StylusPointCollection` long-term.
- **SHOULD:** Compact point struct:
  - `InkPoint { double x, y; float pressure; short tiltX, tiltY; uint t; }`

### 8.2 Pressure/Tilt
- **OPTIONAL:** Persist pressure for brush width.
- **OPTIONAL:** Persist tilt if your brushes need it.
- **SHOULD:** If not used, keep it as an extension field but don’t store it by default.

### 8.3 Eraser
- **SHOULD:** Detect eraser end / eraser mode and route to erase tool.
- **DEFAULT:** Whole-stroke erase (fast).
- **OPTIONAL:** Partial erase (stroke splitting) is future scope.

---

## 9) Stroke Optimization & Rendering Quality

### 9.1 Simplification
- **MUST:** Apply Douglas–Peucker after stroke completion: `epsilon = 0.5`.
- **MUST:** Discard raw input arrays after finalization.

### 9.2 High Zoom Curves
- **SHOULD:** If zoom > 2.0x, render visually smoother curves:
  - centripetal Catmull–Rom → cubic Bezier segments (visual-only OK)
- **MUST:** Keep the persisted data compact; do not store heavy geometry objects.

### 9.3 Bounding Boxes
- **MUST:** Compute stroke AABB for:
  - dirty rect invalidation
  - spatial indexing (QuadTree insertion in rendering kernel)
- **SHOULD:** Inflate bbox by stroke width and AA padding.

---

## 10) Threading & Safety Rules

### 10.1 Pen Thread Forbidden List (Final)
- **FORBIDDEN:** UI access, visual creation, synchronous UI calls, heavy allocations, locks that can block.

### 10.2 UI Thread Boundaries
- **MUST:** All `DrawingVisual` creation/modification on UI thread.
- **MUST:** Freeze Freezables before using them in visuals.
- **FORBIDDEN:** Creating WPF visuals on background threads.

### 10.3 Shutdown / Cancellation
- **MUST:** On shutdown:
  - unsubscribe frame pump
  - stop channel reader
  - return pooled arrays
- **SHOULD:** If persistence happens while drawing:
  - snapshot the “active stroke partial” safely without blocking input.

---

## 11) Performance & Memory Guidance

### 11.1 GC/LOH Protection
- **MUST:** Avoid per-point allocations.
- **MUST:** Reuse buffers (`ArrayPool`, `MemoryPool`).
- **SHOULD:** Avoid LOH allocations (>85KB arrays) by batching/segmenting.

### 11.2 No MVVM Bindings in Move Loop
- **FORBIDDEN:** Per-move ICommand/binding churn.
- **MUST:** Route input to an `InputManager` / engine, not directly to ViewModel commands per point.

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
- **SHOULD:** Measure tip-to-ink latency (camera or instrumentation).
- **MUST:** Log `timestamp(pen) → timestamp(render)`.

### 12.2 Stress
- **MUST:** Rapid scribble at high sample rates.
- **MUST:** Long continuous stroke (60s+).
- **MUST:** Fast zoom warp (0.1 ↔ 50).
- **MUST:** Palm rejection scenarios.

### 12.3 Gesture Correctness
- **MUST:** Centroid-pivot pinch zoom should not drift.

---

## 13) Common Pitfalls & Anti-Patterns

- **FORBIDDEN:** Resolving `DynamicResource` in hot render loops (use cached resources for canvas).
- **FORBIDDEN:** Creating visuals on background thread.
- **FORBIDDEN:** Retaining raw `StylusPointCollection` indefinitely.
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
