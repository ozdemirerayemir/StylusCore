---
description: StylusCore Technical Constitution — 05_RENDERING_PERF_WPF
---

# ⚡ 05 — RENDERING & PERFORMANCE KERNEL (WPF / .NET)

This document defines **STRICT, engine-grade rules** for StylusCore’s WPF rendering layer.  
Its purpose is to guarantee **stable frame times, zero jitter, bounded memory usage, and predictable performance** under deep zoom and large datasets.

This document governs **rendering only**.  
Domain rules live in `03_CANVAS_ENGINE.md`.  
Input rules live in `04_INPUT_INK_THREADING.md`.

---

## 0) Normative Language
- **MUST**: required  
- **MUST NOT / FORBIDDEN**: prohibited  
- **SHOULD**: recommended  
- **MAY**: optional  

---

## 1) RED LINES (Non-Negotiable)

### 1.1 Infinite Canvas & Camera (Render Boundary)
- **MUST:** Editor renders an infinite world-space canvas.
- **FORBIDDEN:** `ScrollViewer` for editor rendering.
- **MUST:** All pan/zoom is applied via a camera matrix (`MatrixTransform`).
- **MUST:** Scale is clamped to `[0.1, 50.0]`.

---

### 1.2 Rendering Primitives
- **MUST:** Dry ink is retained as `DrawingVisual`.
- **FORBIDDEN:** `WriteableBitmap` as primary ink representation.
- **FORBIDDEN:** Thousands of `Path` or `FrameworkElement` instances.
- **FORBIDDEN:** Layout-driven rendering for canvas items.

---

### 1.3 Spatial Culling
- **MUST:** Visible items are computed via QuadTree viewport queries.
- **FORBIDDEN:** Full document scans per frame.

---

### 1.4 View-to-Edit Boundary
- **FORBIDDEN:** Heavy WPF controls on the canvas in view mode.
- **MUST:** Heavy controls appear only as edit overlays outside the canvas visual tree.

---

### 1.5 Theme Boundary
- **FORBIDDEN:** `DynamicResource` resolution in hot render paths.
- **MUST:** Use cached, frozen render resources.
- **MUST:** On theme change, invalidate cache and trigger controlled redraw.

---

## 2) Performance Targets

- **SHOULD:** Stable 60Hz frame cadence.
- **SHOULD:** Renderer logic under 2ms/frame.
- **MUST:** Near-zero allocations per frame.
- **MUST:** Visual count scales with viewport density.

---

## 3) Rendering Pipeline & Scheduling

### 3.1 Frame Pump
- **MUST:** Use deterministic frame pump (`CompositionTarget.Rendering`).
- **FORBIDDEN:** Heavy work inside render callback.
- **MUST:** Per-frame work limited to visible-set resolution and minimal diffs.

---

### 3.2 Work Partitioning
- **MUST:** Heavy computation off UI thread.
- **MUST:** Visual creation on UI thread only.
- **SHOULD:** Spread work across frames.

---

## 4) Visual Layering

### 4.1 Required Layer Stack
1. Background + PageRegions  
2. Dry ink visuals  
3. Lightweight mixed-media visuals  
4. Selection & adorners  
5. Edit overlay host  

---

### 4.2 Z-Order
- **MUST:** Z-order resolved by engine state, not XAML order.
- **MUST:** Overlay host always on top.

---

## 5) Visual Retention Strategy (Hybrid)

### 5.1 Allowed Models
- Per-item visuals  
- Per-tile batching  
- Hybrid (default)

---

### 5.2 Hybrid Rules
- Ink-dense regions may be batched.
- Mixed media typically per-item.
- Active items may be promoted.
- Promotion/demotion affects rendering only.

---

### 5.3 Tile Discipline
- **SHOULD:** Fixed world-space tile grid.
- **MUST:** Update only tiles intersecting dirty rects.
- **FORBIDDEN:** Full tile rebuilds for local changes.

---

## 6) Culling & Visual Lifetime

- **MUST:** QuadTree-driven visible set.
- **SHOULD:** Hysteresis margin (~110–120%).
- **MUST:** Remove or pool visuals when leaving viewport.
- **MUST:** Allocation-free queries.

---

## 7) Dirty Rect & Invalidation

- **MUST:** Dirty rects originate from world-space AABBs.
- **MUST:** Convert via camera to screen-space.
- **MUST:** Inflate for AA and stroke width.
- **MUST:** Coalesce per frame.
- **FORBIDDEN:** Full redraws for local changes.

---

## 8) Render Resource Caching

- **MUST:** Cache brushes, pens, geometries.
- **MUST:** Freeze whenever possible.
- **FORBIDDEN:** Per-frame recreation.

---

## 9) Allocation & Freezable Discipline

- **MUST:** Pool all temporary buffers.
- **MUST:** Avoid LOH allocations.
- **MUST:** Freeze Freezables unless animated.

---

## 10) Text Rendering (View Mode)

- **MUST:** No `RichTextBox` on canvas.
- **SHOULD:** `GlyphRun` for large text.
- **MAY:** `FormattedText` for small layouts.
- **MUST:** Cache text layouts by zoom/style.

---

## 11) Image Rendering

- **MUST:** Decode to target resolution.
- **MUST:** Async decode where possible.
- **FORBIDDEN:** UI-thread full-res decode during interaction.
- **SHOULD:** Thumbnail + progressive loading.

---

## 12) Hit-Testing

- **MUST:** QuadTree → geometry exact test.
- **FORBIDDEN:** Visual-tree hit testing.
- **MUST:** Reuse buffers.

---

## 13) Diagnostics & Profiling

### Required Metrics
- Frame time (avg/p95/p99)
- Visible item count
- Visual count by layer
- Allocations/frame
- QuadTree query time

---

## 14) Anti-Patterns

- ItemsControl for canvas rendering  
- 1 Path per stroke segment  
- Full-scan render loops  
- Heavy work in render callback  
- Theme lookups in hot paths  
- Deep visual trees  
- UI-thread image decode  

---

## 15) Recommended Defaults

| Key | Default |
|---|---:|
| ZOOM_MIN | 0.1 |
| ZOOM_MAX | 50.0 |
| FRAME_PUMP | CompositionTarget.Rendering |
| CULL_HYSTERESIS | 1.15 |
| RENDER_STRATEGY | Hybrid |
| DIRTY_GUTTER_PX | ~2 |
| TEXT_RENDER_DEFAULT | GlyphRun |
