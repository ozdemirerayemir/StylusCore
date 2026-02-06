---
description: StylusCore Technical Constitution — 05_RENDERING_PERF_WPF (v1.0)
---

# ⚡ 05 — RENDERING & PERFORMANCE KERNEL (WPF / .NET 10)

This document defines STRICT, engine-grade rules for StylusCore’s WPF renderer to prevent FPS drops, stutter, excessive allocations, and visual-tree explosion.

StylusCore is a stylus-first Infinite Canvas with deep zoom (0.1–50x), a headless domain, QuadTree culling, DrawingVisual-based retained rendering, and View-to-Edit overlays.

---

## 0) Normative Language
- **MUST**: required
- **MUST NOT / FORBIDDEN**: prohibited
- **SHOULD**: recommended
- **MAY / OPTIONAL**: allowed but not required

---

## 1) RED LINES (Non-Negotiable)

### 1.1 Infinite Canvas + Camera
- **MUST:** Editor surface is infinite in world space.
- **FORBIDDEN:** `ScrollViewer` for the editor surface.
- **MUST:** Pan/zoom via camera matrix (`MatrixTransform`), clamp scale to `[0.1, 50.0]`.

### 1.2 Visual Primitives
- **MUST:** Dry ink retained as `DrawingVisual`.
- **FORBIDDEN:** Primary ink rendering as `WriteableBitmap` (zoom blur).
- **FORBIDDEN:** Thousands of WPF `Path` elements (visual tree blowup).
- **FORBIDDEN:** Rendering thousands of items as `FrameworkElement` (Layout/Measure/Arrange overhead).

### 1.3 Spatial Culling
- **MUST:** Visible set is computed via QuadTree viewport queries.
- **FORBIDDEN:** Full scans of all items per frame for rendering or hit-test.

### 1.4 View-to-Edit Boundary
- **FORBIDDEN:** Heavy controls on canvas in view mode (`DataGrid`, `RichTextBox`, `WebView`, etc.).
- **MUST:** Heavy controls appear only as edit overlays outside the canvas visual tree.

### 1.5 Theme Boundary
- **FORBIDDEN:** `DynamicResource` resolution inside hot render paths.
- **MUST:** Use cached frozen render resources (brush/pen/geometry cache). On theme change: reset cache and request redraw.

---

## 2) Performance Targets (Default Budgets)

These are default targets for mid-range devices (Surface-class) while scaling up on desktops.

- **SHOULD:** Frame cadence stable at 60Hz (avoid visible stutter).
- **SHOULD:** Renderer logic (excluding WPF composition) average < **2ms**/frame in typical viewport workloads.
- **MUST:** No per-frame unbounded allocations; allocations/frame must remain near-zero in steady state.
- **MUST:** Visual count must scale with viewport density, not document size.

---

## 3) Rendering Pipeline & Scheduling

### 3.1 Frame Pump
- **MUST:** Use a predictable frame pump (default: `CompositionTarget.Rendering`).
- **FORBIDDEN:** Doing heavy work inside the rendering callback.
- **MUST:** Per-frame work is limited to:
  - drain render/update requests
  - compute visible set (QuadTree query)
  - apply minimal visual diffs
  - coalesce invalidations

### 3.2 Work Partitioning
- **MUST:** Heavy computations (geometry rebuilds, text layout, image decode, caching) run off the UI thread.
- **MUST:** Visual creation/modification happens on UI thread only.
- **SHOULD:** Use bounded work per frame; if backlog exists, spread work across frames (avoid spikes).

---

## 4) Visual Layering (WPF)

### 4.1 Required Layer Stack
- **MUST:** Maintain distinct layers:
  1) Background + PageRegions (guides)
  2) Dry ink visuals (retained)
  3) Mixed-media lightweight visuals (retained)
  4) Selection/adorners (handles, lasso)
  5) Edit overlay host (real controls; outside canvas visual tree)

### 4.2 Z-Order Rules
- **MUST:** Z-order is resolved by engine state (LayerId + ZIndex), not by XAML element order.
- **SHOULD:** Keep overlay host always on top and separate from the main visual host.

---

## 5) Visual Retention Granularity (Default = Hybrid)

### 5.1 Allowed Strategies
- Per-item `DrawingVisual` (simple, but visual count can explode)
- Per-tile/per-chunk batching (low visual count, higher update complexity)
- **Hybrid** (recommended)

### 5.2 Default Strategy
- **MUST (Default):** Use **Hybrid retention**:
  - Ink-dense content MAY be batched into tile/chunk visuals (reduces visual count).
  - Mixed-media items are typically per-item visuals.
  - Selected/edited items MAY be promoted to their own visual temporarily.
  - After interaction ends, items MAY be demoted back into batch/tile representation.

### 5.3 Promotion/Demotion Rules
- **MUST:** Promotion happens for:
  - active edit overlay anchor target
  - currently dragged/resized items
  - selection feedback requiring per-item redraw
- **MUST:** Demotion happens when:
  - edit committed/canceled
  - manipulation ends
- **MUST:** Promotion/demotion must not change domain truth, only render representation.

### 5.4 Default Tile Guidance (Heuristic)
- **SHOULD:** Tile size begins in a conservative range and is benchmark-tuned.
- **SHOULD:** Use a fixed world-space tile grid (stable keys) so tiles can be reused and cached.
- **MUST:** Tile updates are incremental: update only tiles intersecting dirty rects.

---

## 6) Culling & Visible Set

### 6.1 Visible Set Computation
- **MUST:** Each frame (or on camera change), compute visible IDs via QuadTree `QueryViewport(worldViewport)`.
- **MUST:** Renderer only holds visuals for visible items + small hysteresis margin.

### 6.2 Hysteresis Margin (Anti-Thrash)
- **SHOULD:** Use a slightly expanded viewport (e.g., 110%–120%) for culling to reduce pop-in and constant add/remove at edges.

### 6.3 Visual Lifetime Management
- **MUST:** When items leave the (expanded) viewport, their visuals are removed or returned to a pool.
- **SHOULD:** Reuse visuals and cached resources when items re-enter.

### 6.4 Non-Allocating Queries
- **MUST:** QuadTree queries must avoid allocations in hot paths.
- **SHOULD:** Use pooled `List<Guid>` buffers for results.

---

## 7) Dirty Rect & Invalidation

### 7.1 Dirty Rect Source of Truth
- **MUST:** Dirty rects are computed from item AABBs in **world space**.
- **MUST:** Convert world rect → screen rect using the camera matrix for invalidation.

### 7.2 Padding/Gutter Rule
- **MUST:** Inflate dirty rects to account for:
  - stroke width
  - anti-aliasing
  - subpixel jitter
- **DEFAULT:** Add a small gutter (e.g., ~2px screen-space equivalent) to prevent artifacts.

### 7.3 Coalescing
- **MUST:** Coalesce invalidations per frame.
- **SHOULD:** If many scattered rects are produced, prefer:
  - multi-rect invalidation list with a cap, or
  - union into a few larger rects (benchmark-driven)
- **FORBIDDEN:** Forcing full redraw for small changes.

### 7.4 When Full Redraw Is Allowed
- **MAY:** Full redraw on:
  - theme change cache reset
  - major zoom level changes causing LOD switch
  - catastrophic state reset/reload

---

## 8) Resource Caching & Theme Updates

### 8.1 Render Resource Cache
- **MUST:** Provide a render-side cache for `Brush`, `Pen`, and geometry resources used by visuals.
- **MUST:** Cache outputs are frozen (if possible).
- **FORBIDDEN:** Creating new brushes/pens per frame for the same style.

### 8.2 Theme Change Protocol
- **MUST:** On theme change:
  1) Invalidate render resource cache
  2) Request controlled redraw of visible set
- **MUST:** Theme updates must not cause per-item `DynamicResource` lookups in draw loops.

---

## 9) Freezable Discipline & Allocation Control

### 9.1 Freezables
- **MUST:** Freeze geometries, brushes, pens, transforms when `CanFreeze`.
- **MUST NOT:** Freeze objects that are actively animated.

### 9.2 Pooling & GC Control
- **MUST:** Pool temporary lists/arrays used for:
  - QuadTree query results
  - tile update candidate sets
  - stroke geometry generation buffers
- **MUST:** Avoid LOH allocations (>85KB arrays) in steady-state.
- **SHOULD:** Segment very large arrays (points) into smaller pooled chunks.

---

## 10) Text Rendering (View Mode)

### 10.1 Default Approach
- **MUST:** View mode text is lightweight (no RichTextBox on canvas).
- **SHOULD:** Prefer `GlyphRun`-based rendering for bulk text where performance matters.
- **MAY:** Use `FormattedText` for smaller volumes or when layout complexity is needed.

### 10.2 Caching
- **MUST:** Cache text layout outputs (glyph runs, measured bounds) per zoom bucket or per style.
- **SHOULD:** Rebuild text geometry off the UI thread, commit visuals on UI thread.

---

## 11) Image Rendering

### 11.1 Decode Rules
- **MUST:** Set `DecodePixelWidth/Height` to target display size.
- **MUST:** Use async decode (`BitmapImage.IsAsync = true`) where appropriate.
- **FORBIDDEN:** Decoding full-resolution images on the UI thread during interaction.

### 11.2 Caching & Thumbnails
- **SHOULD:** Maintain a thumbnail cache for zoomed-out views.
- **SHOULD:** Use progressive loading: show low-res first, upgrade when idle.

---

## 12) Hit-Testing Performance (Render-Adjacent)

### 12.1 Two-Phase Hit Test
- **MUST:** Coarse hit-test uses QuadTree candidates (point/rect/lasso).
- **MUST:** Exact hit-test uses item geometry (headless), not VisualTreeHelper scanning.
- **FORBIDDEN:** Visual-tree scanning for hit tests at scale.

### 12.2 Allocation Rules
- **MUST:** Reuse candidate lists; avoid per-hit allocations.

---

## 13) Diagnostics & Profiling (Required)

### 13.1 Tooling
- **SHOULD:** Use WPF Perforator for UI/render insights.
- **SHOULD:** Use ETW/EventPipe/VS Profiler for CPU + allocation + GC analysis.
- **SHOULD:** Track UI thread stalls and long frames.

### 13.2 Instrumentation Points
- **MUST:** Log or expose metrics:
  - frame time (avg/p95/p99)
  - visible set size
  - visual count by layer
  - allocations/frame
  - QuadTree query time
  - tile rebuild time
- **SHOULD:** Provide a developer “Performance Overlay” toggled via debug shortcut.

---

## 14) Anti-Patterns & Pitfalls Checklist

- **FORBIDDEN:** `ItemsControl` binding thousands of items for rendering.
- **FORBIDDEN:** 1 `Path` per stroke segment at scale.
- **FORBIDDEN:** Full-scan hit-test or full-scan render loop.
- **FORBIDDEN:** Doing heavy work inside `CompositionTarget.Rendering`.
- **FORBIDDEN:** Per-frame creation of brushes/pens/geometries.
- **FORBIDDEN:** Theme resource lookups in hot loops.
- **FORBIDDEN:** Deep visual trees for canvas content.
- **FORBIDDEN:** UI-thread image decode during interaction.

---

## 15) Testing & Benchmarks (Required)

### 15.1 Scenarios
- **MUST:** 10k strokes viewport, rapid pan/zoom, stable frame time.
- **MUST:** 100k strokes total document (viewport-culling ensures stability).
- **MUST:** Mixed media heavy viewport (text + images + diagrams).
- **MUST:** Continuous interaction (drag/resize) without long frames.

### 15.2 Regression Harness
- **SHOULD:** Build repeatable benchmark scenes.
- **MUST:** Track regressions over time:
  - p95 frame time
  - memory after steady-state
  - allocations/frame
  - tile rebuild spikes

### 15.3 Leak Detection
- **MUST:** Ensure visuals and caches are released when items leave viewport.
- **SHOULD:** Memory snapshots before/after long sessions to detect leaks.

---

## 16) Recommended Defaults (Config)

| Key | Default | Notes |
|---|---:|---|
| `ZOOM_MIN` | 0.1 | clamp |
| `ZOOM_MAX` | 50.0 | clamp |
| `FRAME_PUMP` | `CompositionTarget.Rendering` | ~60Hz |
| `CULL_HYSTERESIS` | 1.15 | 110–120% recommended |
| `RENDER_STRATEGY` | Hybrid | tile ink + per-item mixed media |
| `DIRTY_GUTTER_PX` | 2 | screen-space equivalent |
| `TEXT_RENDER_DEFAULT` | GlyphRun | FormattedText for low volume |
| `IMAGE_DECODE` | DecodePixel + async | no UI stalls |
| `QUERY_ALLOCS` | near-zero | pooled lists |

---
