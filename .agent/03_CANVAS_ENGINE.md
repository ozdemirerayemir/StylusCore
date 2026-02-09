---
description: StylusCore Technical Constitution — 03_CANVAS_ENGINE
---

# 🧱 03 — CANVAS ENGINE  
(Infinite World · Camera · QuadTree · View-to-Edit)

This document defines the **STRICT, engine-level rules** for the StylusCore Canvas Engine.

**Goal:**  
A true **engine-grade Infinite Canvas core** (OneNote / Miro class) on WPF /.NET 10, supporting deep zoom, high-performance ink, spatial culling, and mixed media — without architectural drift.

This document applies to the **Engine layer** and its interaction with Core and App.  
Shell UI rules live in `08_XAML_UI_HIERARCHY.md`.

---

## 0) Normative Language
- **MUST**: required
- **MUST NOT / FORBIDDEN**: prohibited
- **SHOULD**: recommended
- **MAY / OPTIONAL**: allowed but not required

---

## 1) RED LINES (Non-Negotiable)

### 1.1 Infinite Canvas Mandate
- **MUST:** The editor surface is an **Infinite Canvas** in world coordinates.
- **FORBIDDEN:** Using `ScrollViewer` for editor navigation.
- **MUST:** Pan and zoom are implemented via a **camera transform**  
  (WPF `MatrixTransform` or equivalent pipeline).

---

### 1.2 Headless Domain Mandate
- **MUST:** Core canvas models are **UI-agnostic**.
- **FORBIDDEN:** Any `System.Windows.*` types in Core:
  - `Point`, `Rect`, `Brush`, `Color`, `Geometry`
  - `UIElement`, `DependencyObject`, `Visual`
- **MUST:** Use headless primitives only:
  - Coordinates: `double X, Y`
  - Rectangles: `(MinX, MinY, MaxX, MaxY)`
  - Color: `uint ARGB (0xAARRGGBB)`
  - Geometry: point arrays / lightweight structs

---

### 1.3 Rendering Primitives
- **MUST:** Dry ink is retained as `DrawingVisual`.
- **FORBIDDEN:** Primary ink rendering via `WriteableBitmap` (zoom blur).
- **FORBIDDEN:** Thousands of per-stroke `Path` elements  
  (visual tree explosion).

---

### 1.4 Spatial Culling
- **MUST:** Visibility and hit-testing are driven by a **QuadTree**.
- **FORBIDDEN:** Scanning all items every frame.

---

### 1.5 Mixed Media View-to-Edit
- **FORBIDDEN:** Heavy WPF controls on the canvas in view mode  
  (`DataGrid`, `RichTextBox`, etc.).
- **MUST:** Use **View-to-Edit**:
  - View mode → lightweight visuals only
  - Edit mode → real WPF control in an overlay, anchored to world space

---

## 2) Engine Layering & Dependency Flow

### 2.1 Layers
- **Core (Headless):**  
  Models, algorithms, QuadTree interfaces, command interfaces
- **Engine (WPF Bridge):**  
  Visuals, camera, spatial culling, hit-testing, edit overlay host
- **App (Shell / MVVM):**  
  Navigation, tool selection, dialogs, settings

---

### 2.2 Dependency Rules
- **MUST:** `App → Engine → Core`
- **FORBIDDEN:** Core referencing Engine or App assemblies.

---

## 3) Core Concepts

### 3.1 Coordinate Spaces
- **World Space:**  
  Infinite, canonical coordinate system (source of truth)
- **Screen Space:**  
  Pixels after camera transform (UI overlays only)

---

### 3.2 Camera (Canonical State)
- **MUST:** Store camera state canonically:
  - `CenterWorld`
  - `Scale`
- **MUST:** Rebuild the camera matrix from canonical state  
  (no incremental matrix drift).
- **MUST:** Clamp zoom scale to `[0.1, 50.0]`.
- **MUST:** Pinch-zoom pivot is the **gesture centroid**, mapped from screen → world.

---

### 3.3 Visual Layers (WPF)
- **MUST:** Maintain separate visual layers:
  1. Background + PageRegions (guides)
  2. Dry ink + lightweight items
  3. Selection adorners / handles
  4. Edit overlays (screen-space host)

---

## 4) Domain Data Model (Headless)

### 4.1 Base Item Contract
Every canvas item **MUST** define:
- `ItemId` (stable)
- `Kind` (Stroke / Text / Table / Image / Diagram / Group / PageRegion / …)
- `LayerId` (optional)
- `ZIndex` (or ordered grouping)
- `AABB` (world-space bounds)
- `Transform` (optional, headless)

---

### 4.2 Minimum Item Types
- **StrokeItem:** point data + style + AABB
- **TextItem:** layout data + style + AABB
- **TableItem:** grid model + style + AABB
- **ImageItem:** asset ref + rect + AABB
- **DiagramItem:** vector primitives + AABB
- **GroupItem:** child IDs + group transform + AABB

---

### 4.3 PageRegion (Visual Guide)
- **MUST:** PageRegion is a **visual guide**, not a container.
- **MUST:** Content may overflow; no clipping by default.
- **MUST:** Support presets and orientation:
  - ISO: A4, A5 (optional A3/A6)
  - US: Letter, Legal, Ledger
  - Custom sizes allowed
- **MUST:** Define export / print boundaries when requested.
- **DEFAULT:** No snapping or grid logic.
- **SHOULD:** Provide extension points for snapping/alignment later.

---

## 5) Spatial Indexing (QuadTree)

### 5.1 Responsibilities
- **MUST:** Maintain mapping `ItemId → AABB`.
- **MUST:** Support:
  - viewport queries
  - point / rect / lasso coarse queries
- **MUST:** Exact hit-testing happens **after** QuadTree filtering.

---

### 5.2 Update Rules
- **MUST:** Update QuadTree on:
  - item add / remove
  - geometry changes
  - transforms
- **SHOULD:** Capacity and branching are configurable and benchmark-driven.
- **FORBIDDEN:** Allocating new nodes per query.

---

## 6) Rendering Architecture (WPF)

### 6.1 Visual Retention Strategy (Default: Hybrid)
- **MUST:** Use a **Hybrid** model:
  - Dense ink → batched visuals (per spatial chunk)
  - Mixed media → per-item visuals
  - Actively edited items may be temporarily promoted
- **FORBIDDEN:** One visual per tiny segment.
- **FORBIDDEN:** One mega-visual for the entire document.

---

### 6.2 Dirty-Rect Invalidation
- **MUST:** Redraw only affected regions.
- **MUST:** Use item AABB (+ padding) to compute dirty rects.
- **SHOULD:** Coalesce invalidations per frame.

---

### 6.3 Theme Boundary
- **FORBIDDEN:** `DynamicResource` lookups in hot render paths.
- **MUST:** Use cached, frozen render resources.
- **MUST:** Invalidate caches on theme change and request redraw.

---

### 6.4 Freezable Discipline
- **MUST:** Freeze pens, brushes, geometries, transforms when possible.
- **MUST:** Check `CanFreeze` before calling `Freeze()`.

---

## 7) Interaction & Tool State Machine (Engine)

### 7.1 Tool Model
- **MUST:** Engine defines tool behavior:
  - Ink
  - Erase (whole-stroke baseline)
  - Select
  - Lasso
  - Move / Resize
  - Pan / Zoom
- **MUST:** App selects tools; Engine executes them.

---

### 7.2 Selection Model
- **MUST:** Selection is a set of `ItemId`.
- **MUST:** Hit-testing uses QuadTree → exact test.
- **SHOULD:** Support multi-select, grouping, bounding boxes, transforms.

---

### 7.3 Input Arbitration
- **MUST:** Stylus → ink / erase  
- **MUST:** Touch → pan / zoom  
- **MUST:** Mouse → selection / manipulation  
- **NOTE:** Threading details are defined in `04_INPUT_INK_THREADING.md`.

---

## 8) View-to-Edit (Mixed Media)

### 8.1 View Mode
- **MUST:** Render text/table/diagram as lightweight visuals.
- **FORBIDDEN:** Heavy WPF controls in view mode.

---

### 8.2 Edit Overlay
- **MUST:** Overlay lives outside the canvas visual tree.
- **MUST:** Position = world rect → camera → screen rect.
- **MUST:** On commit:
  - update domain item
  - recompute AABB
  - update QuadTree
  - regenerate view visual
- **MUST:** On cancel, restore prior state.

---

## 9) Persistence & Undo Hooks (Interfaces Only)

### 9.1 Engine Boundary
- **MUST:** Persisted models are headless.
- **MUST:** Engine exposes command hooks only.
- **NOTE:** Full undo spec lives in `07_UNDO_REDO_COMMANDS.md`.

---

### 9.2 Command Units
- **MUST:** Engine emits undoable commands:
  - AddItem
  - RemoveItem
  - TransformItem
  - EditText
  - CommitStroke
- **MUST:** Support grouping hooks (e.g., time-based ink grouping).

---

## 10) Performance & Memory

### 10.1 Visual Budget
- **MUST:** Visual count scales with viewport, not document size.
- **SHOULD:** Batch dense ink.

---

### 10.2 Allocation Control
- **MUST:** Avoid per-frame allocations in hot paths.
- **SHOULD:** Pool temporary lists and query buffers.

---

### 10.3 Level of Detail (Optional)
- **MAY:** Use LOD for extreme zoom levels.
- **MUST:** LOD never alters domain truth — rendering only.

---

## 11) Testing & Validation

### 11.1 Correctness
- Camera math invariants
- Hit-testing across zoom levels
- Selection / transform + QuadTree consistency

---

### 11.2 Performance
- Stress tests with millions of items
- Metrics:
  - frame time
  - visible set size
  - allocations per frame
  - query time

---

## 12) Anti-Patterns (Hard Stops)
- ItemsControl-based canvas rendering
- Full-scan hit-testing
- Heavy controls in view mode
- Theme lookups in draw loops
- Incremental camera drift

---

## 13) Recommended Defaults

| Key | Default | Notes |
|----|----:|----|
| `ZOOM_MIN` | 0.1 | clamp |
| `ZOOM_MAX` | 50.0 | clamp |
| `CAMERA_CANONICAL_STATE` | enabled | rebuild matrix |
| `RENDER_STRATEGY` | Hybrid | batched ink |
| `QUADTREE_CAPACITY` | 32–64 | benchmark |
| `PAGE_REGIONS_ENABLED` | optional | guide/export |
| `SNAP_ENABLED` | false | extension |

---

## 14) Minimal Headless API Sketch

```csharp
public readonly record struct Vec2(double X, double Y);
public readonly record struct RectD(double MinX, double MinY, double MaxX, double MaxY);

public enum CanvasItemKind {
    Stroke, Text, Table, Image, Diagram, Group, PageRegion
}

public interface ICanvasItem {
    Guid Id { get; }
    CanvasItemKind Kind { get; }
    int LayerId { get; }
    int ZIndex { get; }
    RectD Bounds { get; }
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
    void SetCamera(Vec2 centerWorld, double scale);
    RectD GetWorldViewport();

    void AddItem(ICanvasItem item);
    void RemoveItem(Guid id);
    void UpdateItem(ICanvasItem item);

    void HitTestPoint(Vec2 worldPoint, double tol, List<Guid> results);
}
