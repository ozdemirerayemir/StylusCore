---
description: StylusCore Technical Constitution — 02_ARCHITECTURE (v1.0)
---

# 🏗️ 02 — ARCHITECTURE (Solution Layers, Boundaries, Dependency Rules)

This document defines the **canonical architecture** of StylusCore: how the solution is layered, where code belongs, how modules communicate, and which patterns are **FORBIDDEN**.

**Goal:** Build a stylus-first **Infinite Canvas Engine** (deep zoom, QuadTree culling, DrawingVisual rendering) with a **headless domain** and a WPF shell—without architectural drift.

---

## 0) Normative Language
- **MUST**: required
- **MUST NOT / FORBIDDEN**: prohibited
- **SHOULD**: recommended
- **MAY**: optional

---

## 1) The Four-Layer Model (Canonical)

StylusCore is composed of four major layers:

1) **Core (Headless Domain)**  
   - Pure models, algorithms, interfaces, deterministic state changes  
2) **Engine (Runtime Kernel)**  
   - Bridges domain to WPF: rendering, input, camera, scene management  
3) **App (Shell UI)**  
   - MVVM, navigation, dialogs, toolbars, settings, theming, localization  
4) **Infrastructure (Persistence + Native Adapters)**  
   - SQLite/Protobuf/LZ4, filesystem, OS/device integrations, native APIs  

### 1.1 Dependency Flow (RED LINE)
- **MUST:** `App -> Engine -> Core`
- **MUST:** `Infrastructure` is consumed via **interfaces** (defined in Core).
- **FORBIDDEN:** `Core -> Engine` references.
- **FORBIDDEN:** `Core -> App` references.
- **FORBIDDEN:** any `System.Windows.*` usage inside **Core**.

---

## 2) Project Structure (Repo Layout)

### 2.1 Canonical Solution Layout
- `src/StylusCore.Core/` — headless domain
- `src/StylusCore.Engine/` — rendering + input bridge
- `src/StylusCore.App/` — WPF shell + feature UI
- `src/StylusCore.Infrastructure/` — persistence + adapters (if separate assembly; optional)

### 2.2 App Structure (Feature-Based)
Inside `src/StylusCore.App/`:
- `Features/[FeatureName]/Views/`
- `Features/[FeatureName]/ViewModels/`
- `Core/Models/` and `Core/Services/` (App-only “shell core”, not the headless domain)
- `Shared/Components/`
- `Shared/Themes/`
- `Dialogs/`
- `Views/` (shell root views like MainWindow)

### 2.3 Root Hygiene (RED LINE)
- **FORBIDDEN:** creating new folders at the root of `src/StylusCore.App/`.
- **MUST:** place cross-feature UI pieces under `Shared/`.
- **MUST:** place dialogs under `Dialogs/`.

---

## 3) Headless Domain Rules (Core)

### 3.1 Domain Type Rules
- **MUST:** Domain models are POCO/struct-based and use primitives.
- **FORBIDDEN:** WPF types and concepts in Core:
  - `Point`, `Rect`, `Brush`, `Color`, `DependencyObject`, `UIElement`, `DrawingVisual`
- **MUST:** Use primitives:
  - Coordinates: `double x, y`
  - Color: `uint argb` (or `string hex`) in domain payloads
  - Geometry: point arrays, simple AABB structs

### 3.2 Determinism
- **MUST:** Domain operations are deterministic for the same inputs.
- **MUST:** Stable IDs exist for all objects (recommended `ulong` monotonic).
- **SHOULD:** Domain exposes pure-state mutation APIs invoked via commands (see 07).

---

## 4) Engine Rules (Canvas Engine, Not MVVM)

### 4.1 Anti-MVVM for Canvas (RED LINE)
- **FORBIDDEN:** binding `ObservableCollection<T>` of canvas items to an `ItemsControl` for rendering.
- **FORBIDDEN:** per-point ink input routed through ICommand binding on every move.
- **MUST:** Canvas is an **engine**:
  - immediate/wet ink pipeline for latency
  - retained visuals (DrawingVisual) for dry content
  - explicit invalidation / dirty-rect redraw

### 4.2 Camera Model (Pan/Zoom)
- **MUST:** Pan/Zoom uses a camera transform (`MatrixTransform` or equivalent).
- **FORBIDDEN:** ScrollViewer for editor navigation.
- **MUST:** Zoom limits: `0.1` to `50.0`.

### 4.3 Spatial Index & Culling
- **MUST:** QuadTree is the source of truth for what is visible.
- **FORBIDDEN:** iterating all items to render.
- **MUST:** off-screen items do not exist in the visual tree.

### 4.4 Rendering Contract
- **MUST:** Dry ink is a `DrawingVisual` (or equivalent retained visual).
- **FORBIDDEN:** `Path` per stroke (visual tree blow-up).
- **FORBIDDEN:** `WriteableBitmap` for ink (deep zoom blur).
- **MUST:** no `{DynamicResource}` lookups in render loops (use cached frozen pens/brushes).

---

## 5) App (Shell UI) Rules

### 5.1 MVVM Where It Belongs
- **MUST:** MVVM is used for shell navigation and panels (Library/Settings).
- **SHOULD:** CanvasViewModel holds high-level state:
  - camera state (numbers only)
  - active tool
  - selection state (not undoable by default)
- **MUST:** High-frequency rendering/input is handled in Engine, not in ViewModel bindings.

### 5.2 View-to-Edit
- **MUST:** Heavy controls are only created as edit overlays.
- **FORBIDDEN:** DataGrid/RichTextBox living permanently on the canvas surface.

---

## 6) Infrastructure Rules (Persistence + Native)

### 6.1 Persistence Interface Boundary
- **MUST:** Core defines interfaces (ports) such as:
  - `IDocumentStore`, `IChunkStore`, `IAutosaveService`
- **MUST:** Infrastructure implements them using SQLite/Protobuf/LZ4.
- **FORBIDDEN:** App directly doing persistence IO for document state.

### 6.2 Background Work
- **MUST:** Autosave runs in background and uses immutable snapshots.
- **FORBIDDEN:** blocking UI thread for serialization.

---

## 7) Threading Boundary Rules (Cross-Cutting)

### 7.1 Pen Thread Isolation (RED LINE)
- **MUST:** StylusPlugIn callbacks run on the pen thread.
- **FORBIDDEN:** any UI access from pen thread.
- **MUST:** pen thread writes raw input into a thread-safe buffer.
- **MUST:** UI thread consumes buffer at ~16ms to update visuals.

### 7.2 Ownership of Mutations
- **MUST:** Document mutations occur on the authoritative UI/Domain thread.
- **MUST:** background threads operate on immutable snapshots only.

---

## 8) Communication Patterns (How Layers Talk)

### 8.1 Core ↔ Engine
- **MUST:** Core exposes pure APIs and returns pure data (ChangeSets, bounds, IDs).
- **MUST:** Engine converts domain data into WPF visuals.

### 8.2 Engine ↔ App
- **MUST:** App controls high-level navigation and tool selection.
- **MUST:** Engine exposes a narrow surface:
  - `ICanvasHost` / `ICanvasController` style API
  - events for “content changed” / “selection changed”
- **SHOULD:** Keep cross-layer coupling low.

### 8.3 Events vs Commands
- **MUST:** Content mutation flows through commands (07_UNDO_REDO).
- **SHOULD:** Engine emits events with:
  - changed object IDs
  - world-space AABB dirty hints
  - “needs redraw” triggers

---

## 9) Placement Rules (Where Code Goes)

### 9.1 Core
Put here:
- models: StrokeModel, TextModel, TableModel, LayerModel, PageRegionModel (headless)
- algorithms: QuadTree, simplification (RDP), chunking decisions (interfaces)
- interfaces/ports: stores, serialization abstractions

### 9.2 Engine
Put here:
- camera + matrix transforms
- drawing visuals, visual pooling
- culling + invalidation logic
- input manager (stylus/touch/mouse arbitration)
- wet/dry ink pipeline integration

### 9.3 App
Put here:
- MainWindow shell layout (header/sidebar)
- navigation between Library/Editor/Settings
- theme & localization dictionary swapping
- dialogs (create notebook, input dialog)
- toolbars/ribbon UI

### 9.4 Shared UI
Put here:
- reusable controls (Sidebar, HeaderControl)
- styles/templates
- icons/themes resource dictionaries

---

## 10) Forbidden Patterns (Hard Stops)
- **FORBIDDEN:** ItemsControl-based canvas rendering with ObservableCollection binding.
- **FORBIDDEN:** ScrollViewer as editor navigation.
- **FORBIDDEN:** WPF types in Core domain.
- **FORBIDDEN:** touching UI from pen thread.
- **FORBIDDEN:** heavy controls permanently on canvas (no View-to-Edit).
- **FORBIDDEN:** `{DynamicResource}` lookups inside render loops.
- **FORBIDDEN:** thousands of `Path` elements for ink strokes.
- **FORBIDDEN:** JSON/XML as primary document format for large ink.

---

## 11) “If You’re Working On X, Read Y First”
- **Canvas engine / camera / culling:** `03_CANVAS_ENGINE.md`, `05_RENDERING_PERF_WPF.md`
- **Stylus input / threading / wet-dry:** `04_INPUT_INK_THREADING.md`
- **Persistence / autosave / DB:** `06_PERSISTENCE_DB.md`
- **Undo/Redo & commands:** `07_UNDO_REDO_COMMANDS.md`
- **Shell UI / XAML system:** `08_XAML_UI_HIERARCHY.md`
- **Master guardrails:** `01_CONSTITUTION.md`

---

## 12) Self-Audit Checklist (Architecture)
- [ ] Core contains no WPF dependencies
- [ ] Dependency flow is App → Engine → Core
- [ ] Canvas is engine-managed (not ItemsControl bindings)
- [ ] QuadTree culling exists and is mandatory
- [ ] StylusPlugIn pen thread is isolated; no UI access
- [ ] View-to-Edit overlays used for heavy controls
- [ ] Persistence is behind interfaces; autosave is background snapshot
- [ ] Render loops use cached frozen resources (no DynamicResource lookups)

---
