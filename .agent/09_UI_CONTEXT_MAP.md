---
description: StylusCore UI Context Map (Modes → Ownership → Visibility → Forbidden Touch)
---

# 09 — UI CONTEXT MAP (Single Source of Truth)

This document prevents “AI drift” and accidental cross-mode edits by defining:
- **Which UI exists in each Mode**
- **Which files own which UI**
- **What MUST NOT be changed from the wrong context**
- **How to handle temporary mismatches between rules and current code (Transition Notes)**

---

## 0) Normative Language
- **MUST**: required
- **MUST NOT / FORBIDDEN**: prohibited
- **SHOULD**: recommended
- **MAY**: optional

---

## 1) Global UI Regions (Always Present in MainWindow)

### 1.1 Window Chrome Header (Global)
**Purpose:** App identity + window controls only.

**MUST include:**
- App icon + app name (“StylusCore”) at top-left
- Window buttons at top-right (Minimize / Maximize / Close)

**MUST NOT include:**
- Editor tools (pen colors, eraser, shape tools)
- Context-specific commands that only make sense in Editor

**Owner:**
- `src/StylusCore.App/Views/MainWindow.xaml`
- `src/StylusCore.App/Shared/Components/HeaderControl.xaml`

### 1.2 Global Sidebar (Always Present)
**Purpose:** Global navigation between **Library / Editor / Settings** contexts.

**MUST:**
- Stay mounted (do not destroy/recreate on every navigation)
- Support collapsed/expanded width behavior
- Be context-aware (items can change per mode, but sidebar itself remains global)

**Specific Visual Elements:**
- Contains the `BookButton` (Visual Tree), but its Visibility is controlled by `MainViewModel` state.

**Owner:**
- `src/StylusCore.App/Shared/Components/Sidebar.xaml`
- Sidebar state comes from `MainViewModel` (or `SidebarViewModel` if separated)

### 1.3 Main Content Host (Mode Switch Area)
**Purpose:** Hosts the active page: LibraryView OR EditorView OR SettingsView

**Owner:**
- `MainWindow.xaml` contains a single content host (`ContentControl` + DataTemplates) OR equivalent navigation host.

**MUST NOT:**
- Duplicate Editor UI controls here if they belong inside EditorView.

---

## 2) Application Modes (Canonical)

StylusCore has three high-level modes:

1) **Library Mode**
2) **Editor Mode**
3) **Settings Mode**

Modes are not just visual — they define boundaries for ownership, input bindings, and safe edits.

---

## 3) Mode Specifications (Visibility + Responsibilities)

### 3.1 Library Mode (Default Entry)
**Purpose:** Notebook management and navigation.

**Visible:**
- Header: YES (global, identity only)
- Sidebar: YES (global)
- Ribbon/Editor Tools: **NO**
- CanvasHostControl: **NO**

**Main Content:**
- `Features/Library/Views/LibraryView.xaml`

**Sidebar behavior in Library Mode:**
- Shows Library navigation + library actions
- MUST NOT show editor-only actions (pen tool toggles, stroke options)

**Owners:**
- `src/StylusCore.App/Features/Library/Views/LibraryView.xaml`
- `src/StylusCore.App/Features/Library/ViewModels/LibraryViewModel.cs`

**FORBIDDEN in Library Mode tasks:**
- Editing `EditorView.xaml`, `CanvasHostControl`, Engine rendering code
- Adding tool buttons to global header
- Moving/adding Ribbon tools anywhere

---

### 3.2 Editor Mode (Active Work)
**Purpose:** Ink + mixed-media editing on infinite canvas.

**Visible:**
- Header: YES (global identity only)
- Sidebar: YES (global)
- Ribbon/Editor Tools: **YES (Editor context only)**
- CanvasHostControl: **YES (Editor only)**

**Main Content:**
- `Features/Editor/Views/EditorView.xaml`

**Input Context (Invisible but Critical):**
- **MUST:** Active Input Mode must switch to `GraphicsTablet` or `Editor` bindings via `BindingManager`.
- Input events flow through `StylusCore.Engine.Wpf/Input/*` stack.

**Ribbon rule (Target Architecture):**
- **MUST:** Functional Ribbon (pen tools) lives inside `EditorView.xaml`
- **FORBIDDEN:** Ribbon as a global control in `MainWindow`

**Transition Note (Legacy Handling):**
- **CURRENT STATUS:** Ribbon is currently hosted in `MainWindow.xaml` (`EditorRibbonContainer`).
- **RULE:** Any task that modifies the Ribbon MUST explicitly check if it's a refactor task.
    - If Refactor: Move Ribbon to `EditorView.xaml`.
    - If Feature: You MAY modify `MainWindow` resources temporarily, but MUST flag it as "Legacy Location".
- Never “half move” the Ribbon.

**Editor Sidebar behavior:**
- **ALLOWED:** Navigation items (e.g., "Back to Library", "Table of Contents", "Export").
- **STRICTLY FORBIDDEN:** **Canvas Manipulation Tools** (Pen, Eraser, Colors, Lasso) inside Sidebar. These MUST live in the Ribbon/Toolbar.

**Owners:**
- `src/StylusCore.App/Features/Editor/Views/EditorView.xaml`
- `src/StylusCore.App/Features/Editor/ViewModels/EditorViewModel.cs`
- Canvas engine host control (WPF bridge):
  - `src/StylusCore.Engine.Wpf/Controls/CanvasHostControl.xaml` (+ code-behind if needed)

**FORBIDDEN in Editor UI tasks:**
- Rendering canvas items via ItemsControl/ObservableCollection binding
- Adding heavy WPF controls permanently on canvas surface (View-to-Edit only)
- Touching Core with System.Windows types

---

### 3.3 Settings Mode
**Purpose:** All user configuration (keybinds, radial menu setup, preferences).

**Visible:**
- Header: YES (global identity only)
- Sidebar: YES (global, still needed for mode switching)
- Ribbon/Editor Tools: **NO**
- CanvasHostControl: **NO**

**Main Content:**
- `Features/Settings/Views/SettingsView.xaml`

**Settings layout requirement (VS Code–style):**
- SettingsView MUST be a 2-column layout:
  - Left: Settings categories navigation (tree/list)
  - Right: Selected category detail panel
- Left categories are **inside SettingsView**, NOT the global Sidebar.

**Sidebar behavior in Settings Mode:**
- Sidebar stays global for top-level navigation.
- It MUST NOT duplicate the internal settings category navigation.

**Owners:**
- `src/StylusCore.App/Features/Settings/Views/SettingsView.xaml`
- `src/StylusCore.App/Features/Settings/ViewModels/SettingsViewModel.cs`

**FORBIDDEN in Settings tasks:**
- Editing Editor Ribbon or CanvasHostControl
- Changing Library lists
- Adding editor tool controls into SettingsView

---

## 4) Ownership Map (Single Source of Truth)

### 4.1 Global Shell
- `MainWindow.xaml`: owns layout regions only (Header region, Sidebar region, Content host)
- `HeaderControl.xaml`: owns app identity + window controls only
- `Sidebar.xaml`: owns global navigation only (Library/Editor/Settings)

### 4.2 Features
- Library UI lives only under `Features/Library/*`
- Editor UI lives only under `Features/Editor/*`
- Settings UI lives only under `Features/Settings/*`

### 4.3 Engine Surface
- Canvas rendering and input bridge lives in `StylusCore.Engine.Wpf`
- App must not implement canvas rendering using XAML ItemsControl patterns

### 4.4 Dialogs & Windows (Physical Location)
**Purpose:** Modal interactions (e.g., creating notebooks, picking colors).
**Location:** MUST live in `src/StylusCore.App/Dialogs/`.
**Current Files:**
- `CreateNotebookDialog.xaml` (Window)
- `ColorPickerDialog.xaml` (Window)
- `InputDialog.cs` (Code-only dialog helper)
**Rule:** Do not create ad-hoc grids in Views for complex dialogs; use the `Dialogs/` folder, triggered via Services.

---

## 5) Forbidden Cross-Mode Edits (AI Safety Rules)

When the task is scoped to a mode:

### 5.1 Library Task MUST NOT touch:
- `EditorView.xaml`, `RibbonToolbar`, `CanvasHostControl`
- Engine rendering/input code

### 5.2 Editor Task MUST NOT touch:
- `LibraryView.xaml` and library templates
- Settings view/category system

### 5.3 Settings Task MUST NOT touch:
- `EditorView.xaml`, Ribbon, CanvasHostControl
- Library views

### 5.4 Safe Harbor (Global Resources)
Even if working in a specific mode (e.g., Library), the following "Global" files MAY be edited safely to support the feature:
- `src/StylusCore.App/Services/ThemeService.cs` (Theming logic)
- `src/StylusCore.App/Services/LanguageService.cs` (Localization)
- `src/StylusCore.App/Resources/` (String dictionaries)
- `src/StylusCore.App/Shared/Themes/` (Colors, Brushes, Icons)
- `src/StylusCore.Core/` (Domain logic is mode-agnostic)

---

## 6) Prompt Guardrails (Required for AI Work)

Every change request MUST include:

1) **Mode:** Library | Editor | Settings
2) **Goal:** one sentence
3) **Allowed files (whitelist):** max 3–8 files
4) **Forbidden files:** list the mode’s forbidden set
5) **Acceptance checks:** at least 3

If the change requires moving UI between owners (e.g., Ribbon MainWindow → EditorView),
the task MUST be treated as a dedicated refactor and not mixed into other work.

---
## 7) Enforcement: Forbidden Code Patterns (Per Owner)

This section is a hard guardrail. If any forbidden pattern is present, it is a **violation**.
Agents MUST NOT “work around” violations silently.

### 7.1 MainWindow.xaml (Shell Layout Only)
**MainWindow MUST contain ONLY:**
- Header region host
- Sidebar region host
- Main content host (Library/Editor/Settings)
- Optional global overlay host region (dialogs/toasts) that is **not editor-tool-specific**

**FORBIDDEN in MainWindow.xaml:**
- Any Editor tool UI: `RibbonToolbar`, pen color pickers, tool selectors, stroke width UI
- Any canvas surface control: `CanvasHostControl`, drawing-surface visuals
- Any ItemsControl-style rendering of canvas content

**Exception (Legacy):** `EditorRibbonContainer` currently exists here. See Transition Note in Section 3.2.

### 7.2 Features/Library/* (Library Mode Only)
**Library feature MUST contain ONLY library UI:**
- notebook list/grid
- create/rename/delete notebook dialogs/commands
- library search/filter

**FORBIDDEN in Library feature:**
- Any editor tool UI (Ribbon)
- Any canvas host or engine surface
- Any editor-only sidebar items (sections/pages)

### 7.3 Features/Editor/* (Editor Mode Only)
**Editor feature MUST contain:**
- Contextual Ribbon (functional tools)
- Sections/pages panel region (optional)
- Canvas host surface (engine control)

**FORBIDDEN in Editor feature:**
- Library-only UI (notebook cover grid, create notebook entry workflow)
- Settings category tree (settings nav belongs to SettingsView)

### 7.4 Features/Settings/* (Settings Mode Only)
**Settings MUST contain:**
- Internal settings navigation (left categories list/tree)
- Right-side details panel
- Search within settings (optional)

**FORBIDDEN in Settings feature:**
- Editor Ribbon
- CanvasHostControl
- Library notebook shelf UI

---

## 8) If-Detected Protocol (What the Agent MUST Do)

When an agent discovers any forbidden pattern:

1) **STOP feature work immediately**
2) **Report the violation** with:
   - file path
   - exact element names (e.g., `EditorRibbonContainer`)
   - why it violates this doc
3) Choose ONE of these outcomes:

### Outcome A — Refactor Required (Default)
- Create a dedicated refactor plan:
  - Move ownership to correct file
  - Keep UI identical (no visual redesign during refactor)
  - Update bindings + DataContext wiring
  - Remove legacy container after migration

### Outcome B — Temporary Exception (Only with explicit user approval)
- Only if the user explicitly says “keep legacy for now”.
- Agent must add a **TRANSITION NOTE** section at the top of the relevant spec.

**FORBIDDEN:**
- Silent exceptions
- “Fixing” by duplicating UI (Ribbon both in MainWindow and EditorView)

---

## 9) Refactor Ticket Template (Required)

**Title:** Move [Component] to [Correct Owner]  
**Goal:** align code with 08/09 ownership rules without changing behavior  
**Steps:**
1) Identify current hosting location and bindings
2) Create target host in correct view (EditorView/SettingsView/etc.)
3) Rewire DataContext (explicit binding / templated control)
4) Remove old host container and cleanup resources
5) Verify modes: Library hides, Editor shows, Settings hides (as applicable)
**Acceptance Checks:**
- No duplicates
- No regressions in navigation
- Visual layout unchanged