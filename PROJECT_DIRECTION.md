# Project Direction: StylusCore

This document defines the long-term direction, concept, and structural philosophy of the application.  
It is not a technical specification or a rule file, but a structured guide to the project's vision, interaction model, and evolution.

---

## 1. Project Vision & Concept

StylusCore is designed as a hybrid creative workspace that bridges structured documentation and freeform expression.

It is not a simple note-taking application.  
It is a unified editing environment where structured text, free text, ink, shapes, and future object types coexist on the same canvas — without interfering with each other.

The system prioritizes:

- Deterministic behavior
- Explicit tool switching
- User-controlled state transitions
- Modular and expandable architecture

StylusCore aims to solve the rigidity of traditional editors by offering flexibility without sacrificing structural clarity.

---

## 2. Current Application Structure

The application is built on a modular hierarchical architecture:

- **Library Layer**  
  Manages collections, books, and content containers.

- **Book / Content Selection Layer**  
  Organizes and navigates notebooks, sections, and pages.

- **Editor Layer**  
  The core workspace where content creation and manipulation occur.

- **UI Layers**
  - Header
  - Ribbon
  - Sidebar
  - Canvas
  - Panels

Each layer is intentionally isolated and extensible, allowing independent evolution without structural collapse.

---

## 3. Editor Philosophy

The editor operates on clear principles:

- **Deterministic Behavior**  
  No hidden automation. Every action has a visible and predictable result.

- **Explicit State Transitions**  
  Tool and mode changes must always be user-driven and visually clear.

- **Object-Type Separation**  
  Text, ink, shapes, and future media types are independent systems.

- **Expandable Interaction Model**  
  New tools and interaction patterns must integrate without breaking existing logic.

- **Resource-Driven UI System**  
  No hardcoded styling. All visuals derive from centralized resources.

---

# 4. Interaction & Tool System Architecture

StylusCore is a multi-modal editor built around explicit tool control.

---

## 4.1 Core Interaction Modes

The editor operates through clearly defined modes:

- Navigation Mode
- Ink Mode
- Text Mode
- Eraser Mode
- Shape Mode
- Selection / Object Mode (future expansion)

Modes never switch automatically.  
All transitions are explicit.

---

## 4.2 Text System Architecture

StylusCore supports two distinct text paradigms.

### A) Free Text (Floating Text Blocks)

- Activated via Text Tool.
- Cursor changes to a "+" indicator.
- User draws a rectangular area (similar to drawing a text box).
- A temporary boundary appears during creation.
- After writing begins, the boundary becomes invisible.
- Text wraps downward when reaching horizontal bounds.
- It does not expand horizontally beyond defined width.
- The text block remains selectable and movable after creation.

This model supports flexible spatial layout similar to PowerPoint or OneNote.

---

### B) Document Text (Structured Text Mode)

This mode behaves like a traditional document editor (Word-style behavior):

- Clicking anywhere activates a writing cursor.
- Writing begins at a structured origin.
- Supports paragraphs, line breaks, indentation.
- Supports alignment (left, center, right).
- Supports bullet lists and ordered lists (future expansion).
- Supports structured formatting patterns.

This mode is designed for structured documentation inside a page.

---

## 4.3 Text Styling & Customization

Text supports:

- Font family
- Font size
- Font weight (bold, etc.)
- Italic / underline
- Text color
- Alignment
- Paragraph spacing (future expansion)

### Default & Favorite Profiles

Users may define:

- Preferred default font
- Preferred default size
- Preferred default styling

New text objects use the user’s default profile unless explicitly changed.

---

## 4.4 Ink System Architecture

Ink mode supports multiple pen types:

- Ballpoint
- Fountain pen
- Brush
- Marker
- Future brush engines

Configurable parameters:

- Color
- Thickness
- Opacity
- Stroke behavior
- Pressure sensitivity (future-ready)
- Stroke smoothing

Ink strokes are independent objects and do not merge into text logic.

---

## 4.5 Eraser System

The eraser is not a single tool.

Supported types (current and planned):

- Stroke Eraser (removes entire stroke)
- Point Eraser (partial erase)
- Object Eraser (removes selected object)
- Smart Eraser (future expansion)

Text and ink are logically separated.  
Erasing ink must not automatically delete structured text unless explicitly intended.

---

## 4.6 Shape System

Shapes are independent object types.

Planned shape support includes:

- Rectangle
- Ellipse
- Line
- Arrow
- Polygon
- Freeform shapes (future)

Shapes support:

- Fill color
- Border color
- Border thickness
- Opacity
- Transformations (future)

Shapes are editable, movable objects.

---

## 4.7 Radial Menu Philosophy

The radial menu is the primary rapid-access tool system.

It is:

- Mode-aware
- Fully customizable
- Preset-driven

Users can configure:

- Item order
- Size
- Radius
- Tool grouping
- Mode-specific radial layouts

Separate radial configurations may exist for:

- Mouse workflow
- Graphic tablet workflow

The radial system must not introduce hidden state changes.

---

## 4.8 Context Menu Philosophy

Right-click context menus exist across object types.

Behavior includes:

- Object-specific actions (delete, duplicate, transform)
- Text styling shortcuts
- Shape adjustments
- Tool-specific commands

Context menus should reflect professional document editor behavior (Word/PowerPoint style), but remain expandable and tool-aware.

---

## 4.9 Input Model

StylusCore is input-agnostic.

Supported input systems:

- Mouse + Keyboard
- Graphic Tablet
- Future: Touch / Gesture

All inputs must map to the same tool abstraction layer.

There must be:

- No implicit mode switching
- No automatic tool inference
- No ambiguous state transitions

---

## 4.10 Object Coexistence Model

The canvas supports multiple object types:

- Ink strokes
- Structured text
- Free text blocks
- Shapes
- Future embedded media

Each object type maintains:

- Its own logic
- Its own edit rules
- Its own erase behavior

Cross-type interference is prohibited by design.

---

## 5. Technical Direction

The architecture supports long-term extensibility:

- MVVM separation
- Feature encapsulation
- Resource-based styling
- Localization readiness
- Accessibility readiness
- Plugin-capable structure (future)

---

## 6. Completed Milestones

- Core Editor foundation
- Section / Page system
- Panel behavior refactoring
- Resource centralization
- Accessibility baseline integration

---

## 7. Short-Term Roadmap

- Free Text implementation
- Document Text structured system
- Text defaults & favorites
- Radial menu customization engine
- Ink engine refinement
- Eraser type expansion
- Context menu standardization

---

## 8. Long-Term Evolution

- Advanced brush engines
- Plugin-ready tool architecture
- AI-assisted tools (optional future)
- Gesture input
- Layer system (potential future)
- Advanced object grouping
- Power-user customization features

---

## 9. Development Governance Reference

All strict architectural and development rules are defined under the `.agent/` directory and must always be followed.

This document defines product direction and interaction philosophy only.  
Implementation must always comply with the rule system.
