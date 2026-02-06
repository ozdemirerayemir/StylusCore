---
description: StylusCore Technical Constitution — 03_CANVAS_ENGINE (v1.0)
---

# 🧱 03 — CANVAS ENGINE (Infinite World + Camera + QuadTree + View-to-Edit)

This document defines the STRICT rules for the StylusCore Canvas Engine.
**Goal:** an engine-grade Infinite Canvas core (OneNote/Miro-class) on WPF/.NET 10 with deep zoom, high-performance ink, and mixed media.

---

## 0) Normative Language
- **MUST**: required
- **MUST NOT / FORBIDDEN**: prohibited
- **SHOULD**: recommended
- **MAY / OPTIONAL**: allowed but not required

---

## 1) RED LINES (Non-Negotiable)

### 1.1 Infinite Canvas Mandate
- **MUST:** Editor surface is an **Infinite Canvas** in world coordinates.
- **FORBIDDEN:** `ScrollViewer` for the editor surface.
- **MUST:** Pan/zoom uses a **camera matrix** (WPF `MatrixTransform`).

### 1.2 Headless Domain Mandate
- **MUST:** Core canvas models are UI-agnostic (no `System.Windows.*`).
- **FORBIDDEN:** `Point`, `Rect`, `Brush`, `Color`, `Geometry`, `UIElement`, `DependencyObject` in domain.
- **MUST:** Use primitive/headless representations:
  - Coordinates: `double X, Y`
  - Rect: `minX,maxX,minY,maxY`
  - Color: `uint ARGB (0xAARRGGBB)`
  - Geometry: point arrays / lightweight structs only

### 1.3 Rendering Primitives
- **MUST:** Dry ink retained as `DrawingVisual`.
- **FORBIDDEN:** Primary ink rendering as `WriteableBitmap` (zoom blur).
- **FORBIDDEN:** Thousands of per-stroke `Path` objects (visual tree explosion).

### 1.4 Spatial Culling
- **MUST:** Culling uses **QuadTree** (viewport queries).
- **FORBIDDEN:** Scanning all items every frame for rendering/hit-test.

### 1.5 Mixed Media View-to-Edit
- **FORBIDDEN:** Heavy controls on the canvas in view mode (`DataGrid`, `RichTextBox`, etc.).
- **MUST:** View-to-Edit overlay:
  - View mode: lightweight visuals (`DrawingVisual` / minimal primitives)
  - Edit mode: overlay real WPF control anchored to world coords

---

## 2) Engine Layering & Dependency Flow

### 2.1 Layers
- **Core (Headless):** models, algorithms, QuadTree interfaces, command interfaces
- **Engine (WPF Bridge):** visuals, camera, hit-test bridge, edit overlay host
- **App (Shell/MVVM):** navigation/toolbars/settings/dialogs

### 2.2 Dependency Rules
- **MUST:** `App → Engine → Core` only.
- **FORBIDDEN:** Core referencing Engine/App assemblies.

---

## 3) Core Concepts

### 3.1 Spaces
- **World Space:** infinite coordinate plane (source of truth for items)
- **Screen Space:** pixels after camera transform (UI overlays)

### 3.2 Camera (Canonical State)
- **MUST:** Camera state stored canonically (e.g., `CenterWorld`, `Scale`).
- **MUST:** Rebuild camera matrix from canonical state (avoid drift from incremental matrix multiplication).
- **MUST:** Clamp scale to `[0.1, 50.0]`.
- **MUST:** Pinch zoom pivot = gesture centroid (screen-space pivot mapped to world-space).

### 3.3 Visual Layers (WPF)
- **MUST:** Separate layers:
  1) Background + PageRegions (guides)
  2) Dry ink + lightweight objects
  3) Selection adorners / handles
  4) Edit overlays (real controls, screen-space host)

---

## 4) Domain Data Model (Headless)

### 4.1 Item Model
- **MUST:** Every item has:
  - `ItemId` (stable)
  - `Kind` (Stroke/Text/Table/Image/Diagram/Group/PageRegion/…)
  - `LayerId` (optional)
  - `ZIndex` (or ordered list)
  - `AABB` (world-space bounds)
  - `Transform` (optional; headless, e.g., matrix struct)

### 4.2 Minimum Types (baseline)
- **StrokeItem:** point data reference + style (color, width) + AABB
- **TextItem:** text runs / layout data + style + AABB
- **TableItem:** grid model + style + AABB
- **ImageItem:** asset ref + display rect + AABB
- **DiagramItem:** vector primitives (shapes/connectors) + AABB
- **GroupItem:** child ids + group transform + AABB

### 4.3 PageRegion (Optional Visual Preset)
- **MUST:** PageRegion is a **visual guide**, not a container.
- **MUST:** Content may overflow; no clipping by default.
- **MUST:** PageRegion supports presets and orientation:
  - ISO: A4, A5 (and optionally A3/A6)
  - US: Letter, Legal, Tabloid/Ledger
  - Custom width/height allowed
- **MUST:** PageRegion defines export/print boundaries when requested.
- **DEFAULT:** No snapping/grid logic required.
- **SHOULD:** Provide extension points for optional snapping/alignment later.

---

## 5) Spatial Indexing (QuadTree)

### 5.1 QuadTree Responsibilities
- **MUST:** Maintain mapping from `ItemId → AABB`.
- **MUST:** Support:
  - `QueryViewport(RectD worldViewport) → IEnumerable<ItemId>`
  - `QueryPoint(Vec2 worldPoint, tolerance) → candidates`
  - `QueryRect(RectD worldRect) → candidates`
  - `QueryLasso(Polyline worldPath) → candidates` (coarse filter; exact test after)

### 5.2 Update Rules
- **MUST:** Insert/update/remove in QuadTree on:
  - item add/remove
  - transform changes
  - geometry changes (stroke finalize, text reflow, etc.)
- **SHOULD:** Keep capacity/branching configurable (benchmark-tuned).
- **FORBIDDEN:** Allocating new nodes per query; queries should be allocation-light.

---

## 6) Rendering Architecture (WPF)

### 6.1 Visual Retention Granularity (Default = Hybrid)
- **MUST (Default):** Use a **Hybrid** model:
  - Ink-heavy content may be **batched** (per spatial chunk/tile visual) to reduce visual count
  - Non-ink objects (text/table/image/diagram) may be **per-item visuals**
  - When an item is being edited or interacted with, it may be temporarily “promoted” to its own visual
- **FORBIDDEN:** 1 visual per tiny segment (over-fragmentation).
- **FORBIDDEN:** A single mega-visual for the entire document (update cost too high).

### 6.2 Dirty Rect Invalidation
- **MUST:** Redraw only the impacted region when possible.
- **MUST:** Use item AABB (+ padding) to compute dirty rect requests.
- **SHOULD:** Batch invalidations per frame (coalesce).

### 6.3 Theme Resources Boundary
- **FORBIDDEN:** `DynamicResource` resolution inside hot render paths.
- **MUST:** Use a render-side cached brush/pen provider; invalidate caches on theme change and request redraw.

### 6.4 Freezable Discipline
- **MUST:** Freeze pens/brushes/geometries/transforms used by retained visuals whenever possible.
- **MUST:** Check `CanFreeze` before `Freeze()`.

---

## 7) Interaction & Tool State Machine (Engine-Side)

### 7.1 Tool Model
- **MUST:** Engine defines tool states:
  - Ink, Erase (whole-stroke baseline), Select, Lasso, Move, Resize, Pan, Zoom
- **MUST:** UI shell selects tools; engine executes tool behavior.

### 7.2 Selection Model
- **MUST:** Maintain selection as a set of `ItemId`.
- **MUST:** Provide hit-test methods returning candidate IDs via QuadTree, then exact tests.
- **SHOULD:** Support multi-select, group selection, bounding box handles, and move/resize transforms.

### 7.3 Input Arbitration (Boundary Only)
- **MUST:** Stylus = ink/erase; Touch = pan/zoom; Mouse = selection/manipulation.
- **NOTE:** Detailed stylus threading rules live in `04_INPUT_INK_THREADING.md`.

---

## 8) View-to-Edit (Mixed Media)

### 8.1 View Mode Rendering
- **MUST:** Text/table/code/diagram render as lightweight visuals:
  - `DrawingVisual` preferred
  - minimal WPF primitives only if truly cheap
- **FORBIDDEN:** Heavy WPF controls in view mode on the canvas.

### 8.2 Edit Overlay Host
- **MUST:** Edit overlay is hosted outside the canvas visual tree (overlay layer).
- **MUST:** Overlay positioning is derived from world rect → camera → screen rect.
- **MUST:** On commit:
  - update domain item
  - recompute AABB
  - update QuadTree
  - regenerate lightweight view visual
- **MUST:** On cancel: discard changes and restore prior visual.

---

## 9) Persistence & Undo/Redo Hooks (Interfaces Only)

### 9.1 Engine Interfaces
- **MUST:** Engine exposes headless serialization boundaries; no WPF types in persisted model.
- **MUST:** Canvas operations are commands (undoable), but full undo spec lives in `07_UNDO_REDO.md`.

### 9.2 Command Hooks (minimum)
- **MUST:** Engine operations produce `IUndoableCommand` units:
  - AddItem / RemoveItem / TransformItem / EditText / CommitStroke / etc.
- **MUST:** Support grouping hooks (e.g., time-based grouping for strokes).

---

## 10) Performance & Memory Rules

### 10.1 Visual Tree Budget
- **MUST:** Keep visual count bounded by viewport + batching strategy.
- **SHOULD:** Prefer batching for dense ink regions.

### 10.2 Allocation Control
- **MUST:** Avoid per-frame allocations in hot loops (render/query).
- **SHOULD:** Use pooling for temporary lists used by QuadTree queries and rendering batches.

### 10.3 LOD (Optional)
- **MAY:** Apply LOD policies:
  - extremely zoomed-out: simplified stroke representation
  - thumbnails for heavy images
- **MUST:** LOD must not change domain truth, only rendering representation.

---

## 11) Testing & Validation

### 11.1 Correctness
- **MUST:** Camera math tests (centroid zoom, pan invariants, clamp rules).
- **MUST:** Hit-test correctness tests (point/rect/lasso across zoom levels).
- **MUST:** Selection/transform tests (AABB updates + QuadTree consistency).

### 11.2 Performance
- **MUST:** Stress tests with item counts in the millions (culling correctness).
- **SHOULD:** Metrics:
  - frame time
  - visible set size
  - allocations per frame
  - query time for viewport/hit-test

---

## 12) Common Pitfalls & Anti-Patterns

- **FORBIDDEN:** Binding thousands of canvas items via `ItemsControl` / `ObservableCollection` for rendering.
- **FORBIDDEN:** Full-scan hit-testing.
- **FORBIDDEN:** Heavy controls in view mode.
- **FORBIDDEN:** Theme lookups (`DynamicResource`) inside draw loops.
- **FORBIDDEN:** Camera drift from incremental matrix accumulation.

---

## 13) Recommended Defaults (Config)

| Key | Default | Notes |
|---|---:|---|
| `ZOOM_MIN` | 0.1 | clamp |
| `ZOOM_MAX` | 50.0 | clamp |
| `CAMERA_CANONICAL_STATE` | enabled | rebuild matrix each update |
| `RENDER_STRATEGY` | Hybrid | batch ink, per-item mixed media |
| `QUADTREE_CAPACITY` | 32–64 (tune) | benchmark-driven |
| `PAGE_REGIONS_ENABLED` | optional | guide/export only |
| `PAGE_DEFAULT_PRESET` | A4 Portrait | user-configurable |
| `SNAP_ENABLED` | false | extension only |

---

## 14) Minimal API Sketch (Headless)

```csharp
public readonly record struct Vec2(double X, double Y);
public readonly record struct RectD(double MinX, double MinY, double MaxX, double MaxY);

public enum CanvasItemKind { Stroke, Text, Table, Image, Diagram, Group, PageRegion }

public interface ICanvasItem {
    Guid Id { get; }
    CanvasItemKind Kind { get; }
    int LayerId { get; }
    int ZIndex { get; }
    RectD Bounds { get; } // world AABB
}

public interface ISpatialIndex {
    void Insert(Guid id, RectD bounds);
    void Update(Guid id, RectD bounds);
    void Remove(Guid id);

    void QueryViewport(RectD viewport, List<Guid> results);
    void QueryPoint(Vec2 point, double tol, List<Guid> results);
    void QueryRect(RectD rect, List<Guid> results);
}

public interface ICanvasEngine {
    // camera
    void SetCamera(Vec2 centerWorld, double scale);
    RectD GetWorldViewport();

    // items
    void AddItem(ICanvasItem item);
    void RemoveItem(Guid id);
    void UpdateItem(ICanvasItem item);

    // hit-test
    void HitTestPoint(Vec2 worldPoint, double tol, List<Guid> results);
}
