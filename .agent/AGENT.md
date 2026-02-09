---
description: StylusCore Agent Entry Point (Routing + Precedence + Output Contract)
---

# 🤖 StylusCore — AGENT.md (Read This First)

This file is the **single entry point** that defines **which rule documents an AI agent** (Antigravity / Cursor / Copilot / etc.) must read **for which type of task**.

> **Usage:** In every prompt → “Read and follow `.agent/AGENT.md`”

---

## 0) Precedence (What wins on conflict?)

1) **`.agent/01_CONSTITUTION.md`** (RED LINES) — highest authority  
2) **`.agent/00_PRODUCT_VISION.md`** (Product / UX Intent) — behavior & UX intent  
3) Task-specific module specifications (02–08)  
4) Best practices / recommendations (only if not contradicted)

If in doubt:
- **Do NOT violate a RED LINE.**
- If there is uncertainty about user behavior or UX decisions,  
  **00_PRODUCT_VISION.md wins** (as long as RED LINES are respected).
- Propose a compliant alternative and explicitly state the conflict.

> **Note:** Module documents (02–08) may **specialize** RED LINES,  
> but MUST NOT weaken or override them.

---

## 1) Mandatory Before Every Task

1. ✅ Read `.agent/01_CONSTITUTION.md` (RED LINES)
2. ✅ If the task affects UX or behavior, read `.agent/00_PRODUCT_VISION.md`
3. ✅ From this file (AGENT.md), select the appropriate **Task Routing** category
4. ✅ Read **all documents listed in that category**
5. ✅ Produce output using the **Output Contract** defined below

---

## 2) Task Routing — “What are you doing?”

Select the closest category. **Do not write code before reading the listed files.**

### A) Product / UX / Interaction Design  
*(Modes, Radial, Text/Voice behavior, user flows)*

Read:
- `.agent/01_CONSTITUTION.md`
- `.agent/00_PRODUCT_VISION.md`
- `.agent/02_ARCHITECTURE.md`
- `.agent/08_XAML_UI_HIERARCHY.md`

> This category defines behavior **before** implementation.

---

### B) Editor / Canvas Engine  
*(Camera, zoom/pan, scene graph, QuadTree, selection)*

Read:
- `.agent/01_CONSTITUTION.md`
- `.agent/02_ARCHITECTURE.md`
- `.agent/03_CANVAS_ENGINE.md`
- `.agent/05_RENDERING_PERF_WPF.md`
- `.agent/00_PRODUCT_VISION.md` (behavior / latency intent)
- `.agent/07_UNDO_REDO_COMMANDS.md` (if adding canvas mutations)

---

### C) Ink / Input / Threading  
*(StylusPlugIn, buffering, wet/dry ink, palm rejection)*

Read:
- `.agent/01_CONSTITUTION.md`
- `.agent/04_INPUT_INK_THREADING.md`
- `.agent/03_CANVAS_ENGINE.md`
- `.agent/05_RENDERING_PERF_WPF.md`
- `.agent/00_PRODUCT_VISION.md` (pen + touch intent)

---

### D) Text / Tables / Mixed Media  
*(View-to-Edit, text formatting, hybrid content)*

Read:
- `.agent/01_CONSTITUTION.md`
- `.agent/00_PRODUCT_VISION.md`
- `.agent/02_ARCHITECTURE.md`
- `.agent/03_CANVAS_ENGINE.md`
- `.agent/05_RENDERING_PERF_WPF.md`
- `.agent/07_UNDO_REDO_COMMANDS.md`

---

### E) Voice / Microphone  
*(STT, inline bars, device handling, settings)*

Read:
- `.agent/01_CONSTITUTION.md`
- `.agent/00_PRODUCT_VISION.md`
- `.agent/02_ARCHITECTURE.md`
- `.agent/06_PERSISTENCE_DB.md` (settings / presets)
- `.agent/07_UNDO_REDO_COMMANDS.md` (voice → text undo/redo)
- `.agent/08_XAML_UI_HIERARCHY.md` (UI + theming)

---

### F) Persistence / DB / File Format  
*(SQLite, LZ4, autosave, recovery, chunking)*

Read:
- `.agent/01_CONSTITUTION.md`
- `.agent/06_PERSISTENCE_DB.md`
- `.agent/02_ARCHITECTURE.md` (interfaces / boundaries)
- `.agent/03_CANVAS_ENGINE.md` (bounds / chunk contracts)
- `.agent/07_UNDO_REDO_COMMANDS.md`
- `.agent/00_PRODUCT_VISION.md` (zero data loss intent)

---

### G) App Shell / Navigation / MVVM / Dialogs

Read:
- `.agent/01_CONSTITUTION.md`
- `.agent/02_ARCHITECTURE.md`
- `.agent/08_XAML_UI_HIERARCHY.md`
- `.agent/00_PRODUCT_VISION.md` (shell intent)

---

### H) XAML / UI  
*(Sidebar, layout, components, theming, icons, localization, accessibility)*

Read:
- `.agent/01_CONSTITUTION.md`
- `.agent/08_XAML_UI_HIERARCHY.md`
- `.agent/02_ARCHITECTURE.md` (folder placement / App root whitelist)
- `.agent/00_PRODUCT_VISION.md` (UI scale / typography intent)
- `.agent/05_RENDERING_PERF_WPF.md` (virtualization / nested scroll bans)

---

### I) Undo / Redo  
*(Command pattern, grouping, atomicity)*

Read:
- `.agent/01_CONSTITUTION.md`
- `.agent/07_UNDO_REDO_COMMANDS.md`
- `.agent/03_CANVAS_ENGINE.md`
- `.agent/05_RENDERING_PERF_WPF.md`
- `.agent/00_PRODUCT_VISION.md` (user expectations / safety)

---

### J) General Refactoring / Code Quality / Architectural Change

Read:
- `.agent/01_CONSTITUTION.md`
- `.agent/02_ARCHITECTURE.md`
- The relevant layer document (03/04/05/06/07/08)
- `.agent/00_PRODUCT_VISION.md` (if UX or behavior is affected)

---

## 3) Document Map (Quick Reference)

| File | Scope |
|-----|------|
| `00_PRODUCT_VISION.md` | Product intent, UX, modes, radial, text/voice, non-goals |
| `01_CONSTITUTION.md` | Master rules, RED LINES, project-wide authority |
| `02_ARCHITECTURE.md` | Layering, dependencies, App root whitelist |
| `03_CANVAS_ENGINE.md` | Infinite canvas, camera, QuadTree, View-to-Edit |
| `04_INPUT_INK_THREADING.md` | StylusPlugIn, pen thread, wet/dry pipeline |
| `05_RENDERING_PERF_WPF.md` | DrawingVisual, dirty rects, virtualization |
| `06_PERSISTENCE_DB.md` | SQLite, LZ4, autosave, chunking, templates |
| `07_UNDO_REDO_COMMANDS.md` | Command pattern, 50-step limit, grouping |
| `08_XAML_UI_HIERARCHY.md` | Shell UI, theming, icons, dialogs, accessibility |

---

## 4) Output Contract (Agent Response Standard)

For every task, the agent MUST respond using this structure:

1. **Documents read:** (explicit list)
2. **Vision alignment:** (check against 00_PRODUCT_VISION — if applicable)
3. **Rule impact:** At least one concrete rule from each document and how it constrained the solution
4. **Plan:** 3–8 clear steps
5. **Files to change:** path list (create / modify)
6. **Risks:** performance / threading / persistence / memory / UX regressions
7. **Tests / Validation:** at least 3 items (metrics or visual checks included)

---

## 5) Hard Stops (Non-Negotiable)

The agent MUST NOT do the following; instead, propose a compliant alternative:

| Forbidden | Reason | Alternative |
|---------|--------|-------------|
| `ScrollViewer` on canvas | Breaks pan/zoom | `MatrixTransform` camera |
| UI access on pen thread | Thread-safety | Buffer → UI thread pump |
| `ItemsControl`-based canvas rendering | Performance | Retained `DrawingVisual` |
| Permanent `DataGrid` / `RichTextBox` on canvas | Memory | View-to-Edit overlay |
| Hardcoded strings / colors / fonts | Theming & i18n | `DynamicResource` |
| Fixed `Width/Height` on icon `Path` | DPI/layout issues | Wrapper + `Stretch` |
| Synchronous I/O on UI thread | Freezes | Background + snapshots |
| `System.Windows.*` in Core | Breaks headless domain | Primitive-only Core |

---

## 6) Reference Integrity (When Files Move)

- **MUST:** `.agent/` is the single source of truth for documentation.
- **MUST:** After any move/rename, search and update old paths in `.agent/*.md`.
- **MUST:** After migrations, produce a **Migration Map**: `old path -> new path`.
- **FORBIDDEN:** Path-less references such as “see above” or “previous doc”.

---

## 7) Version

- **Date:** 2026-02-09
- **Version:** v1.3
- **.NET Target:** 10.0
