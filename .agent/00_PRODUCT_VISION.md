---
description: StylusCore Product Vision, Core Philosophy & Design DNA
---

# 🌌 StylusCore — Product Vision & Core Philosophy

> **This document defines the mind of StylusCore.**  
> Architecture, UI, input systems, feature design, and all future extensions  
> **MUST align with the principles defined here**.
>
> This file is the **Single Source of Truth** for developers, designers,  
> and all AI agents working on StylusCore.

---

## 0) Purpose (Why)

StylusCore goes beyond traditional note-taking software.  
It is a **stylus-first**, **infinite-canvas–based**, **deeply customizable**
space for thinking, producing, and organizing ideas.

The core goals are:
- Adapt the application to the user — **not the user to the application**
- Fully support graphic tablet users (Wacom / Huion / XP-Pen) for:
  - note-taking
  - diagramming
  - freeform thinking
- Deliver **power-user capabilities** without sacrificing usability

StylusCore is:
- not just a “note app”
- but a **thinking and production environment**

---

## 1) Core Philosophy

### 1.1 Stylus-First (Pen-Centric Design)

StylusCore is designed **around the pen**, not mouse or keyboard.

- Pen input is **the primary interaction model**
- Input latency is unacceptable
- Stylus buttons, holds, pressure, and gestures are first-class citizens
- Users coming from creative tools (Blender, ZBrush, CAD tools)
  must be able to use the same hardware naturally

#### Simultaneous Pen & Touch
- **Pen** → writing / drawing
- **Touch** → navigation only (pan / zoom / rotate)
- **Perfect palm rejection is mandatory**
- Pen and touch must work **simultaneously without conflict**

---

### 1.2 Extreme Customization

> “There are no wrong features — only unconfigured ones.”

- UI, input, shortcuts, modes, menus, scale, typography — everything is configurable
- Defaults are **starting points**, not constraints
- Users must be able to adapt StylusCore to their own mental model

---

## 2) Canvas & Page Model

StylusCore does **not** force users into a single canvas paradigm.

### 2.1 Hybrid Canvas Model

A project may use different canvas/page models:

#### Infinite Canvas
- Unbounded world space
- Ideal for:
  - diagrams
  - mind maps
  - freeform notes

#### Fixed Page Formats
- A4, A5, Letter
- Portrait / Landscape variants
- Optimized for print and export

Rules:
- Pages are **visual regions**, not hard containers
- Content may overflow page bounds
- The page system is **extensible**, not a fixed list

---

### 2.2 Paper & Background Customization

Each page or canvas region may use:

- Grid
- Lined
- Dotted (symmetric / asymmetric)
- Music staff
- Plain (blank)

Additional controls:
- Line density
- Background color
- Eye comfort is a priority

---

### 2.3 Custom Page Templates & Background Import

StylusCore supports **user-defined page templates**.

- Preferred format: **SVG**
- PNG may be supported at sufficiently high resolution

Imported templates:
- Are stored per-user
- Appear in a **Page Template Library**
- Can be reused across projects

Background composition:
- Base page color
- Overlay color
- Opacity
must work together and be user-adjustable

---

### 2.4 Export & Sharing Vision

User data must **never be locked into the application**.

- Fixed pages → PDF
- Infinite canvas →
  - high-resolution PNG
  - sliced PDF
  - SVG (diagram-oriented)

---

## 3) UI, Scale & Visual Consistency

### 3.1 DPI & Scale Awareness

StylusCore must work consistently across:
- 1080p / 2K / 4K displays
- Windows scaling (100% / 125% / 150%+)

Rules:
- Bitmap icons are forbidden
- Only vector icons (StreamGeometry)
- Layouts must be deterministic (no jitter)

---

### 3.2 Typography Freedom

- UI font is user-selectable
- Font size and density are configurable
- No font family or size may be hardcoded
- All values come from `DynamicResource`

---

### 3.3 Fluid UX & Micro-Interactions

- The UI must never feel static or rigid
- Smooth transitions are encouraged (Fluent-style)
- Micro-interactions should provide visual feedback:
  - button press
  - panel open/close
  - mode changes
- Animations must never compromise FPS or latency

---

## 4) Shell UI vs Editor Layer

### 4.1 Shell UI
Includes:
- Sidebar
- Library
- Settings
- Navigation
- Radial menu configuration

Rules:
- Must never affect editor performance
- Must remain lightweight and modular

---

### 4.2 Editor Layer
Includes:
- Canvas
- Ink engine
- Page layout
- Writing and drawing tools

Rules:
- Low latency is mandatory
- Must scale to large datasets
- Designed for long-running sessions

---

### 4.3 Contextual UI Principle

> **“Tools belong next to the work they operate on.”**

- The **Ribbon/Toolbar** belongs to the **Editor**, not the global window
- The **Sidebar** handles navigation, not drawing tools
- When entering the editor, tools appear
- When returning to the library, tools disappear

---

## 5) Interaction Modes (Mental Model)

StylusCore uses **behavior-driven modes**, not hardware-driven modes.

Modes include:
- Text Mode
- Ink Mode
- Shape / Diagram Mode
- Navigation Mode

Rules:
- A Mode is **not a UI panel**
- A Mode defines how input is interpreted

---

## 6) Radial Menu System

The Radial Menu is:
- not just UI
- but an **interaction system**

Rules:
- Each Mode may have one or more radials
- Users may create multiple radials
- Each radial is configurable:
  - item count
  - order
  - size
  - hold vs toggle behavior
  - assigned commands

---

## 7) Performance, Stability & Data Safety

Performance is not an optimization — it is a **design requirement**.

- Low-latency input
- UI thread must not block
- Memory usage must be controlled

### Zero Data Loss
- Data is safe the moment a stroke is made
- Crash recovery must restore **up to the last stroke**
- “Save” is psychological
- The system autosaves continuously in the background

---

## 8) Text Mode, Writing Models & Input Methods

### 8.1 Writing Models

Text Mode supports two writing models:

#### Free Text
- Click anywhere on the canvas
- Creates an independent text container
- Default for Infinite Canvas

#### Flow / Linear Text
- Only for fixed page formats (A4 / A5 / Letter)
- Word / OneNote–like behavior
- Paragraph flow and alignment supported

Rule:
- Flow Text is **intentionally disabled** on Infinite Canvas

---

### 8.2 Text Input Methods

Writing models are independent of input methods:

- Physical keyboard
- On-screen keyboard
- Voice (speech-to-text)

---

### 8.3 Inline Input Bar

When a caret is created:
- A small **Inline Input Bar** appears near it

Provides:
- 🎙️ Voice input
- ⌨️ Keyboard input

Voice behavior:
- Partial text may appear in a temporary style
- Final text commits to normal formatting

---

## 9) Voice Input & Whisper Integration

### 9.1 Whisper Usage

StylusCore targets a **Whisper-based** speech-to-text pipeline.

- Multi-language support
- Automatic language detection
- Mixed-language input (e.g., Turkish + English terms)

Default behavior:
- No manual language selection
- Detection is automatic

---

### 9.2 Voice Recording Modes

Supported modes:
- Push-to-talk (default)
- Toggle recording

Configurable via settings.

---

## 10) Settings Philosophy

Settings define **how the user works**, not just preferences.

Includes:
- Default input method
- Microphone device
- Push vs toggle
- Optional silence auto-stop

These settings integrate with:
- Text Mode
- Radial menus
- Inline Input Bar

---

## 11) Localization & Accessibility

- Multi-language support is mandatory
- UI strings must never be hardcoded
- Font, scale, and contrast must be configurable

---

## 12) Non-Goals

StylusCore is **not**:
- Mobile-first
- Cloud-dependent
- A social network
- A real-time collaborative editor

---

## 13) Golden Rule for Agents

> **No design or code decision may violate this document.**

All agents:
- before adding features
- before designing UI
- before proposing architecture

**MUST read this file first.**

---

## Final Note

This document defines:
- StylusCore’s character
- Its long-term direction
- The behavioral context for all AI agents

It may evolve,
but **the core philosophy must remain intact**.
