---
description: StylusCore Technical Constitution — 02_ARCHITECTURE (v1.1)
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

2) **Engine (Runtime Kernel / WPF Bridge)**  
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

### 2.1 Canonical Solution Layout (UPDATED)
- `src/StylusCore.Core/` — headless domain
- `src/StylusCore.Engine.Wpf/` — rendering + input bridge (WPF-specific)
- `src/StylusCore.App/` — WPF shell + feature UI
- `src/StylusCore.Infrastructure/` — persistence + adapters (optional separate assembly)

> **NOTE:** `StylusCore.Engine.Wpf` is the canonical Engine implementation.  
> Future renderers (e.g. Skia, DirectX) must live in sibling Engine projects.

---

### 2.2 App Structure (Feature-Based, UPDATED)

Inside `src/StylusCore.App/`:

**Feature Modules**
- `Features/[FeatureName]/Views/`
- `Features/[FeatureName]/ViewModels/`
- `Features/[FeatureName]/Services/` (optional)

**Shell Core (App-only)**
- `Core/ViewModels/` — shell-level view models (MainWindow, navigation state)
- `Core/Services/` — theme, language, navigation, dialog orchestration

**Shared UI**
- `Shared/Components/` — reusable UI components (Header, Sidebar, etc.)
- `Shared/Themes/` — themes, styles, icons, typography, metrics

**Other App-Level Areas**
- `Dialogs/` — modal dialogs and overlays
- `Views/` — shell root views (e.g. MainWindow)
- `Converters/` — XAML value converters
- `Resources/` — non-theme shared resources (language dictionaries, misc)

---

### 2.3 App Root Hygiene (REVISED – CRITICAL)

- **FORBIDDEN:** creating ad-hoc, undocumented folders at the root of `src/StylusCore.App/`.
- **MUST:** all root-level folders be one of the **approved categories** below.

**Approved App Root Folders (Whitelist):**
- `Features/`
- `Core/`
- `Shared/`
- `Dialogs/`
- `Views/`
- `Converters/`
- `Resources/`
- `Services/`

- **MUST:** cross-feature UI lives under `Shared/`.
- **MUST:** feature-specific UI lives under `Features/[Feature]/`.
- **MUST:** dialogs live under `Dialogs/`.

> Any new root folder requires an explicit update to this document.

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
- **MUST:** Stable IDs exist for all objects (type `Guid`, see `03_CANVAS_ENGINE.md` §4).
- **SHOULD:** Domain exposes pure-state mutation APIs invoked via commands (see 07).

---

## 4) Engine Rules (Canvas Engine, Not MVVM)

### 4.1 Anti-MVVM for Canvas (RED LINE)
- **FORBIDDEN:** binding `ObservableCollection<T>` of canvas items to an `ItemsControl`.
- **FORBIDDEN:** per-point ink input routed through ICommand binding.
- **MUST:** Canvas is an **engine**:
  - immediate/wet ink pipeline
  - retained visuals (`DrawingVisual`) for dry content
  - explicit invalidation / dirty-rect redraw

### 4.2 Camera Model (Pan/Zoom)
- **MUST:** Pan/Zoom uses a camera transform (`MatrixTransform` or equivalent).
- **FORBIDDEN:** ScrollViewer for editor navigation.
- **MUST:** Zoom limits: `0.1` to `50.0`.

### 4.3 Spatial Index & Culling
- **MUST:** QuadTree is the source of truth for visibility.
- **FORBIDDEN:** iterating all items to render.
- **MUST:** off-screen items do not exist in the visual tree.

### 4.4 Rendering Contract
- **MUST:** Dry ink is a retained visual (`DrawingVisual`).
- **FORBIDDEN:** `Path` per stroke.
- **FORBIDDEN:** `WriteableBitmap` for ink.
- **MUST:** no `{DynamicResource}` lookups in render loops.

---

## 5) App (Shell UI) Rules

### 5.1 MVVM Where It Belongs
- **MUST:** MVVM is used for shell navigation and panels.
- **SHOULD:** Editor/ViewModel holds:
  - camera state (numeric only)
  - active tool
  - selection state (non-undoable)
- **MUST:** High-frequency rendering/input lives in Engine.

### 5.2 View-to-Edit
- **MUST:** Heavy controls exist only as edit overlays.
- **FORBIDDEN:** permanent DataGrid/RichTextBox on canvas.

---

## 6) Infrastructure Rules

### 6.1 Persistence Boundary
- **MUST:** Core defines persistence interfaces.
- **MUST:** Infrastructure implements them.
- **FORBIDDEN:** App performing document persistence directly.

### 6.2 Background Work
- **MUST:** Autosave uses background immutable snapshots.
- **FORBIDDEN:** blocking UI thread for serialization.

---

## 7) Threading Boundary Rules

### 7.1 Pen Thread Isolation (RED LINE)
- **MUST:** StylusPlugIn runs on pen thread.
- **FORBIDDEN:** UI access from pen thread.
- **MUST:** thread-safe input buffer → UI pump (~16ms).

### 7.2 Mutation Ownership
- **MUST:** mutations on authoritative UI/Domain thread.
- **MUST:** background threads read immutable data only.

---

## 8) Communication Patterns

### 8.1 Core ↔ Engine
- **MUST:** Core returns pure data.
- **MUST:** Engine maps to WPF visuals.

### 8.2 Engine ↔ App
- **MUST:** App controls navigation/tools.
- **MUST:** Engine exposes narrow controller APIs.
- **SHOULD:** low coupling.

### 8.3 Events vs Commands
- **MUST:** mutations via commands.
- **SHOULD:** Engine emits minimal change events.

---

## 9) Placement Rules

### 9.1 Core
- models, algorithms, ports/interfaces

### 9.2 Engine
- camera, visuals, culling, input arbitration, ink pipeline

### 9.3 App
- shell layout, navigation, dialogs, theming, localization, ribbon UI

### 9.4 Shared UI
- reusable components
- styles/templates
- icons/themes

---

## 10) Forbidden Patterns (Hard Stops)
- ItemsControl-based canvas rendering
- ScrollViewer editor navigation
- WPF types in Core
- UI access from pen thread
- heavy controls on canvas
- DynamicResource in render loops
- Path-per-stroke ink
- JSON/XML for large ink documents

---

## 11) “If You’re Working On X, Read Y First”
- Canvas engine: `03_CANVAS_ENGINE.md`, `05_RENDERING_PERF_WPF.md`
- Input/threading: `04_INPUT_INK_THREADING.md`
- Persistence: `06_PERSISTENCE_DB.md`
- Undo/Redo: `07_UNDO_REDO_COMMANDS.md`
- Shell UI: `08_XAML_UI_HIERARCHY.md`
- Master rules: `01_CONSTITUTION.md`

---

## 12) Self-Audit Checklist
- [ ] Core has zero WPF dependencies
- [ ] App → Engine → Core dependency flow
- [ ] Engine-managed canvas
- [ ] QuadTree culling active
- [ ] Pen thread isolated
- [ ] View-to-Edit overlays only
- [ ] Persistence via interfaces
- [ ] Frozen resources in render loops
