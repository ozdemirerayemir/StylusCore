---
description: StylusCore Technical Constitution — 01_CONSTITUTION (v1.0)
---

# 📜 StylusCore Technical Constitution (Master Index)

This document is the **single source of truth** for StylusCore engineering rules.  
All contributors and all AI agents MUST follow this constitution and the referenced module specs.

**Goal:** Build a stylus-first, high-performance **Infinite Canvas Engine** (OneNote/Miro-class core) where Ink, Text, Tables, Code Blocks, Images, and Diagrams coexist with **Deep Zoom**.

---

## 0) Normative Language
- **MUST**: required
- **MUST NOT / FORBIDDEN**: prohibited
- **SHOULD**: recommended
- **MAY**: optional

If a rule conflicts, the priority order is:
1) **This constitution (01)**  
2) Module constitutions (03–08)  
3) Implementation details

---

## 1) Project Pillars (Non-Negotiable)
- **MUST:** The Editor surface is an **Infinite Canvas** (world space), not a document-scroller UI.
- **MUST:** Support **Deep Zoom** and panning via a camera transform.
- **MUST:** Support **mixed media** (ink + text + tables + code + diagrams + images).
- **MUST:** Keep the system **fast at scale** (thousands to millions of items).
- **MUST:** Keep the core logic **headless** and portable across renderers (WPF is only one renderer).

---

## 2) Solution / Layering Rules (Dependency Flow)
StylusCore is layered as:

1) **Core (Headless Domain)**
   - Models, algorithms, interfaces
2) **Engine (Render/Input Bridge)**
   - WPF visuals, camera, input managers, scene integration
3) **App (Shell UI)**
   - MVVM, navigation, dialogs, toolbars, settings, themes
4) **Infrastructure (Persistence + Native Adapters)**
   - SQLite/Protobuf/LZ4, filesystem, device integrations

### 2.1 Dependency Direction (RED LINE)
- **MUST:** `App -> Engine -> Core`
- **MUST:** Infrastructure is consumed by Core/Engine through interfaces.
- **FORBIDDEN:** Core referencing WPF/App (`System.Windows.*`, `DependencyObject`, `UIElement`).

---

## 3) Editor Canvas Fundamentals (RED LINES)
- **FORBIDDEN:** `ScrollViewer` on the Editor surface.
- **MUST:** Pan/Zoom via **MatrixTransform camera** (or equivalent transform pipeline).
- **MUST:** Zoom constraints:
  - **Min scale:** `0.1`
  - **Max scale:** `50.0`
- **MUST:** A “Page” is a **visual region** guide on the infinite canvas (not a hard container). Content may overflow.

---

## 4) Ink Input & Threading (RED LINES)
- **MUST:** Use a custom **StylusPlugIn** pipeline for low-latency input.
- **CRITICAL:** StylusPlugIn callbacks run on the **PEN THREAD**.
  - **FORBIDDEN:** Any UI access from the pen thread (DependencyProperty, Brushes, Visuals, Dispatcher-bound objects).
  - **MUST:** Pen thread only collects raw points into a **thread-safe buffer**.
  - **MUST:** UI thread consumes buffer on a frame pump (~16ms) to update visuals.
- **MUST:** Input arbitration:
  - Stylus = ink
  - Touch = pan/zoom
  - Mouse = selection
- **MUST:** Palm rejection:
  - ignore touch for **300ms** after last stylus event
  - pinch zoom pivot must be gesture centroid (not origin)

---

## 5) Rendering & Performance Kernel (RED LINES)
- **MUST:** Dry ink is retained as **DrawingVisual** (not thousands of Path objects).
- **FORBIDDEN:** `WriteableBitmap` for ink rendering (blurs on zoom).
- **MUST:** Spatial culling uses **QuadTree**:
  - **FORBIDDEN:** iterating all items to render
  - items off-screen MUST NOT exist in the visual tree
- **MUST:** Use **dirty-rect invalidation** (do not redraw everything for small changes).
- **MUST:** No `{DynamicResource}` lookups inside render loops:
  - Engine uses cached, frozen resources (Brush/Pen cache) and invalidates cache on theme change.
- **MUST:** Freeze Freezables where possible:
  - geometries, pens, brushes, transforms
  - `if (obj.CanFreeze) obj.Freeze()`

---

## 6) Mixed Media & View-to-Edit (RED LINES)
- **FORBIDDEN:** Heavy controls on the canvas in view mode (`DataGrid`, `RichTextBox`, etc.).
- **MUST:** Use **View-to-Edit**:
  - View mode: lightweight visuals (`DrawingVisual`, minimal TextBlock)
  - Edit mode: overlay the real WPF control only while editing
  - Exit: commit back to headless model + lightweight visual

---

## 7) Persistence & Autosave (RED LINES)
- **MUST:** Persistence is **binary** (custom binary or Protobuf).  
  - **FORBIDDEN:** XML/JSON for large ink data.
- **MUST:** Ink blocks are **LZ4-compressed**.
- **MUST:** Autosave is background-only:
  - every **30 seconds** if DirtyFlag is set
  - MUST NOT block UI
- **SHOULD:** Use spatial chunking and viewport-based loading for large notebooks.

---

## 8) Undo/Redo (RED LINES)
- **MUST:** Command pattern for all canvas operations.
- **MUST:** Limit undo stack to **50 steps**.
- **MUST:** Group rapid ink commits within **500ms** into one undo unit.
- **MUST:** Undo history is NOT persisted; crash recovery uses autosaved snapshots.

---

## 9) UI Shell & XAML Rules (RED LINES)
### 9.1 Shell Geometry
- **MUST:** MainWindow layout is grid-based:
  - Header height: **48px**
  - Sidebar widths: collapsed **48px**, expanded **240px** (fixed)
  - Content area uses remaining `*`

### 9.2 Themes, Fonts, Strings
- **MUST:** Colors via `DynamicResource` keys from shared dictionaries.
- **FORBIDDEN:** hardcoded `FontFamily` → use `DynamicResource App.MainFont`
- **FORBIDDEN:** hardcoded UI strings → use runtime-switchable resources.

### 9.3 Icons (CLIP PROTECTION)
- **MUST:** Icons are vector `StreamGeometry` (path data) in `Shared/Themes/Icons.xaml`.
- **FORBIDDEN:** setting explicit `Width/Height` on the `Path` itself.
- **MUST:** Parent container defines size; Path uses `Stretch="Uniform"` and centered alignment.

### 9.4 Virtualization & Layout
- **MUST:** Lists use virtualization (`VirtualizationMode="Recycling"`).
- **FORBIDDEN:** `ListBox` inside `ScrollViewer` (kills virtualization).
- **SHOULD:** Keep visual tree shallow; avoid unnecessary nested containers.

### 9.5 Accessibility
- **MUST:** `AutomationProperties.Name` on all interactive controls.
- **MUST:** Keyboard navigation supported (Tab/Arrow).
- **MUST:** Ensure visibility in Windows High Contrast mode.

---

## 10) Folder / File Placement (RED LINE)
- **MUST:** Feature modules live in `src/StylusCore.App/Features/[FeatureName]/`
- **MUST:** Shared UI components in `src/StylusCore.App/Shared/Components/`
- **MUST:** Dialogs in `src/StylusCore.App/Dialogs/`
- **MUST:** Shared themes in `src/StylusCore.App/Shared/Themes/`
- **FORBIDDEN:** Creating new folders at the root of `src/StylusCore.App/`

---

## 11) Documentation Map (Read This First)
Use these documents as the authoritative specs:

- **02_ARCHITECTURE.md** — Layering, boundaries, solution structure, feature organization
- **03_CANVAS_ENGINE.md** — Infinite canvas engine, camera, scene model, spatial culling
- **04_INPUT_INK_THREADING.md** — StylusPlugIn, buffering, wet/dry pipeline, gestures
- **05_RENDERING_PERF_WPF.md** — frame pump, invalidation, caching, text/image perf
- **06_PERSISTENCE_DB.md** — binary format, chunking, SQLite, crash safety, autosave
- **07_UNDO_REDO_COMMANDS.md** — commands, grouping, atomicity, tests
- **08_XAML_UI_HIERARCHY.md** — shell UI, tokens, density, templates, dialogs, a11y

---

## 12) Change Control (How Rules Evolve)
- **MUST:** Any change to a RED LINE requires updating:
  - this constitution (01)
  - the relevant module spec(s) (03–08)
  - and the implementation in Core/Engine/App
- **SHOULD:** When changing persistence schemas, include:
  - versioning/migrations plan
  - backward/forward compatibility strategy
- **MUST:** Prefer additive evolution; avoid breaking changes unless strictly necessary.

---

## 13) Quick “Do Not Violate” Checklist
- [ ] No ScrollViewer in Editor; camera uses MatrixTransform
- [ ] StylusPlugIn never touches UI; buffer to UI thread at ~16ms
- [ ] Dry ink is DrawingVisual, not Path/WriteableBitmap
- [ ] QuadTree culling is mandatory
- [ ] View-to-Edit: heavy controls only in edit overlay
- [ ] Binary persistence + LZ4; autosave every 30s background
- [ ] Undo/redo command pattern; 50 steps; 500ms ink grouping
- [ ] DynamicResource for theme/strings; icons never clipped; virtualization preserved
- [ ] Freeze Freezables; keep visual tree shallow
- [ ] AutomationProperties.Name everywhere

---
