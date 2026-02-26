---
description: StylusCore Golden Path & Decision Tree — 00_GOLDEN_PATH (v1.0 - Architecture Compiler Initiative)
---

# 🧭 00 — GOLDEN PATH & DECISION TREE
*(The "One True Way" to extend StylusCore)*

This document is the **Operational Cheat Sheet** for StylusCore.  
Do not read the entire constitution when adding a new feature. Use this tree.
If your feature does not fit this tree, you are likely breaking the architecture.

**Goal:** Zero decision fatigue. Architecture as a Default, not a Decision.

---

## 1) The Decision Tree: "Where does my code go?"

**Q1: Is it a new Canvas Tool (e.g., Pen, AI Gen, Shape, Lasso)?**
- **YES** ➔ It is a **Tool**.
  - **Path:** `src/StylusCore.App/Features/[ToolName]/`
  - **Rules:**
    - MUST implement `ITool` (or equivalent Tool Lifecycle Contract).
    - MUST only produce `ICommand`s.
    - **FORBIDDEN:** Direct rendering. Tools mutate `DocumentState` via Commands.

**Q2: Is it changing the data on the canvas (e.g., adding text, removing stroke)?**
- **YES** ➔ It is a **Command**.
  - **Path:** `src/StylusCore.Core/Commands/` (or Feature-specific Domain context)
  - **Rules:**
    - MUST implement `ICommand`.
    - MUST mutate the headless `DocumentState`.
    - MUST be undoable/redoable.
    - **FORBIDDEN:** UI namespaces (`System.Windows.*`).

**Q3: Is it a new Shell UI element (Toolbar, Panel, Settings, Dialog)?**
- **YES** ➔ It is **App Shell UI**.
  - **Path:** `src/StylusCore.App/Features/[FeatureName]/`
  - **Rules:**
    - MUST use `{DynamicResource}` for sizes/colors/fonts.
    - **FORBIDDEN:** Hardcoded `Margin`, `Width`, `Color`.
    - MUST follow MVVM (View + ViewModel).

**Q4: Is it a new way to render Canvas Data (e.g., drawing shapes, text blocks)?**
- **YES** ➔ It is **Engine Rendering**.
  - **Path:** `src/StylusCore.Engine.Wpf/Renderers/`
  - **Rules:**
    - MUST read from Headless `DocumentState`.
    - MUST use `DrawingVisual` or Spatial QuadTree chunks.
    - **FORBIDDEN:** `UIElement` for thousands of items.

**Q5: Is it a heavy WPF Control needed for editing (e.g., TextBox, DataGrid)?**
- **YES** ➔ It is a **View-To-Edit Overlay**.
  - **Path:** `src/StylusCore.Engine.Wpf/Overlays/` (or App layer depending on exact View-To-Edit boundary)
  - **Rules:**
    - MUST float above the canvas in screen-space.
    - On Commit ➔ Produce `ICommand` ➔ Destroy/Hide control ➔ Engine renders lightweight representation.

---

## 2) The "One Door" API (Enforced Boundaries)

To prevent the "God Object" anti-pattern, cross-layer communication MUST use these single entry points:

- **UI to Document:** Via **CommandBus** (Dispatching `ICommand`).
- **Tool to Document:** Via **CommandBus**.
- **Engine to Document:** Read-only observers (Rendering what the DocumentState dictates).
- **Core to Ext:** Core calls **Infrastructure Interfaces** (e.g., `IStorageProvider`), defined in Core but implemented in Infrastructure.

---

## 3) The Future: Architecture as Tests (ArchTests)
*(This section outlines the upcoming automated enforcement phase)*

The rules in the `.agent/` folder will be transformed into automated tests that break the build if violated:
1. `Core_Cannot_Reference_App_Or_Engine`
2. `Core_Cannot_Use_System_Windows`
3. `XAML_Cannot_Use_Hardcoded_Sizes_Or_Colors`
4. `Editor_Cannot_Contain_ScrollViewer`

If your code breaks the build, do not bypass the test. Fix your architecture.

---

## 4) Feature Blueprint (The Template)
When adding a new feature `[X]`, the target structure is always:

```text
Feature_[X]/
├── Views/          (XAML + Codebehind -> UI Shell only)
├── ViewModels/     (State + Command Binding)
├── Tools/          (If it interacts with canvas -> ITool adapter)
└── Commands/       (If it mutates canvas -> ICommand logic)
```
*Do not invent new folder structures at the root level.*
