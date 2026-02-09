---
description: StylusCore Technical Constitution — 01_CONSTITUTION (v1.2 - Architecture & App Root Sync)
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

### Rule Priority Order
If a rule conflicts, the priority order is:

1) **This constitution (01)**  
2) **Module constitutions (02–08)**  
3) **Implementation details (code)**  

> Module specs MAY specialize rules but MUST NOT violate RED LINES defined here.

---

## 1) Project Pillars (Non-Negotiable)

- **MUST:** The Editor surface is an **Infinite Canvas** (world space), not a document-scroller UI.
- **MUST:** Support **Deep Zoom** and panning via a camera transform.
- **MUST:** Support **mixed media** (ink + text + tables + code + diagrams + images).
- **MUST:** Keep the system **fast at scale** (thousands to millions of items).
- **MUST:** Keep the core logic **headless** and portable across renderers  
  (WPF is one renderer, not the architecture).

---

## 2) Solution / Layering Rules (Dependency Flow)

StylusCore is layered as:

1) **Core (Headless Domain)**  
   - Models, algorithms, interfaces, deterministic logic  

2) **Engine (Runtime Kernel / Renderer Bridge)**  
   - Rendering, input, camera, scene management  
   - **Canonical implementation:** `StylusCore.Engine.Wpf`

3) **App (Shell UI)**  
   - MVVM, navigation, dialogs, toolbars, settings, themes, localization  

4) **Infrastructure (Persistence + Native Adapters)**  
   - SQLite/Protobuf/LZ4, filesystem, OS/device integrations  

### 2.1 Dependency Direction (RED LINE)
- **MUST:** `App -> Engine -> Core`
- **MUST:** Infrastructure is consumed via interfaces defined in Core.
- **FORBIDDEN:** `Core -> Engine` references.
- **FORBIDDEN:** `Core -> App` references.
- **FORBIDDEN:** Any `System.Windows.*` usage inside Core.

---

## 3) Editor Canvas Fundamentals (RED LINES)

- **FORBIDDEN:** `ScrollViewer` on the Editor surface.
- **MUST:** Pan/Zoom via a **camera transform** (`MatrixTransform` or equivalent).
- **MUST:** Zoom constraints:
  - Min scale: `0.1`
  - Max scale: `50.0`
- **MUST:** A “Page” is a **visual guide region** on the infinite canvas  
  (not a hard container; content may overflow).

---

## 4) Ink Input & Threading (RED LINES)

- **MUST:** Use a custom **StylusPlugIn** pipeline for low-latency input.
- **CRITICAL:** StylusPlugIn callbacks run on the **PEN THREAD**.
  - **FORBIDDEN:** Any UI access from the pen thread.
  - **MUST:** Pen thread only collects raw input into a **thread-safe buffer**.
  - **MUST:** UI thread consumes buffer on a frame pump (~16ms).
- **MUST:** Input arbitration:
  - Stylus → ink
  - Touch → pan/zoom
  - Mouse → selection
- **MUST:** Palm rejection:
  - Ignore touch for **300ms** after last stylus event.
  - Pinch zoom pivot = gesture centroid.

---

## 5) Rendering & Performance Kernel (RED LINES)

- **MUST:** Dry ink is retained as **DrawingVisual**.
- **FORBIDDEN:** `WriteableBitmap` for ink rendering.
- **MUST:** Spatial culling via **QuadTree**:
  - **FORBIDDEN:** iterating all items to render
  - Off-screen items MUST NOT exist in the visual tree
- **MUST:** Use dirty-rect invalidation.
- **MUST:** No `{DynamicResource}` lookups inside render loops.
- **MUST:** Freeze Freezables whenever possible.

---

## 6) Mixed Media & View-to-Edit (RED LINES)

- **FORBIDDEN:** Heavy controls permanently on the canvas.
- **MUST:** Use **View-to-Edit**:
  - View: lightweight visuals
  - Edit: overlay real WPF control
  - Exit: commit back to headless model

---

## 7) Persistence & Autosave (RED LINES)

- **MUST:** Persistence is binary (custom or Protobuf).
- **FORBIDDEN:** XML/JSON for large ink data.
- **MUST:** Ink data is **LZ4-compressed**.
- **MUST:** Autosave:
  - Background-only
  - Every **30s** if dirty
  - Must not block UI

---

## 8) Undo/Redo (RED LINES)

- **MUST:** Command pattern for all canvas mutations.
- **MUST:** Undo stack limited to **50 steps**.
- **MUST:** Group rapid ink commits within **500ms**.
- **MUST:** Undo history is not persisted.

---

## 9) UI Shell & XAML Rules (RED LINES)

- **MUST:** UI is contextual.
- **MUST:** Ribbon lives inside `EditorView`.
- **FORBIDDEN:** Ribbon in `MainWindow`.
- **MUST:** Sidebar and Ribbon must not overlap.
- **MUST:** Theme, font, and string usage via `DynamicResource`.
- **FORBIDDEN:** Hardcoded fonts, colors, or user-facing strings.
- **MUST:** Icons are `StreamGeometry` with fit-to-box rules.
- **MUST:** Lists are virtualized; no `ListBox` in `ScrollViewer`.
- **MUST:** Accessibility via `AutomationProperties.Name`.

(See `08_XAML_UI_HIERARCHY.md` for full details.)

---

## 10) App Folder / File Placement (RED LINE — REVISED)

- **MUST:** Feature modules live in `src/StylusCore.App/Features/[FeatureName]/`
- **MUST:** Shared UI components in `src/StylusCore.App/Shared/Components/`
- **MUST:** Shared themes in `src/StylusCore.App/Shared/Themes/`
- **MUST:** Dialogs in `src/StylusCore.App/Dialogs/`

### 10.1 App Root Folder Policy (SYNCED)
- **FORBIDDEN:** Creating ad-hoc, undocumented new root folders under `src/StylusCore.App/`.
- **MUST:** Root folders MUST be one of the approved categories defined in `02_ARCHITECTURE.md`.

> Any new root folder requires updating both **01_CONSTITUTION** and **02_ARCHITECTURE**.

---

## 11) Documentation Map (Read This First)

- **02_ARCHITECTURE.md** — layering, boundaries, solution structure
- **03_CANVAS_ENGINE.md** — infinite canvas, camera, culling
- **04_INPUT_INK_THREADING.md** — stylus pipeline, threading
- **05_RENDERING_PERF_WPF.md** — invalidation, caching, perf
- **06_PERSISTENCE_DB.md** — binary format, autosave
- **07_UNDO_REDO_COMMANDS.md** — command model
- **08_XAML_UI_HIERARCHY.md** — shell UI, tokens, density, dialogs

---

## 12) Change Control (How Rules Evolve)

- **MUST:** Changing a RED LINE requires updating:
  - this constitution (01)
  - the relevant module spec(s)
  - and the implementation
- **SHOULD:** Prefer additive evolution.
- **MUST:** Avoid breaking changes unless strictly necessary.

---

## 13) Quick “Do Not Violate” Checklist

- [ ] No ScrollViewer in Editor
- [ ] Camera-based pan/zoom
- [ ] StylusPlugIn never touches UI
- [ ] DrawingVisual dry ink
- [ ] QuadTree culling mandatory
- [ ] View-to-Edit for heavy controls
- [ ] Binary persistence + LZ4
- [ ] Undo/redo: 50 steps, 500ms grouping
- [ ] DynamicResource for UI; no hardcoding
- [ ] Ribbon inside EditorView only
- [ ] Freeze Freezables
- [ ] Accessibility enforced
