---
description: StylusCore Project Guidelines and Rules (v6.5 - Universal Canvas + Icon Safety + Context Awareness + File Hierarchy)
---

# 📏 StylusCore Development Guidelines

This document establishes the **STRICT** rules for contributing to the StylusCore project. **All future development must adhere to these standards.**

The goal is to build a **"Super-App"** (like OneNote + draw.io + VS Code) where Ink, Text, Tables, Code Blocks, and Diagrams coexist on a high-performance **Infinite Canvas** with **Deep Zoom**.

---

## 🏗️ 1. Architectural & Layout Rules

### 1.1. Global Shell Layout (MainWindow)
* **Structure:** Grid-based layout.
* **Header:** Height **48px** (standard).
* **Sidebar (Rail):**
    * **Width (Collapsed):** `48px` or `64px`.
    * **Width (Expanded):** `240px` - `280px`.
* **Content Area:** Occupies the remaining space (`*`).

### 1.2. View Layout Strategy
* **Standard Views (Settings, Library):** Root is `Grid`.
* **Editor View (Note Surface):** Root is **Infinite Canvas**. NEVER use `ScrollViewer`. Use `MatrixTransform` for Pan/Zoom.

### 1.3. Page Concept
* **Concept:** A "Page" is a **Visual Region** on the Infinite Canvas, not a hard container. Content can overflow.

---

## 🖼️ 2. Iconography Rules (CLIP PROTECTION)

Icons must be **vector-based** and **never clipped**.

* **Format:** SVG Path Data string only (`StreamGeometry`).
* **Location:** `src/StylusCore.App/Shared/Themes/Icons.xaml`.
* **The "Fit-to-Box" Rule (CRITICAL):**
    * **Rule:** NEVER set explicit `Width` or `Height` on the `Path` element itself.
    * **Reason:** This causes clipping if the parent container is smaller.
    * **Correct Usage:** Let the Parent (`Grid` or `Border`) define the size.
* **Usage Pattern:**
    ```xml
    <Grid Width="24" Height="24">
        <Path Data="{StaticResource Icon.Pen}"
              Fill="{DynamicResource PrimaryTextBrush}"
              Stretch="Uniform"
              HorizontalAlignment="Center"
              VerticalAlignment="Center"/>
    </Grid>
    ```

---

## 🎨 3. Theming & Colors

* **Colors:** Always use `DynamicResource` keys defined in `Shared/Themes/`.
    * ✅ Correct: `Background="{DynamicResource PrimaryBackgroundBrush}"`
* **Fonts:** NEVER hardcode `FontFamily`. Use `DynamicResource App.MainFont`.
* **Standard Keys:** `PrimaryBackgroundBrush`, `PrimaryTextBrush`, `PrimaryAccentBrush`, `PrimaryBorderBrush`.

---

## 🌍 4. Localization & Strings

* **Rule:** Hardcoded text strings are **FORBIDDEN**.
* **Method:** Use `DynamicResource` to support runtime language switching.
    * ✅ Correct: `<TextBlock Text="{DynamicResource String.LibrarySettings.Title}"/>`

---

## 🧩 5. Mixed Media & Controls

### 5.1. The "View-to-Edit" Pattern
* **Rule:** Heavy controls (`DataGrid`, `RichTextBox`) are PROHIBITED on the Canvas for viewing.
* **Mechanism:**
    * **View Mode:** Render as **Vector Visual** (`DrawingVisual`) or lightweight `TextBlock`.
    * **Edit Mode:** Overlay real WPF Control on double-click.
    * **Exit:** Rasterize/Vectorize back to Visual on focus loss.

### 5.2. Smart Objects
* **Tables:** Draw lines using `DrawingContext` (Vector).
* **Code:** Use `FormattedText` or lightweight Border.

---

## 🖊️ 6. Ink Subsystem & "Deep Zoom"

### 6.1. StylusPlugIn Mandate
* **Rule:** Use custom `StylusPlugIn` on **Pen Thread**. No UI property access allowed.

### 6.2. Vector Retention (OneNote Quality)
* **Rule:** Use **`DrawingVisual`** for dry ink.
* **Ban:** DO NOT use `WriteableBitmap` (blurs on zoom) or `Path` objects (too heavy).

### 6.3. Input Arbitration
* **Stylus:** Ink. | **Touch:** Pan/Zoom. | **Mouse:** Select.

---

## ⚡ 7. Performance & Optimization

### 7.1. Navigation & Rendering
* **Zoom:** MUST use `MatrixTransform`.
* **Culling:** Use **QuadTree** to render only visible items.

### 7.2. Lists & Images
* **Lists:** `VirtualizationMode="Recycling"`.
* **Images:** `DecodePixelWidth` MUST be set to display size. Use `IsAsync=True`.

---

## 🧠 8. MVVM & Memory

* **Interaction:** Use `ICommand` (No Code-Behind).
* **Memory:** `.Freeze()` all Geometries/Brushes. Use `WeakEventManager` for events.

---

## 📂 9. Hierarchy & File Placement Strategy (RESTORED)

When creating a new file, follow this decision tree:

### 1. Is it a standalone Feature? (e.g., Export, CloudSync)
* **Path:** `src/StylusCore.App/Features/[FeatureName]/`
* **Contains:** `Views/` (XAML), `ViewModels/` (Logic)

### 2. Is it a reusable UI Component?
* **Path:** `src/StylusCore.App/Shared/Components/`

### 3. Is it a Pop-up / Dialog?
* **Path:** `src/StylusCore.App/Dialogs/`

### 4. Is it Core Logic?
* **Path:** `src/StylusCore.App/Core/Models/` or `Services/`

**CRITICAL:** Do NOT create new folders at the root of `src/StylusCore.App/`. Keep the root clean.

---

# 🛡️ 10. DETAILED TECHNICAL XAML RULES

## 10.1. COMPONENT CONSTRUCTION (Icon Safety)
* **Wrapper Rule:** Wrap interactive controls in a `Border`.
* **The "Zero-Padding" Rule:** For **Icon Buttons**, always set `Padding="0"`. Let the Grid/Border handle spacing.

## 10.2. ALIGNMENT LOGIC
* **Grid/Border:** `HorizontalAlignment="Stretch"`.
* **Content/Icon:** `HorizontalAlignment="Center"`, `VerticalAlignment="Center"`.

## 10.3. DIAGRAMMING (draw.io Logic)
* **Anchors:** Shapes must have semantic anchor points.
* **Geometry:** Use `PathGeometry` for all shapes.

---

# 🧩 11. COMPONENT-SPECIFIC BEST PRACTICES (RESTORED)

## 11.1. ScrollViewer & Scrolling
* **The "Nested Scroll" Ban:** NEVER place a `ListBox` inside a `ScrollViewer`. It kills virtualization.

## 11.2. Text & Input Controls
* **Text Wrapping:** For multi-line text, always set `TextWrapping="Wrap"`.
* **Search/Filter:** Use `UpdateSourceTrigger=PropertyChanged` with `Delay=300`.

## 11.3. DataGrid Optimization
* **Column Widths:** Avoid `Width="Auto"` on large datasets. Use `*` or fixed width.

---

# 🚀 12. HIGH-PERFORMANCE ENGINEERING

## 12.1. Layout Thrashing Mitigation
* **Rule:** NEVER modify `Canvas.Left` for frequent movement. Use `RenderTransform`.

## 12.2. Custom Drying Strategy
* **Rule:** Disable default InkCanvas drying (`ActivateCustomDrying`). Render to `DrawingVisual`.

---

# 🎮 13. VIEW-SPECIFIC SHELL BEHAVIOR (Context Awareness)

The application Shell must adapt based on `CurrentView`.

## 13.1. The "Context Switch" Rule
* **State: Library (LibraryView)** -> Sidebar shows `AllNotebooksList`. Ribbon shows `LibraryTools`.
* **State: Editor (NotebookView)** -> Sidebar shows `Pages/Sections`. Ribbon shows `InkingTools`. Canvas is Active.

## 13.2. State Preservation
* **Rule:** When navigating Back, clear previous selection to prevent "Ghost Data".